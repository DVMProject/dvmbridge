// SPDX-License-Identifier: AGPL-3.0-only
/**
* Digital Voice Modem - Audio Bridge
* AGPLv3 Open Source. Use is subject to license terms.
* DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
*
* @package DVM / Audio Bridge
* @license AGPLv3 License (https://opensource.org/licenses/AGPL-3.0)
*
*   Copyright (C) 2022-2024 Bryan Biedenkapp, N2PLL
*
*/
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq;

using Serilog;

using fnecore;
using fnecore.DMR;
using dvmbridge.MDC1200;

using vocoder;

using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace dvmbridge
{
    /// <summary>
    /// Represents the individual timeslot data status.
    /// </summary>
    public class SlotStatus
    {
        /// <summary>
        /// Rx Start Time
        /// </summary>
        public DateTime RxStart = DateTime.Now;
        
        /// <summary>
        /// 
        /// </summary>
        public uint RxSeq = 0;
        
        /// <summary>
        /// Rx RF Source
        /// </summary>
        public uint RxRFS = 0;
        /// <summary>
        /// Tx RF Source
        /// </summary>
        public uint TxRFS = 0;
        
        /// <summary>
        /// Rx Stream ID
        /// </summary>
        public uint RxStreamId = 0;
        /// <summary>
        /// Tx Stream ID
        /// </summary>
        public uint TxStreamId = 0;
        
        /// <summary>
        /// Rx TG ID
        /// </summary>
        public uint RxTGId = 0;
        /// <summary>
        /// Tx TG ID
        /// </summary>
        public uint TxTGId = 0;
        /// <summary>
        /// Tx Privacy TG ID
        /// </summary>
        public uint TxPITGId = 0;
        
        /// <summary>
        /// Rx Time
        /// </summary>
        public DateTime RxTime = DateTime.Now;
        /// <summary>
        /// Tx Time
        /// </summary>
        public DateTime TxTime = DateTime.Now;
        
        /// <summary>
        /// Rx Type
        /// </summary>
        public FrameType RxType = FrameType.TERMINATOR;
        
        /** DMR Data */
        /// <summary>
        /// Rx Link Control Header
        /// </summary>
        public LC DMR_RxLC = null;
        /// <summary>
        /// Rx Privacy Indicator Link Control Header
        /// </summary>
        public PrivacyLC DMR_RxPILC = null;
        /// <summary>
        /// Tx Link Control Header
        /// </summary>
        public LC DMR_TxHLC = null;
        /// <summary>
        /// Tx Privacy Link Control Header
        /// </summary>
        public PrivacyLC DMR_TxPILC = null;
        /// <summary>
        /// Tx Terminator Link Control
        /// </summary>
        public LC DMR_TxTLC = null;
    } // public class SlotStatus

    /// <summary>
    /// Implements a FNE system.
    /// </summary>
    public abstract partial class FneSystemBase : fnecore.FneSystemBase
    {
        private const string LOCAL_CALL = "Local Traffic";
        private const string UDP_CALL = "UDP Traffic";

        public abstract Task StartListeningAsync();

        private const int P25_FIXED_SLOT = 2;

        public const int SAMPLE_RATE = 8000;
        public const int BITS_PER_SECOND = 16;

        private const int MBE_SAMPLES_LENGTH = 160;

        private const int AUDIO_BUFFER_MS = 20;
        private const int AUDIO_NO_BUFFERS = 2;
        private const int AFSK_AUDIO_BUFFER_MS = 60;
        private const int AFSK_AUDIO_NO_BUFFERS = 4;

        private const int TX_MODE_DMR = 1;
        private const int TX_MODE_P25 = 2;

        private bool callInProgress = false;

        private SlotStatus[] status;

#if WIN32
        private AmbeVocoder extFullRateVocoder;
        private AmbeVocoder extHalfRateVocoder;
#endif

        private WaveFormat waveFormat;
        private BufferedWaveProvider waveProvider;

        private Task waveInRecorder;
        private WaveInEvent waveIn;
        private MDCWaveIn mdcProcessor;

        private Stopwatch dropAudio;
        private int dropTimeMs;
        bool audioDetect;
        bool trafficFromUdp;

        private BufferedWaveProvider meterInternalBuffer;
        private SampleChannel sampleChannel;
        private MeteringSampleProvider meterProvider;

        private WaveOut waveOut;

        private Random rand;
        private uint txStreamId;

        private uint srcIdOverride = 0;
        private uint udpSrcId = 0;
        private uint udpDstId = 0;

        private UdpClient udpClient;

        /*
        ** Methods
        */

        /// <summary>
        /// Initializes a new instance of the <see cref="FneSystemBase"/> class.
        /// </summary>
        /// <param name="fne">Instance of <see cref="FneMaster"/> or <see cref="FnePeer"/></param>
        public FneSystemBase(FnePeer fne) : base(fne, Program.FneLogLevel)
        {
            this.fne = fne;

            this.rand = new Random(Guid.NewGuid().GetHashCode());

            // initialize slot statuses
            this.status = new SlotStatus[3];
            this.status[0] = new SlotStatus();  // DMR Slot 1
            this.status[1] = new SlotStatus();  // DMR Slot 2
            this.status[2] = new SlotStatus();  // P25

            // hook logger callback
            this.fne.Logger = (LogLevel level, string message) =>
            {
                switch (level)
                {
                    case LogLevel.WARNING:
                        Log.Logger.Warning(message);
                        break;
                    case LogLevel.ERROR:
                        Log.Logger.Error(message);
                        break;
                    case LogLevel.DEBUG:
                        Log.Logger.Debug(message);
                        break;
                    case LogLevel.FATAL:
                        Log.Logger.Fatal(message);
                        break;
                    case LogLevel.INFO:
                    default:
                        Log.Logger.Information(message);
                        break;
                }
            };

            this.udpClient = new UdpClient();

            this.dropAudio = new Stopwatch();
            this.dropTimeMs = Program.Configuration.DropTimeMs;

            // "stuck" call (improperly ended call) checker thread
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    string trafficType = LOCAL_CALL;
                    if (trafficFromUdp)
                        trafficType = UDP_CALL;

                    // if we've exceeded the audio drop timeout, then really drop the audio
                    if ((dropAudio.IsRunning && (dropAudio.ElapsedMilliseconds > dropTimeMs * 2)) ||
                        (!dropAudio.IsRunning && !audioDetect && callInProgress))
                    {
                        if (audioDetect)
                        {
                            Log.Logger.Information($"({SystemName}) {trafficType} *CALL END (S)   * PEER {fne.PeerId} [STREAM ID {txStreamId}]");

                            audioDetect = false;
                            dropAudio.Reset();
#if !ENCODER_LOOPBACK_TEST
                            if (!callInProgress)
                            {
                                switch (Program.Configuration.TxMode)
                                {
                                    case TX_MODE_DMR:
                                        SendDMRTerminator();
                                        break;
                                    case TX_MODE_P25:
                                        SendP25TDU();
                                        break;
                                }
                            }
#endif
                            srcIdOverride = 0;
                            txStreamId = 0;

                            dropTimeMs = Program.Configuration.DropTimeMs;

                            udpSrcId = 0;
                            udpDstId = 0;
                            trafficFromUdp = false;
                        }
                    }

                    Thread.Sleep(5);
                }
            });

            this.audioDetect = false;
            this.trafficFromUdp = false;

            this.waveFormat = new WaveFormat(SAMPLE_RATE, BITS_PER_SECOND, 1);

            this.meterInternalBuffer = new BufferedWaveProvider(waveFormat);
            this.meterInternalBuffer.DiscardOnBufferOverflow = true;

            this.sampleChannel = new SampleChannel(meterInternalBuffer);
            this.meterProvider = new MeteringSampleProvider(sampleChannel);
            this.meterProvider.StreamVolume += MeterProvider_StreamVolume;

            // initialize the output audio provider
            if (Program.WaveOutDevice != -1)
            {
                this.waveOut = new WaveOut();
                this.waveOut.DeviceNumber = Program.WaveOutDevice;

                this.waveProvider = new BufferedWaveProvider(waveFormat) { DiscardOnBufferOverflow = true };
                this.waveOut.Init(waveProvider);
                this.waveOut.Play();
            }

            // initialize the primary input audio provider
            if (Program.WaveInDevice != -1)
            {
                waveInRecorder = Task.Factory.StartNew(() =>
                {
                    this.waveIn = new WaveInEvent();
                    this.waveIn.WaveFormat = waveFormat;
                    this.waveIn.DeviceNumber = Program.WaveInDevice;
                    this.waveIn.BufferMilliseconds = AUDIO_BUFFER_MS;
                    this.waveIn.NumberOfBuffers = AUDIO_NO_BUFFERS;

                    this.waveIn.DataAvailable += WaveIn_DataAvailable;

                    this.waveIn.StartRecording();

                    this.mdcProcessor = new MDCWaveIn(waveFormat, Program.WaveInDevice);
                    this.mdcProcessor.BufferMilliseconds = AFSK_AUDIO_BUFFER_MS;
                    this.mdcProcessor.NumberOfBuffers = AFSK_AUDIO_NO_BUFFERS;

                    if (Program.Configuration.DetectAnalogMDC1200)
                    {
                        this.mdcProcessor.MDCPacketDetected += MdcProcessor_MDCPacketDetected;
                        this.mdcProcessor.StartRecording();
                    }
                });
            }

            // initialize DMR vocoders
            dmrDecoder = new MBEDecoderManaged(MBEMode.DMRAMBE);
            dmrDecoder.GainAdjust = Program.Configuration.VocoderDecoderAudioGain;
            dmrDecoder.AutoGain = Program.Configuration.VocoderDecoderAutoGain;
            dmrEncoder = new MBEEncoderManaged(MBEMode.DMRAMBE);
            dmrEncoder.GainAdjust = Program.Configuration.VocoderEncoderAudioGain;

            // initialize P25 vocoders
            p25Decoder = new MBEDecoderManaged(MBEMode.IMBE);
            p25Decoder.GainAdjust = Program.Configuration.VocoderDecoderAudioGain;
            p25Decoder.AutoGain = Program.Configuration.VocoderDecoderAutoGain;
            p25Encoder = new MBEEncoderManaged(MBEMode.IMBE);
            p25Encoder.GainAdjust = Program.Configuration.VocoderEncoderAudioGain;
#if WIN32
            // initialize external AMBE vocoder
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);

            // if the assembly executing directory contains the external DVSI USB-3000 interface DLL
            // setup the external vocoder code
            if (File.Exists(Path.Combine(new string[] { Path.GetDirectoryName(path), "AMBE.DLL" })))
            {
                extFullRateVocoder = new AmbeVocoder();
                extHalfRateVocoder = new AmbeVocoder(false);
                Log.Logger.Information($"({SystemName}) Using external USB vocoder.");
            }
#endif
            embeddedData = new EmbeddedData();
            ambeBuffer = new byte[27];

            netLDU1 = new byte[9 * 25];
            netLDU2 = new byte[9 * 25];
        }

        /// <summary>
        /// Event that occurs when an MDC1200 frame is detected in the PCM audio.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="frameCount"></param>
        /// <param name="first"></param>
        /// <param name="second"></param>
        private void MdcProcessor_MDCPacketDetected(object sender, int frameCount, MDCPacket first, MDCPacket second)
        {
            if (first.Operation == OpType.PTT_ID)
            {
                if (Program.Configuration.OverrideSourceIdFromMDC)
                {
                    try
                    {
                        // do some nasty-nasty to convert MDC hex to integer
                        string txtSrcId = first.Target.ToString("X4");
                        srcIdOverride = Convert.ToUInt32(txtSrcId);
                        Log.Logger.Information($"({SystemName}) Local Traffic *MDC DETECT     * PEER {fne.PeerId} SRC_ID {srcIdOverride}");
                    }
                    catch (Exception) { /* stub */ }
                }
            }
        }

        /// <summary>
        /// Event that occurs when wave audio is detected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MeterProvider_StreamVolume(object sender, StreamVolumeEventArgs e)
        {
            float sampleLevel = Program.Configuration.VoxSampleLevel / 1000;

            FnePeer peer = (FnePeer)fne;
            uint srcId = (uint)Program.Configuration.SourceId;
            if (srcIdOverride != 0 && Program.Configuration.OverrideSourceIdFromMDC)
                srcId = srcIdOverride;

            uint dstId = (uint)Program.Configuration.DestinationId;

            string trafficType = LOCAL_CALL;
            if (trafficFromUdp)
            {
                srcId = udpSrcId;
                trafficType = UDP_CALL;
            }

            // handle Rx triggered by internal VOX
            if (e.MaxSampleValues[0] > sampleLevel)
            {
                audioDetect = true;
                if (txStreamId == 0)
                {
                    txStreamId = (uint)rand.Next(int.MinValue, int.MaxValue);
                    Log.Logger.Information($"({SystemName}) {trafficType} *CALL START     * PEER {fne.PeerId} SRC_ID {srcId} TGID {dstId} [STREAM ID {txStreamId}]");

                    if (Program.Configuration.GrantDemand)
                    {
                        switch (Program.Configuration.TxMode)
                        {
                            case TX_MODE_P25:
                                SendP25TDU(true);
                                break;
                        }
                    }
                }
                dropAudio.Reset();
            }
            else
            {
                // if we've exceeded the audio drop timeout, then really drop the audio
                if (dropAudio.IsRunning && (dropAudio.ElapsedMilliseconds > dropTimeMs))
                {
                    if (audioDetect)
                    {
                        Log.Logger.Information($"({SystemName}) {trafficType} *CALL END       * PEER {fne.PeerId} SRC_ID {srcId} TGID {dstId} [STREAM ID {txStreamId}]");

                        audioDetect = false;
                        dropAudio.Reset();
#if !ENCODER_LOOPBACK_TEST
                        if (!callInProgress)
                        {
                            switch (Program.Configuration.TxMode)
                            {
                                case TX_MODE_DMR:
                                    SendDMRTerminator();
                                    break;
                                case TX_MODE_P25:
                                    SendP25TDU();
                                    break;
                            }
                        }
#endif
                        srcIdOverride = 0;
                        txStreamId = 0;

                        dropTimeMs = Program.Configuration.DropTimeMs;

                        udpSrcId = 0;
                        udpDstId = 0;
                        trafficFromUdp = false;
                    }
                }

                if (!dropAudio.IsRunning)
                    dropAudio.Start();
            }
        }

        /// <summary>
        /// Event that occurs when wave audio data is available from the input device.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (!Program.Configuration.UdpAudio && Program.Configuration.LocalAudio)
            {
                int samples = SampleTimeConvert.MSToSampleBytes(waveFormat, AUDIO_BUFFER_MS);
                if (e.BytesRecorded == samples)
                {
                    // add samples to the metering buffer
                    meterInternalBuffer.AddSamples(e.Buffer, 0, e.BytesRecorded);

                    // trigger readback of metering buffer
                    float[] temp = new float[meterInternalBuffer.BufferedBytes];
                    this.meterProvider.Read(temp, 0, temp.Length);

                    if (audioDetect && !callInProgress)
                    {
                        switch (Program.Configuration.TxMode)
                        {
                            case TX_MODE_DMR:
                                DMREncodeAudioFrame(e.Buffer);
                                break;
                            case TX_MODE_P25:
                                P25EncodeAudioFrame(e.Buffer);
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Function that gets called when receiving audio from UDP
        /// </summary>
        /// <param name="receivedData"></param>
        public void ProcessAudioData(byte[] receivedData)
        {
            if (Program.Configuration.UdpAudio && !Program.Configuration.LocalAudio)
            {
                // Log.Logger.Debug($"UDP RECV BYTE BUFFER {FneUtils.HexDump(receivedData)}");

                int pcmLength = FneUtils.ToInt32(receivedData, 0);
                byte[] pcm = new byte[pcmLength];
                for (int idx = 0; idx < pcmLength; idx++)
                    pcm[idx] = receivedData[idx + 4];

                // Log.Logger.Debug($"PCM RECV BYTE BUFFER {FneUtils.HexDump(pcm)}");

                udpSrcId = (uint)Program.Configuration.SourceId;
                if (Program.Configuration.UdpMetaData)
                {
                    if (Program.Configuration.OverrideSourceIdFromUDP)
                        udpSrcId = FneUtils.ToUInt32(receivedData, pcmLength + 8);
                }

                udpDstId = (uint)Program.Configuration.DestinationId;

                // add audio samples to the metering buffer
                meterInternalBuffer.AddSamples(pcm, 0, pcm.Length);

                // read back metering buffer to check volume
                float[] temp = new float[meterInternalBuffer.BufferedBytes];
                meterProvider.Read(temp, 0, temp.Length);

                trafficFromUdp = true;

                // force start a call if one isn't already in progress
                if (!audioDetect && !callInProgress)
                {
                    audioDetect = true;
                    if (txStreamId == 0)
                    {
                        txStreamId = (uint)rand.Next(int.MinValue, int.MaxValue);
                        Log.Logger.Information($"({SystemName}) {UDP_CALL} *CALL START     * PEER {fne.PeerId} SRC_ID {udpSrcId} TGID {udpDstId} [STREAM ID {txStreamId}]");

                        if (Program.Configuration.GrantDemand)
                        {
                            switch (Program.Configuration.TxMode)
                            {
                                case TX_MODE_P25:
                                    SendP25TDU(true);
                                    break;
                            }
                        }
                    }
                    dropAudio.Reset();
                    dropTimeMs = Program.Configuration.DropTimeMs * 2; // double length drop time for UDP start

                    if (!dropAudio.IsRunning)
                        dropAudio.Start();
                }

                // If audio detection is active and no call is in progress, encode and transmit the audio
                if (audioDetect && !callInProgress)
                {
                    switch (Program.Configuration.TxMode)
                    {
                        case TX_MODE_DMR:
                            DMREncodeAudioFrame(pcm, udpSrcId);
                            break;
                        case TX_MODE_P25:
                            P25EncodeAudioFrame(pcm, udpSrcId);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Helper to generate a leader tone.
        /// </summary>
        private void GenerateLeaderTone()
        {
            SignalGenerator gen = new SignalGenerator(waveFormat.SampleRate, waveFormat.Channels)
            {
                Gain = 0.2f,
                Frequency = Program.Configuration.PreambleTone,
                Type = SignalGeneratorType.Sin
            };

            SampleToAudioProvider16 smpTo16 = new SampleToAudioProvider16(gen);
            int bufLen = SampleTimeConvert.MSToSampleBytes(waveFormat, Program.Configuration.PreambleLength);
            byte[] preambleBuf = new byte[bufLen];
            smpTo16.Read(preambleBuf, 0, bufLen);
            waveProvider.AddSamples(preambleBuf, 0, preambleBuf.Length);
        }

        /// <summary>
        /// Stops the main execution loop for this <see cref="FneSystemBase"/>.
        /// </summary>
        public override void Stop()
        {
            if (udpClient != null)
                udpClient.Dispose();
            
            ShutdownAudio();

            base.Stop();
        }

        /// <summary>
        /// Shuts down the audio resources.
        /// </summary>
        private void ShutdownAudio()
        {
            if (this.waveOut != null)
            {
                if (waveOut.PlaybackState == PlaybackState.Playing)
                    waveOut.Stop();
                waveOut.Dispose();
                waveOut = null;
            }

            if (waveInRecorder != null)
            {
                if (this.mdcProcessor != null)
                {
                    mdcProcessor.StopRecording();
                    mdcProcessor.Dispose();
                    mdcProcessor = null;
                }

                if (this.waveIn != null)
                {
                    waveIn.StopRecording();
                    waveIn.Dispose();
                    waveIn = null;
                }

                try
                {
                    waveInRecorder.GetAwaiter().GetResult();
                }
                catch (OperationCanceledException) { /* stub */ }
            }
        }

        /// <summary>
        /// Callback used to process whether or not a peer is being ignored for traffic.
        /// </summary>
        /// <param name="peerId">Peer ID</param>
        /// <param name="srcId">Source Address</param>
        /// <param name="dstId">Destination Address</param>
        /// <param name="slot">Slot Number</param>
        /// <param name="callType">Call Type (Group or Private)</param>
        /// <param name="frameType">Frame Type</param>
        /// <param name="dataType">DMR Data Type</param>
        /// <param name="streamId">Stream ID</param>
        /// <returns>True, if peer is ignored, otherwise false.</returns>
        protected override bool PeerIgnored(uint peerId, uint srcId, uint dstId, byte slot, CallType callType, FrameType frameType, DMRDataType dataType, uint streamId)
        {
            return false;
        }

        /// <summary>
        /// Event handler used to handle a peer connected event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void PeerConnected(object sender, PeerConnectedEvent e)
        {
            return;
        }
    } // public abstract partial class FneSystemBase : fnecore.FneSystemBase
} // namespace dvmbridge

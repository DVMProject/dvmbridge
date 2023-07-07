/**
* Digital Voice Modem - Bridge
* AGPLv3 Open Source. Use is subject to license terms.
* DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
*
* @package DVM / Bridge
*
*/
/*
*   Copyright (C) 2022-2023 by Bryan Biedenkapp N2PLL
*
*   This program is free software: you can redistribute it and/or modify
*   it under the terms of the GNU Affero General Public License as published by
*   the Free Software Foundation, either version 3 of the License, or
*   (at your option) any later version.
*
*   This program is distributed in the hope that it will be useful,
*   but WITHOUT ANY WARRANTY; without even the implied warranty of
*   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*   GNU Affero General Public License for more details.
*/

using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Serilog;

using dvmbridge.FNE;
using dvmbridge.FNE.DMR;

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
    public abstract partial class FneSystemBase
    {
        private const int P25_FIXED_SLOT = 2;

        public const int SAMPLE_RATE = 8000;
        public const int BITS_PER_SECOND = 16;

        private const int AUDIO_BUFFER_MS = 20;
        private const int AUDIO_NO_BUFFERS = 2;
        private const int DMR_AUDIO_DROP_MS = 60;
        private const int P25_AUDIO_DROP_MS = 180;

        private const int TX_MODE_DMR = 1;
        private const int TX_MODE_P25 = 2;

        protected FneBase fne;

        private SlotStatus[] status;

        private WaveFormat waveFormat;                 //
        private BufferedWaveProvider waveProvider;     //

        private Task waveInRecorder;
        private WaveInEvent waveIn;

        private Stopwatch dropAudio;
        bool audioDetect;

        private BufferedWaveProvider meterInternalBuffer;
        private SampleChannel sampleChannel;
        private MeteringSampleProvider meterProvider;

        private WaveOut waveOut;

        private Random rand;
        private uint txStreamId;

        /*
        ** Properties
        */

        /// <summary>
        /// Gets the system name for this <see cref="FneSystemBase"/>.
        /// </summary>
        public string SystemName
        {
            get
            {
                if (fne != null)
                    return fne.SystemName;
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the peer ID for this <see cref="FneSystemBase"/>.
        /// </summary>
        public uint PeerId
        {
            get
            {
                if (fne != null)
                    return fne.PeerId;
                return uint.MaxValue;
            }
        }

        /// <summary>
        /// Flag indicating whether this <see cref="FneSystemBase"/> is running.
        /// </summary>
        public bool IsStarted
        { 
            get
            {
                if (fne != null)
                    return fne.IsStarted;
                return false;
            }
        }

        /// <summary>
        /// Gets the <see cref="FneType"/> this <see cref="FneBase"/> is.
        /// </summary>
        public FneType FneType
        {
            get
            {
                if (fne != null)
                    return fne.FneType;
                return FneType.UNKNOWN;
            }
        }

        /*
        ** Methods
        */

        /// <summary>
        /// Initializes a new instance of the <see cref="FneSystemBase"/> class.
        /// </summary>
        /// <param name="fne">Instance of <see cref="FneMaster"/> or <see cref="FnePeer"/></param>
        public FneSystemBase(FneBase fne)
        {
            this.fne = fne;

            this.rand = new Random(Guid.NewGuid().GetHashCode());

            // initialize slot statuses
            this.status = new SlotStatus[3];
            this.status[0] = new SlotStatus();  // DMR Slot 1
            this.status[1] = new SlotStatus();  // DMR Slot 2
            this.status[2] = new SlotStatus();  // P25

            // hook various FNE network callbacks
            this.fne.DMRDataValidate = DMRDataValidate;
            this.fne.DMRDataReceived += DMRDataReceived;

            this.fne.P25DataValidate = P25DataValidate;
            this.fne.P25DataPreprocess += P25DataPreprocess;
            this.fne.P25DataReceived += P25DataReceived;

            this.fne.NXDNDataValidate = NXDNDataValidate;
            this.fne.NXDNDataReceived += NXDNDataReceived;

            this.fne.PeerIgnored = PeerIgnored;
            this.fne.PeerConnected += PeerConnected;

            // hook logger callback
            this.fne.LogLevel = Program.FneLogLevel;
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

            this.dropAudio = new Stopwatch();
            this.audioDetect = false;

            this.waveFormat = new WaveFormat(SAMPLE_RATE, BITS_PER_SECOND, 1);

            // initialize the output audio provider
            this.waveOut = new WaveOut();
            this.waveOut.DeviceNumber = Program.WaveOutDevice;

            this.waveProvider = new BufferedWaveProvider(waveFormat) { DiscardOnBufferOverflow = true };
            this.waveOut.Init(waveProvider);
            this.waveOut.Play();

            // initialize the primary input audio provider
            if (Program.WaveInDevice != -1)
            {
                this.meterInternalBuffer = new BufferedWaveProvider(waveFormat);
                this.meterInternalBuffer.DiscardOnBufferOverflow = true;
                this.sampleChannel = new SampleChannel(meterInternalBuffer);
                this.meterProvider = new MeteringSampleProvider(sampleChannel);
                this.meterProvider.StreamVolume += MeterProvider_StreamVolume;

                waveInRecorder = Task.Factory.StartNew(() =>
                {
                    this.waveIn = new WaveInEvent();
                    this.waveIn.WaveFormat = waveFormat;
                    this.waveIn.DeviceNumber = Program.WaveInDevice;
                    this.waveIn.BufferMilliseconds = AUDIO_BUFFER_MS;
                    this.waveIn.NumberOfBuffers = AUDIO_NO_BUFFERS;

                    this.waveIn.DataAvailable += WaveIn_DataAvailable;

                    this.waveIn.StartRecording();
                });
            }

            // initialize DMR vocoders
            dmrDecoder = new MBEDecoderManaged(MBEMode.DMRAMBE);
            dmrDecoder.GainAdjust = Program.Configuration.AudioGain;
            dmrEncoder = new MBEEncoderManaged(MBEMode.DMRAMBE);
            dmrEncoder.GainAdjust = Program.Configuration.AudioGain;

            // initialize P25 vocoders
            p25Decoder = new MBEDecoderManaged(MBEMode.IMBE);
            p25Decoder.GainAdjust = Program.Configuration.AudioGain;
            p25Encoder = new MBEEncoderManaged(MBEMode.IMBE);
            p25Encoder.GainAdjust = Program.Configuration.AudioGain;

            embeddedData = new EmbeddedData();
            ambeBuffer = new byte[27];

            netLDU1 = new byte[9 * 25];
            netLDU2 = new byte[9 * 25];
        }

        /// <summary>
        /// Event that occurs when wave audio is detected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MeterProvider_StreamVolume(object sender, StreamVolumeEventArgs e)
        {
            // handle Rx triggered by internal VOX
            if (e.MaxSampleValues[0] > -40.0f)
            {
                audioDetect = true;
                txStreamId = (uint)rand.Next(int.MinValue, int.MaxValue);
                dropAudio.Reset();
            }
            else
            {
                // if we've exceeded the audio drop timeout, then really drop the audio
                int dropTimeMs = (Program.Configuration.TxMode == TX_MODE_P25) ? P25_AUDIO_DROP_MS : DMR_AUDIO_DROP_MS;
                if (dropAudio.IsRunning && (dropAudio.ElapsedMilliseconds > dropTimeMs))
                {
                    audioDetect = false;
                    dropAudio.Reset();

                    txStreamId = 0;
                    p25N = 0;
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
            int samples = SampleTimeConvert.MSToSampleBytes(waveFormat, AUDIO_BUFFER_MS);
            if (e.BytesRecorded == samples)
            {
                // add samples to the metering buffer
                meterInternalBuffer.AddSamples(e.Buffer, 0, e.BytesRecorded);

                // trigger readback of metering buffer
                float[] temp = new float[meterInternalBuffer.BufferedBytes];
                this.meterProvider.Read(temp, 0, temp.Length);
/*
                if (audioDetect)
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
*/
            }
        }

        /// <summary>
        /// Starts the main execution loop for this <see cref="FneSystemBase"/>.
        /// </summary>
        public void Start()
        {
            if (!fne.IsStarted)
                fne.Start();
        }

        /// <summary>
        /// Stops the main execution loop for this <see cref="FneSystemBase"/>.
        /// </summary>
        public void Stop()
        {
            ShutdownAudio();
            if (fne.IsStarted)
                fne.Stop();
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
        protected virtual bool PeerIgnored(uint peerId, uint srcId, uint dstId, byte slot, CallType callType, FrameType frameType, DMRDataType dataType, uint streamId)
        {
            return false;
        }

        /// <summary>
        /// Event handler used to handle a peer connected event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void PeerConnected(object sender, PeerConnectedEvent e)
        {
            return;
        }
    } // public abstract partial class FneSystemBase
} // namespace dvmbridge

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
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Serilog;

using fnecore;
using fnecore.DMR;

using NAudio.Wave;

using vocoder;

namespace dvmbridge
{
    /// <summary>
    /// Implements a FNE system base.
    /// </summary>
    public abstract partial class FneSystemBase : fnecore.FneSystemBase
    {
        private const int AMBE_BUF_LEN = 9;

        private const int DMR_AMBE_LENGTH_BYTES = 27;
        private const int AMBE_PER_SLOT = 3;

        private MBEDecoderManaged dmrDecoder;
        private MBEEncoderManaged dmrEncoder;

        private EmbeddedData embeddedData;

        private byte[] ambeBuffer;
        private int ambeCount = 0;
        private int dmrSeqNo = 0;
        private byte dmrN = 0;

        /*
        ** Methods
        */

        /// <summary>
        /// Callback used to validate incoming DMR data.
        /// </summary>
        /// <param name="peerId">Peer ID</param>
        /// <param name="srcId">Source Address</param>
        /// <param name="dstId">Destination Address</param>
        /// <param name="slot">Slot Number</param>
        /// <param name="callType">Call Type (Group or Private)</param>
        /// <param name="frameType">Frame Type</param>
        /// <param name="dataType">DMR Data Type</param>
        /// <param name="streamId">Stream ID</param>
        /// <param name="message">Raw message data</param>
        /// <returns>True, if data stream is valid, otherwise false.</returns>
        protected override bool DMRDataValidate(uint peerId, uint srcId, uint dstId, byte slot, CallType callType, FrameType frameType, DMRDataType dataType, uint streamId, byte[] message)
        {
            return true;
        }

        /// <summary>
        /// Creates an DMR frame message.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="frameType"></param>
        /// <param name="n"></param>
        private void CreateDMRMessage(ref byte[] data, FrameType frameType, byte seqNo, byte n)
        {
            RemoteCallData callData = new RemoteCallData()
            {
                SrcId = (uint)Program.Configuration.SourceId,
                DstId = (uint)Program.Configuration.DestinationId,
                FrameType = frameType,
                Slot = (byte)Program.Configuration.Slot
            };

            CreateDMRMessage(ref data, callData, seqNo, n);
        }

        /// <summary>
        /// Helper to send a DMR terminator with LC message.
        /// </summary>
        private void SendDMRTerminator()
        {
            uint srcId = (uint)Program.Configuration.SourceId;
            if (srcIdOverride != 0 && Program.Configuration.OverrideSourceIdFromMDC)
                srcId = srcIdOverride;
            uint dstId = (uint)Program.Configuration.DestinationId;

            RemoteCallData callData = new RemoteCallData()
            {
                SrcId = srcId,
                DstId = dstId,
                FrameType = FrameType.DATA_SYNC,
                Slot = (byte)Program.Configuration.Slot
            };

            SendDMRTerminator(callData, ref dmrSeqNo, ref dmrN, embeddedData);
            ambeCount = 0;
        }

        /// <summary>
        /// Helper to encode and transmit PCM audio as DMR AMBE frames.
        /// </summary>
        /// <param name="pcm"></param>
        /// <param name="forcedSrcId"></param>
        /// <param name="forcedDstId"></param>
        private void DMREncodeAudioFrame(byte[] pcm, uint forcedSrcId = 0, uint forcedDstId = 0)
        {
            uint srcId = (uint)Program.Configuration.SourceId;
            if (srcIdOverride != 0 && Program.Configuration.OverrideSourceIdFromMDC)
                srcId = srcIdOverride;
            if (forcedSrcId > 0 && forcedSrcId != (uint)Program.Configuration.SourceId)
                srcId = forcedSrcId;
            uint dstId = (uint)Program.Configuration.DestinationId;
            if (forcedDstId > 0 && forcedSrcId != (uint)Program.Configuration.DestinationId)
                dstId = forcedDstId;

            byte slot = (byte)Program.Configuration.Slot;
#if ENCODER_LOOPBACK_TEST
            if (ambeCount == AMBE_PER_SLOT)
            {
                for (int n = 0; n < AMBE_PER_SLOT; n++)
                {
                    byte[] ambePartial = new byte[AMBE_BUF_LEN];
                    for (int i = 0; i < AMBE_BUF_LEN; i++)
                        ambePartial[i] = ambeBuffer[i + (n * 9)];

                    short[] samp = null;
                    int errs = dmrDecoder.decode(ambePartial, out samp);
                    if (samp != null)
                    {
                        Log.Logger.Debug($"LOOPBACK_TEST PARTIAL AMBE {FneUtils.HexDump(ambePartial)}");
                        Log.Logger.Debug($"LOOPBACK_TEST SAMPLE BUFFER {FneUtils.HexDump(samp)}");

                        int pcmIdx = 0;
                        byte[] pcm2 = new byte[samp.Length * 2];
                        for (int smpIdx2 = 0; smpIdx2 < samp.Length; smpIdx2++)
                        {
                            pcm2[pcmIdx + 0] = (byte)(samp[smpIdx2] & 0xFF);
                            pcm2[pcmIdx + 1] = (byte)((samp[smpIdx2] >> 8) & 0xFF);
                            pcmIdx += 2;
                        }

                        Log.Logger.Debug($"LOOPBACK_TEST BYTE BUFFER {FneUtils.HexDump(pcm)}");
                        waveProvider.AddSamples(pcm2, 0, pcm2.Length);
                    }
                }

                FneUtils.Memset(ambeBuffer, 0, 27);
                ambeCount = 0;
            }
#else
            byte[] data = null, dmrpkt = null;
            dmrN = (byte)(dmrSeqNo % 6);
            if (ambeCount == AMBE_PER_SLOT)
            {
                FnePeer peer = (FnePeer)fne;
                ushort pktSeq = 0;

                // is this the intitial sequence?
                if (dmrSeqNo == 0)
                {
                    pktSeq = peer.pktSeq(true);

                    // send DMR voice header
                    data = new byte[DMR_FRAME_LENGTH_BYTES];

                    // generate DMR LC
                    LC dmrLC = new LC();
                    dmrLC.FLCO = (byte)DMRFLCO.FLCO_GROUP;
                    dmrLC.SrcId = srcId;
                    dmrLC.DstId = dstId;
                    embeddedData.SetLC(dmrLC);

                    // generate the Slot TYpe
                    SlotType slotType = new SlotType();
                    slotType.DataType = (byte)DMRDataType.VOICE_LC_HEADER;
                    slotType.GetData(ref data);

                    FullLC.Encode(dmrLC, ref data, DMRDataType.VOICE_LC_HEADER);

                    // generate DMR network frame
                    dmrpkt = new byte[DMR_PACKET_SIZE];
                    CreateDMRMessage(ref dmrpkt, FrameType.VOICE_SYNC, (byte)dmrSeqNo, 0);
                    Buffer.BlockCopy(data, 0, dmrpkt, 20, DMR_FRAME_LENGTH_BYTES);

                    peer.SendMaster(new Tuple<byte, byte>(Constants.NET_FUNC_PROTOCOL, Constants.NET_PROTOCOL_SUBFUNC_DMR), dmrpkt, pktSeq, txStreamId);

                    dmrSeqNo++;
                }

                pktSeq = peer.pktSeq();

                // send DMR voice
                data = new byte[DMR_FRAME_LENGTH_BYTES];

                Buffer.BlockCopy(ambeBuffer, 0, data, 0, 13);
                data[13U] = (byte)(ambeBuffer[13U] & 0xF0);
                data[19U] = (byte)(ambeBuffer[13U] & 0x0F);
                Buffer.BlockCopy(ambeBuffer, 14, data, 20, 13);

                FrameType frameType = FrameType.VOICE_SYNC;
                if (dmrN == 0)
                    frameType = FrameType.VOICE_SYNC;
                else
                {
                    frameType = FrameType.VOICE;

                    byte lcss = embeddedData.GetData(ref data, dmrN);

                    // generated embedded signalling
                    EMB emb = new EMB();
                    emb.ColorCode = 0;
                    emb.LCSS = lcss;
                    emb.Encode(ref data);
                }

                Log.Logger.Information($"({SystemName}) DMRD: Traffic *VOICE FRAME    * PEER {fne.PeerId} SRC_ID {srcId} TGID {dstId} TS {slot} VC{dmrN} [STREAM ID {txStreamId}]");

                // generate DMR network frame
                dmrpkt = new byte[DMR_PACKET_SIZE];
                CreateDMRMessage(ref dmrpkt, frameType, (byte)dmrSeqNo, dmrN);
                Buffer.BlockCopy(data, 0, dmrpkt, 20, DMR_FRAME_LENGTH_BYTES);

                peer.SendMaster(new Tuple<byte, byte>(Constants.NET_FUNC_PROTOCOL, Constants.NET_PROTOCOL_SUBFUNC_DMR), dmrpkt, pktSeq, txStreamId);

                dmrSeqNo++;

                FneUtils.Memset(ambeBuffer, 0, 27);
                ambeCount = 0;
            }
#endif
            // Log.Logger.Debug($"BYTE BUFFER {FneUtils.HexDump(pcm)}");

            // pre-process: apply gain to PCM audio frames
            if (Program.Configuration.TxAudioGain != 1.0f)
            {
                BufferedWaveProvider buffer = new BufferedWaveProvider(waveFormat);
                buffer.AddSamples(pcm, 0, pcm.Length);

                VolumeWaveProvider16 gainControl = new VolumeWaveProvider16(buffer);
                gainControl.Volume = Program.Configuration.TxAudioGain;
                gainControl.Read(pcm, 0, pcm.Length);
            }

            int smpIdx = 0;
            short[] samples = new short[MBE_SAMPLES_LENGTH];
            for (int pcmIdx = 0; pcmIdx < pcm.Length; pcmIdx += 2)
            {
                samples[smpIdx] = (short)((pcm[pcmIdx + 1] << 8) + pcm[pcmIdx + 0]);
                smpIdx++;
            }

            // Log.Logger.Debug($"SAMPLE BUFFER {FneUtils.HexDump(samples)}");

            // encode PCM samples into AMBE codewords
            byte[] ambe = null;
#if WIN32
            if (extHalfRateVocoder != null)
                extHalfRateVocoder.encode(samples, out ambe, true);
            else
                dmrEncoder.encode(samples, out ambe);
#else
            dmrEncoder.encode(samples, out ambe);
#endif
            // Log.Logger.Debug($"AMBE {FneUtils.HexDump(ambe)}");

            Buffer.BlockCopy(ambe, 0, ambeBuffer, ambeCount * 9, AMBE_BUF_LEN);
            ambeCount++;
        }

        /// <summary>
        /// Helper to decode and playback DMR AMBE frames as PCM audio.
        /// </summary>
        /// <param name="ambe"></param>
        /// <param name="e"></param>
        private void DMRDecodeAudioFrame(byte[] ambe, DMRDataReceivedEvent e)
        {
            try
            {
                // Log.Logger.Debug($"FULL AMBE {FneUtils.HexDump(ambe)}");
                for (int n = 0; n < AMBE_PER_SLOT; n++)
                {
                    byte[] ambePartial = new byte[AMBE_BUF_LEN];
                    for (int i = 0; i < AMBE_BUF_LEN; i++)
                        ambePartial[i] = ambe[i + (n * 9)];

                    short[] samples = null;
                    int errs = 0;
#if WIN32
                    if (extHalfRateVocoder != null)
                        errs = extHalfRateVocoder.decode(ambePartial, out samples);
                    else
                        errs = dmrDecoder.decode(ambePartial, out samples);
#else
                    errs = dmrDecoder.decode(ambePartial, out samples);
#endif

                    if (samples != null)
                    {
                        Log.Logger.Information($"({SystemName}) DMRD: Traffic *VOICE FRAME    * PEER {e.PeerId} SRC_ID {e.SrcId} TGID {e.DstId} TS {e.Slot + 1} VC{e.n}.{n} ERRS {errs} [STREAM ID {e.StreamId}]");
                        // Log.Logger.Debug($"PARTIAL AMBE {FneUtils.HexDump(ambePartial)}");
                        // Log.Logger.Debug($"SAMPLE BUFFER {FneUtils.HexDump(samples)}");

                        int pcmIdx = 0;
                        byte[] pcm = new byte[samples.Length * 2];
                        for (int smpIdx = 0; smpIdx < samples.Length; smpIdx++)
                        {
                            pcm[pcmIdx + 0] = (byte)(samples[smpIdx] & 0xFF);
                            pcm[pcmIdx + 1] = (byte)((samples[smpIdx] >> 8) & 0xFF);
                            pcmIdx += 2;
                        }

                        // post-process: apply gain to decoded audio frames
                        if (Program.Configuration.RxAudioGain != 1.0f)
                        {
                            BufferedWaveProvider buffer = new BufferedWaveProvider(waveFormat);
                            buffer.AddSamples(pcm, 0, pcm.Length);

                            VolumeWaveProvider16 gainControl = new VolumeWaveProvider16(buffer);
                            gainControl.Volume = Program.Configuration.RxAudioGain;
                            gainControl.Read(pcm, 0, pcm.Length);
                        }

                        // Log.Logger.Debug($"PCM BYTE BUFFER {FneUtils.HexDump(pcm)}");
                        if (Program.Configuration.LocalAudio)
                            waveProvider.AddSamples(pcm, 0, pcm.Length);

                        if (Program.Configuration.UdpAudio)
                        {
                            byte[] audioData = null;
                            if (!Program.Configuration.UdpMetaData)
                            {
                                audioData = new byte[pcm.Length + 4]; // PCM + 4 bytes (PCM length)
                                FneUtils.WriteBytes(pcm.Length, ref audioData, 0);
                                for (int idx = 0; idx < pcm.Length; idx++)
                                    audioData[idx + 4] = pcm[idx];
                            }
                            else
                            {
                                audioData = new byte[pcm.Length + 12]; // PCM + (4 bytes (PCM length) + 4 bytes (srcId) + 4 bytes (dstId))
                                FneUtils.WriteBytes(pcm.Length, ref audioData, 0);
                                for (int idx = 0; idx < pcm.Length; idx++)
                                    audioData[idx + 4] = pcm[idx];

                                // embed destination ID
                                FneUtils.WriteBytes(e.DstId, ref audioData, pcm.Length + 4);

                                // embed source ID
                                FneUtils.WriteBytes(e.SrcId, ref audioData, pcm.Length + 8);
                            }

                            // Log.Logger.Debug($"UDP SEND BYTE BUFFER {FneUtils.HexDump(audioData)}");

                            IPAddress destinationIP = IPAddress.Parse(Program.Configuration.UdpSendAddress);
                            udpClient.Send(audioData, audioData.Length, new IPEndPoint(destinationIP, Program.Configuration.UdpSendPort));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Audio Decode Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Event handler used to process incoming DMR data.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void DMRDataReceived(object sender, DMRDataReceivedEvent e)
        {
            DateTime pktTime = DateTime.Now;

            if (Program.Configuration.TxMode != TX_MODE_DMR)
                return;

            byte[] data = new byte[DMR_FRAME_LENGTH_BYTES];
            Buffer.BlockCopy(e.Data, 20, data, 0, DMR_FRAME_LENGTH_BYTES);
            byte bits = e.Data[15];

            if (e.CallType == CallType.GROUP)
            {
                if (e.SrcId == 0)
                {
                    Log.Logger.Warning($"({SystemName}) DMRD: Received call from SRC_ID {e.SrcId}? Dropping call data.");
                    return;
                }

                // ensure destination ID and slot matches
                if (e.DstId != Program.Configuration.DestinationId && e.Slot != (byte)Program.Configuration.Slot)
                    return;

                // is this a new call stream?
                if (e.StreamId != status[e.Slot].RxStreamId)
                {
                    callInProgress = true;
                    status[e.Slot].RxStart = pktTime;
                    Log.Logger.Information($"({SystemName}) DMRD: Traffic *CALL START     * PEER {e.PeerId} SRC_ID {e.SrcId} TGID {e.DstId} [STREAM ID {e.StreamId}]");
                    if (Program.Configuration.PreambleLeaderTone)
                        GenerateLeaderTone();

                    // if we can, use the LC from the voice header as to keep all options intact
                    if ((e.FrameType == FrameType.DATA_SYNC) && (e.DataType == DMRDataType.VOICE_LC_HEADER))
                    {
                        LC lc = FullLC.Decode(data, DMRDataType.VOICE_LC_HEADER);
                        status[e.Slot].DMR_RxLC = lc;
                    }
                    else // if we don't have a voice header; don't wait to decode it, just make a dummy header
                        status[e.Slot].DMR_RxLC = new LC()
                        {
                            SrcId = e.SrcId,
                            DstId = e.DstId
                        };

                    status[e.Slot].DMR_RxPILC = new PrivacyLC();
                    Log.Logger.Debug($"({SystemName}) TS {e.Slot + 1} [STREAM ID {e.StreamId}] RX_LC {FneUtils.HexDump(status[e.Slot].DMR_RxLC.GetBytes())}");
                }

                // if we can, use the PI LC from the PI voice header as to keep all options intact
                if ((e.FrameType == FrameType.DATA_SYNC) && (e.DataType == DMRDataType.VOICE_PI_HEADER))
                {
                    PrivacyLC lc = FullLC.DecodePI(data);
                    status[e.Slot].DMR_RxPILC = lc;
                    Log.Logger.Information($"({SystemName}) DMRD: Traffic *CALL PI PARAMS  * PEER {e.PeerId} DST_ID {e.DstId} TS {e.Slot + 1} ALGID {lc.AlgId} KID {lc.KId} [STREAM ID {e.StreamId}]");
                    Log.Logger.Debug($"({SystemName}) TS {e.Slot + 1} [STREAM ID {e.StreamId}] RX_PI_LC {FneUtils.HexDump(status[e.Slot].DMR_RxPILC.GetBytes())}");
                }

                if ((e.FrameType == FrameType.DATA_SYNC) && (e.DataType == DMRDataType.TERMINATOR_WITH_LC) && (status[e.Slot].RxType != FrameType.TERMINATOR))
                {
                    callInProgress = false;
                    TimeSpan callDuration = pktTime - status[0].RxStart;
                    Log.Logger.Information($"({SystemName}) DMRD: Traffic *CALL END       * PEER {e.PeerId} SRC_ID {e.SrcId} TGID {e.DstId} DUR {callDuration} [STREAM ID {e.StreamId}]");
                }

                if (e.FrameType == FrameType.VOICE_SYNC || e.FrameType == FrameType.VOICE)
                {
                    byte[] ambe = new byte[DMR_AMBE_LENGTH_BYTES];
                    Buffer.BlockCopy(data, 0, ambe, 0, 14);
                    ambe[13] &= 0xF0;
                    ambe[13] |= (byte)(data[19] & 0x0F);
                    Buffer.BlockCopy(data, 20, ambe, 14, 13);
                    DMRDecodeAudioFrame(ambe, e);
                }

                status[e.Slot].RxRFS = e.SrcId;
                status[e.Slot].RxType = e.FrameType;
                status[e.Slot].RxTGId = e.DstId;
                status[e.Slot].RxTime = pktTime;
                status[e.Slot].RxStreamId = e.StreamId;
            }

            return;
        }
    } // public abstract partial class FneSystemBase : fnecore.FneSystemBase
} // namespace dvmbridge

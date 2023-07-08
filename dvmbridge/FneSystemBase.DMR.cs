/**
* Digital Voice Modem - Bridge
* AGPLv3 Open Source. Use is subject to license terms.
* DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
*
* @package DVM / Bridge
*
*/
/*
*   Copyright (C) 2023 by Bryan Biedenkapp N2PLL
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
using System.Collections.Generic;
using System.Threading;

using Serilog;

using dvmbridge.FNE;
using dvmbridge.FNE.DMR;

using vocoder;

namespace dvmbridge
{
    /// <summary>
    /// Implements a FNE system base.
    /// </summary>
    public abstract partial class FneSystemBase
    {
        private const int DMR_FRAME_LENGTH_BYTES = 33;
        private const int DMR_PACKET_SIZE = 55;
        private const int AMBE_BUF_LEN = 9;

        private const int DMR_AMBE_LENGTH_BYTES = 27;
        private const int AMBE_PER_SLOT = 3;

        private static readonly byte[] DMR_SILENCE_DATA = { 0x01, 0x00,
            0xB9, 0xE8, 0x81, 0x52, 0x61, 0x73, 0x00, 0x2A, 0x6B, 0xB9, 0xE8,
            0x81, 0x52, 0x60, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x73, 0x00,
            0x2A, 0x6B, 0xB9, 0xE8, 0x81, 0x52, 0x61, 0x73, 0x00, 0x2A, 0x6B };

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
        /// <returns>True, if data stream is valid, otherwise false.</returns>
        protected virtual bool DMRDataValidate(uint peerId, uint srcId, uint dstId, byte slot, CallType callType, FrameType frameType, DMRDataType dataType, uint streamId)
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
            FneUtils.StringToBytes(Constants.TAG_DMR_DATA, data, 0, Constants.TAG_DMR_DATA.Length);

            FneUtils.Write3Bytes((uint)Program.Configuration.SourceId, ref data, 5);        // Source Address
            FneUtils.Write3Bytes((uint)Program.Configuration.DestinationId, ref data, 8);   // Destination Address

            data[15U] = (byte)((Program.Configuration.Slot == 1) ? 0x00 : 0x80);            // Slot Number
            data[15U] |= 0x00;                                                              // Group

            if (frameType == FrameType.VOICE_SYNC)
                data[15U] |= 0x10;
            else if (frameType == FrameType.VOICE)
                data[15U] |= n;
            else
                data[15U] |= (byte)(0x20 | (byte)frameType);

            data[4U] = seqNo;
        }

        /// <summary>
        /// Helper to send a DMR terminator with LC message.
        /// </summary>
        private void SendDMRTerminator()
        {
            byte n = (byte)((dmrSeqNo - 3U) % 6U);
            uint fill = 6U - n;

            FnePeer peer = (FnePeer)fne;
            ushort pktSeq = peer.pktSeq(true);

            byte[] data = null, dmrpkt = null;
            if (n > 0U) 
            {
                for (uint i = 0U; i < fill; i++) 
                {
                    // generate DMR AMBE data
                    data = new byte[DMR_FRAME_LENGTH_BYTES];
                    Buffer.BlockCopy(DMR_SILENCE_DATA, 0, data, 0, DMR_FRAME_LENGTH_BYTES);

                    byte lcss = embeddedData.GetData(ref data, dmrN);

                    // generated embedded signalling
                    EMB emb = new EMB();
                    emb.ColorCode = 0;
                    emb.LCSS = lcss;
                    emb.Encode(ref data);

                    // generate DMR network frame
                    dmrpkt = new byte[DMR_PACKET_SIZE];
                    CreateDMRMessage(ref dmrpkt, FrameType.DATA_SYNC, (byte)dmrSeqNo, n);
                    Buffer.BlockCopy(data, 0, dmrpkt, 20, DMR_FRAME_LENGTH_BYTES);

                    peer.SendMaster(new Tuple<byte, byte>(Constants.NET_FUNC_PROTOCOL, Constants.NET_PROTOCOL_SUBFUNC_DMR), dmrpkt, pktSeq, txStreamId);

                    dmrSeqNo++;
                    n++;
                }
            }

            data = new byte[DMR_FRAME_LENGTH_BYTES];

            // generate DMR LC
            LC dmrLC = new LC();
            dmrLC.FLCO = (byte)DMRFLCO.FLCO_GROUP;
            dmrLC.SrcId = (uint)Program.Configuration.SourceId;
            dmrLC.DstId = (uint)Program.Configuration.DestinationId;

            // generate the Slot TYpe
            SlotType slotType = new SlotType();
            slotType.DataType = (byte)DMRDataType.TERMINATOR_WITH_LC;
            slotType.GetData(ref data);

            FullLC.Encode(dmrLC, ref data, DMRDataType.TERMINATOR_WITH_LC);

            // generate DMR network frame
            dmrpkt = new byte[DMR_PACKET_SIZE];
            CreateDMRMessage(ref dmrpkt, FrameType.DATA_SYNC, (byte)dmrSeqNo, 0);
            Buffer.BlockCopy(data, 0, dmrpkt, 20, DMR_FRAME_LENGTH_BYTES);

            peer.SendMaster(new Tuple<byte, byte>(Constants.NET_FUNC_PROTOCOL, Constants.NET_PROTOCOL_SUBFUNC_DMR), dmrpkt, pktSeq, txStreamId);

            ambeCount = 0;
            dmrSeqNo = 0;
            dmrN = 0;
        }

        /// <summary>
        /// Helper to encode and transmit PCM audio as DMR AMBE frames.
        /// </summary>
        /// <param name="pcm"></param>
        private void DMREncodeAudioFrame(byte[] pcm)
        {
            uint srcId = (uint)Program.Configuration.SourceId;
            uint dstId = (uint)Program.Configuration.DestinationId;

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
                    dmrLC.SrcId = (uint)Program.Configuration.SourceId;
                    dmrLC.DstId = (uint)Program.Configuration.DestinationId;
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
            dmrEncoder.encode(samples, out ambe);
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
            // Log.Logger.Debug($"FULL AMBE {FneUtils.HexDump(ambe)}");
            for (int n = 0; n < AMBE_PER_SLOT; n++)
            {
                byte[] ambePartial = new byte[AMBE_BUF_LEN];
                for (int i = 0; i < AMBE_BUF_LEN; i++)
                    ambePartial[i] = ambe[i + (n * 9)];

                short[] samples = null;
                int errs = dmrDecoder.decode(ambePartial, out samples);
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

                    // Log.Logger.Debug($"BYTE BUFFER {FneUtils.HexDump(pcm)}");
                    waveProvider.AddSamples(pcm, 0, pcm.Length);
                }
            }
        }

        /// <summary>
        /// Event handler used to process incoming DMR data.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void DMRDataReceived(object sender, DMRDataReceivedEvent e)
        {
            DateTime pktTime = DateTime.Now;
            
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
                    status[e.Slot].RxStart = pktTime;
                    Log.Logger.Information($"({SystemName}) DMRD: Traffic *CALL START     * PEER {e.PeerId} SRC_ID {e.SrcId} TGID {e.DstId} [STREAM ID {e.StreamId}]");

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
            else
                Log.Logger.Warning($"({SystemName}) DMRD: Bridge does not support private calls.");

            return;
        }
    } // public abstract partial class FneSystemBase
} // namespace dvmbridge

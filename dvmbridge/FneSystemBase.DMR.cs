/**
* Digital Voice Modem - Fixed Network Equipment
* AGPLv3 Open Source. Use is subject to license terms.
* DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
*
* @package DVM / Fixed Network Equipment
*
*/
/*
*   Copyright (C) 2022 by Bryan Biedenkapp N2PLL
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
using System.Threading.Tasks;

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
        private MBEDecoderManaged dmrDecoder;
        private MBEEncoderManaged dmrEncoder;

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
        /// Event handler used to process incoming DMR data.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void DMRDataReceived(object sender, DMRDataReceivedEvent e)
        {
            DateTime pktTime = DateTime.Now;
            
            byte[] dmrpkt = new byte[33];
            Buffer.BlockCopy(e.Data, 20, dmrpkt, 0, 33);
            byte bits = e.Data[15];

            if (e.CallType == CallType.GROUP)
            {
                if (e.SrcId == 0)
                {
                    Log.Logger.Warning($"({SystemName}) DMRD: Received call from SRC_ID {e.SrcId}? Dropping call data.");
                    return;
                }

                // is this a new call stream?
                if (e.StreamId != status[e.Slot].RxStreamId)
                {
                    status[e.Slot].RxStart = pktTime;
                    Log.Logger.Information($"({SystemName}) DMRD: Traffic *CALL START     * PEER {e.PeerId} SRC_ID {e.SrcId} TGID {e.DstId} [STREAM ID {e.StreamId}]");

                    // if we can, use the LC from the voice header as to keep all options intact
                    if ((e.FrameType == FrameType.DATA_SYNC) && (e.DataType == DMRDataType.VOICE_LC_HEADER))
                    {
                        LC lc = FullLC.Decode(dmrpkt, DMRDataType.VOICE_LC_HEADER);
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
                    PrivacyLC lc = FullLC.DecodePI(dmrpkt);
                    status[e.Slot].DMR_RxPILC = lc;
                    Log.Logger.Information($"({SystemName}) DMRD: Traffic *CALL PI PARAMS  * PEER {e.PeerId} DST_ID {e.DstId} TS {e.Slot + 1} ALGID {lc.AlgId} KID {lc.KId} [STREAM ID {e.StreamId}]");
                    Log.Logger.Debug($"({SystemName}) TS {e.Slot + 1} [STREAM ID {e.StreamId}] RX_PI_LC {FneUtils.HexDump(status[e.Slot].DMR_RxPILC.GetBytes())}");
                }

                if ((e.FrameType == FrameType.DATA_SYNC) && (e.DataType == DMRDataType.TERMINATOR_WITH_LC) && (status[e.Slot].RxType != FrameType.TERMINATOR))
                {
                    TimeSpan callDuration = pktTime - status[0].RxStart;
                    Log.Logger.Information($"({SystemName}) DMRD: Traffic *CALL END       * PEER {e.PeerId} SRC_ID {e.SrcId} TGID {e.DstId} DUR {callDuration} [STREAM ID {e.StreamId}]");
                }

                /* TODO: handle vocoding */

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

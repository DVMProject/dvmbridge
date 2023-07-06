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
using dvmbridge.FNE.P25;
using vocoder;

namespace dvmbridge
{
    /// <summary>
    /// Implements a FNE system base.
    /// </summary>
    public abstract partial class FneSystemBase
    {
        private MBEDecoderManaged p25Decoder;
        private MBEEncoderManaged p25Encoder;

        /*
        ** Methods
        */

        /// <summary>
        /// Callback used to validate incoming P25 data.
        /// </summary>
        /// <param name="peerId">Peer ID</param>
        /// <param name="srcId">Source Address</param>
        /// <param name="dstId">Destination Address</param>
        /// <param name="callType">Call Type (Group or Private)</param>
        /// <param name="duid">P25 DUID</param>
        /// <param name="frameType">Frame Type</param>
        /// <param name="streamId">Stream ID</param>
        /// <returns>True, if data stream is valid, otherwise false.</returns>
        protected virtual bool P25DataValidate(uint peerId, uint srcId, uint dstId, CallType callType, P25DUID duid, FrameType frameType, uint streamId)
        {
            return true;
        }

        /// <summary>
        /// Event handler used to pre-process incoming P25 data.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void P25DataPreprocess(object sender, P25DataReceivedEvent e)
        {
            return;
        }

        /// <summary>
        /// Event handler used to process incoming P25 data.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void P25DataReceived(object sender, P25DataReceivedEvent e)
        {
            DateTime pktTime = DateTime.Now;
            if (e.CallType == CallType.GROUP)
            {
                if (e.SrcId == 0)
                {
                    Log.Logger.Warning($"({SystemName}) P25D: Received call from SRC_ID {e.SrcId}? Dropping call e.Data.");
                    return;
                }

                // is this a new call stream?
                if (e.StreamId != status[P25_FIXED_SLOT].RxStreamId && ((e.DUID != P25DUID.TDU) && (e.DUID != P25DUID.TDULC)))
                {
                    status[P25_FIXED_SLOT].RxStart = pktTime;
                    Log.Logger.Information($"({SystemName}) P25D: Traffic *CALL START     * PEER {e.PeerId} SRC_ID {e.SrcId} TGID {e.DstId} [STREAM ID {e.StreamId}]");
                }

                if (((e.DUID == P25DUID.TDU) || (e.DUID == P25DUID.TDULC)) && (status[P25_FIXED_SLOT].RxType != FrameType.TERMINATOR))
                {
                    TimeSpan callDuration = pktTime - status[P25_FIXED_SLOT].RxStart;
                    Log.Logger.Information($"({SystemName}) P25D: Traffic *CALL END       * PEER {e.PeerId} SRC_ID {e.SrcId} TGID {e.DstId} DUR {callDuration} [STREAM ID {e.StreamId}]");
                }

                /* TODO: handle vocoding */

                status[P25_FIXED_SLOT].RxRFS = e.SrcId;
                status[P25_FIXED_SLOT].RxType = e.FrameType;
                status[P25_FIXED_SLOT].RxTGId = e.DstId;
                status[P25_FIXED_SLOT].RxTime = pktTime;
                status[P25_FIXED_SLOT].RxStreamId = e.StreamId;
            }
            else
                Log.Logger.Warning($"({SystemName}) P25D: Bridge does not support private calls.");

            return;
        }
    } // public abstract partial class FneSystemBase
} // namespace dvmbridge

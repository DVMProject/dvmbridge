/**
* Digital Voice Modem - Bridge
* AGPLv3 Open Source. Use is subject to license terms.
* DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
*
* @package DVM / Bridge
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
using dvmbridge.FNE.NXDN;

namespace dvmbridge
{
    /// <summary>
    /// Implements a FNE system base.
    /// </summary>
    public abstract partial class FneSystemBase
    {
        private List<Tuple<byte[], ushort>> nxdnCallData = new List<Tuple<byte[], ushort>>();

        /*
        ** Methods
        */

        /// <summary>
        /// Callback used to validate incoming NXDN data.
        /// </summary>
        /// <param name="peerId">Peer ID</param>
        /// <param name="srcId">Source Address</param>
        /// <param name="dstId">Destination Address</param>
        /// <param name="callType">Call Type (Group or Private)</param>
        /// <param name="messageType">NXDN Message Type</param>
        /// <param name="frameType">Frame Type</param>
        /// <param name="streamId">Stream ID</param>
        /// <returns>True, if data stream is valid, otherwise false.</returns>
        protected virtual bool NXDNDataValidate(uint peerId, uint srcId, uint dstId, CallType callType, NXDNMessageType messageType, FrameType frameType, uint streamId)
        {
            return true;
        }

        /// <summary>
        /// Event handler used to process incoming NXDN data.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void NXDNDataReceived(object sender, NXDNDataReceivedEvent e)
        {
            /* unsupported for now */
            return;
        }
    } // public abstract partial class FneSystemBase
} // namespace dvmbridge

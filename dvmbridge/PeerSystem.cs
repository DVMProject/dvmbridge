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
using System.Net;
using System.Collections.Generic;
using System.Text;

using Serilog;

using dvmbridge.FNE;

namespace dvmbridge
{
    /// <summary>
    /// Implements a peer FNE router system.
    /// </summary>
    public class PeerSystem : FneSystemBase
    {
        protected FnePeer peer;

        /*
        ** Methods
        */

        /// <summary>
        /// Initializes a new instance of the <see cref="PeerSystem"/> class.
        /// </summary>
        public PeerSystem() : base(Create())
        {
            this.peer = (FnePeer)fne;
        }

        /// <summary>
        /// Internal helper to instantiate a new instance of <see cref="FnePeer"/> class.
        /// </summary>
        /// <param name="config">Peer stanza configuration</param>
        /// <returns><see cref="FnePeer"/></returns>
        private static FnePeer Create()
        {
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, Program.Configuration.Port);

            if (Program.Configuration.Address == null)
                throw new NullReferenceException("address");
            if (Program.Configuration.Address == string.Empty)
                throw new ArgumentException("address");

            // handle using address as IP or resolving from hostname to IP
            try
            {
                endpoint = new IPEndPoint(IPAddress.Parse(Program.Configuration.Address), Program.Configuration.Port);
            }
            catch (FormatException)
            {
                IPAddress[] addresses = Dns.GetHostAddresses(Program.Configuration.Address);
                if (addresses.Length > 0)
                    endpoint = new IPEndPoint(addresses[0], Program.Configuration.Port);
            }

            FnePeer peer = new FnePeer(Program.Configuration.Name, Program.Configuration.PeerId, endpoint);

            // set configuration parameters
            peer.RawPacketTrace = Program.Configuration.RawPacketTrace;

            peer.PingTime = Program.Configuration.PingTime;
            peer.Passphrase = Program.Configuration.Passphrase;
            peer.Information.Details = ConfigurationObject.ConvertToDetails(Program.Configuration);

            return peer;
        }

        /// <summary>
        /// Helper to send a activity transfer message to the master.
        /// </summary>
        /// <param name="message">Message to send</param>
        public void SendActivityTransfer(string message)
        {
            /* stub */
        }

        /// <summary>
        /// Helper to send a diagnostics transfer message to the master.
        /// </summary>
        /// <param name="message">Message to send</param>
        public void SendDiagnosticsTransfer(string message)
        {
            /* stub */
        }
    } // public class PeerSystem
} // namespace dvmbridge

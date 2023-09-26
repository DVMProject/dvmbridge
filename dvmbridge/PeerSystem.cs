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
using System.Net;
using System.Collections.Generic;
using System.Text;

using Serilog;

using fnecore;

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

            Log.Logger.Information($"    Peer ID: {Program.Configuration.PeerId}");
            Log.Logger.Information($"    Master Addresss: {Program.Configuration.Address}");
            Log.Logger.Information($"    Master Port: {Program.Configuration.Port}");
            Log.Logger.Information($"    PCM Rx Audio Gain: {Program.Configuration.RxAudioGain}");
            Log.Logger.Information($"    Vocoder Decoder Gain (audio from network): {Program.Configuration.VocoderDecoderAudioGain}");
            string decoderAutoGainEnabled = (Program.Configuration.VocoderDecoderAutoGain) ? "yes" : "no";
            Log.Logger.Information($"    Vocoder Decoder Automatic Gain: {decoderAutoGainEnabled}");
            Log.Logger.Information($"    PCM Tx Audio Gain: {Program.Configuration.TxAudioGain}");
            Log.Logger.Information($"    Vocoder Encoder Gain (audio to network): {Program.Configuration.VocoderEncoderAudioGain}");
            switch (Program.Configuration.TxMode)
            {
                case 1:
                    Log.Logger.Information($"    Tx Audio Mode: DMR");
                    break;
                case 2:
                    Log.Logger.Information($"    Tx Audio Mode: P25");
                    break;
            }
            Log.Logger.Information($"    VOX Sample Level: {Program.Configuration.VoxSampleLevel}");
            Log.Logger.Information($"    VOX Dropout Time: {Program.Configuration.DropTimeMs} ms");
            string detectAnalogMDC1200Enabled = (Program.Configuration.DetectAnalogMDC1200) ? "yes" : "no";
            Log.Logger.Information($"    Detect Analog MDC1200: {detectAnalogMDC1200Enabled}");
            string preambleLeaderEnabled = (Program.Configuration.PreambleLeaderTone) ? "yes" : "no";
            Log.Logger.Information($"    Preamble Leader: {preambleLeaderEnabled}");
            Log.Logger.Information($"    Preamble Tone: {Program.Configuration.PreambleTone} Hz");
            Log.Logger.Information($"    Preamble Length: {Program.Configuration.PreambleLength} ms");
            string grantDemandEnabled = (Program.Configuration.GrantDemand) ? "yes" : "no";
            Log.Logger.Information($"    Grant Demand: {grantDemandEnabled}");
            string localAudioEnabled = (Program.Configuration.LocalAudio) ? "yes" : "no";
            Log.Logger.Information($"    Local Audio: {localAudioEnabled}");
            string udpAudioEnabled = (Program.Configuration.UdpAudio) ? "yes" : "no";
            Log.Logger.Information($"    UDP Audio: {udpAudioEnabled}");
            Log.Logger.Information($"    Source Radio ID: {Program.Configuration.SourceId}");
            string overrideSourceIdFromMDCEnabled = (Program.Configuration.OverrideSourceIdFromMDC) ? "yes" : "no";
            Log.Logger.Information($"    Override Source Radio ID from MDC: {overrideSourceIdFromMDCEnabled}");
            Log.Logger.Information($"    Destination ID: {Program.Configuration.DestinationId}");
            Log.Logger.Information($"    Destination DMR Slot: {Program.Configuration.Slot}");

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

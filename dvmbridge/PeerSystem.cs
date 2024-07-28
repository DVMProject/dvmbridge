// SPDX-License-Identifier: AGPL-3.0-only
/**
* Digital Voice Modem - Audio Bridge
* AGPLv3 Open Source. Use is subject to license terms.
* DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
*
* @package DVM / Audio Bridge
* @license AGPLv3 License (https://opensource.org/licenses/AGPL-3.0)
*
*   Copyright (C) 2023 Bryan Biedenkapp, N2PLL
*   Copyright (C) 2024 Caleb, KO4UYJ
*
*/
using System;
using System.Net;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;

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

        private UdpClient udpAudioClient;
        private IPEndPoint endPoint;

        /*
        ** Methods
        */

        /// <summary>
        /// Initializes a new instance of the <see cref="PeerSystem"/> class.
        /// </summary>
        public PeerSystem() : base(Create())
        {
            this.peer = (FnePeer)fne;

            // only initialize the audio client if we are using UDP audio
            if (Program.Configuration.UdpAudio)
            {
                udpAudioClient = new UdpClient(Program.Configuration.UdpReceivePort);
                endPoint = new IPEndPoint(IPAddress.Parse(Program.Configuration.UdpReceiveAddress), Program.Configuration.UdpReceivePort);
            }
        }

        /// <summary>
        /// Internal helper to instantiate a new instance of <see cref="FnePeer"/> class.
        /// </summary>
        /// <param name="config">Peer stanza configuration</param>
        /// <returns><see cref="FnePeer"/></returns>
        private static FnePeer Create()
        {
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, Program.Configuration.Port);
            string presharedKey = Program.Configuration.Encrypted ? Program.Configuration.PresharedKey : null;

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
            string overrideSourceIdFromUDPEnabled = (Program.Configuration.OverrideSourceIdFromUDP) ? "yes" : "no";
            Log.Logger.Information($"    Override Source Radio ID from UDP: {overrideSourceIdFromUDPEnabled}");
            Log.Logger.Information($"    Destination ID: {Program.Configuration.DestinationId}");
            Log.Logger.Information($"    Destination DMR Slot: {Program.Configuration.Slot}");

            FnePeer peer = new FnePeer(Program.Configuration.Name, Program.Configuration.PeerId, endpoint, presharedKey);

            // set configuration parameters
            peer.RawPacketTrace = Program.Configuration.RawPacketTrace;

            peer.PingTime = Program.Configuration.PingTime;
            peer.Passphrase = Program.Configuration.Passphrase;
            peer.Information.Details = ConfigurationObject.ConvertToDetails(Program.Configuration);

            peer.PeerConnected += Peer_PeerConnected;

            return peer;
        }

        /// <summary>
        /// Event action that handles when a peer connects.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private static void Peer_PeerConnected(object sender, PeerConnectedEvent e)
        {
            // fake a group affiliation
            FnePeer peer = (FnePeer)sender;
            peer.SendMasterGroupAffiliation(1, (uint)Program.Configuration.DestinationId);
        }

        /// <summary>
        /// Start UDP audio listener
        /// </summary>
        public override async Task StartListeningAsync()
        {
            // only initialize the audio client if we are using UDP audio
            if (Program.Configuration.UdpAudio)
            {
                Log.Logger.Information($"Started UDP audio listener on {Program.Configuration.UdpReceiveAddress}:{Program.Configuration.UdpReceivePort}");

                while (true)
                {
                    try
                    {
                        UdpReceiveResult result = await udpAudioClient.ReceiveAsync();
                        ProcessAudioData(result.Buffer);
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error($"Error receiving UDP data: {ex}");
                    }
                }
            }
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

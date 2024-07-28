// SPDX-License-Identifier: AGPL-3.0-only
/**
* Digital Voice Modem - Audio Bridge
* AGPLv3 Open Source. Use is subject to license terms.
* DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
*
* @package DVM / Audio Bridge
* @license AGPLv3 License (https://opensource.org/licenses/AGPL-3.0)
*
*   Copyright (C) 2022 Bryan Biedenkapp, N2PLL
*
*/
using System;
using System.Collections.Generic;

using fnecore;

namespace dvmbridge
{
    /// <summary>
    /// 
    /// </summary>
    public class ConfigLogObject
    {
        /// <summary>
        /// 
        /// </summary>
        public int DisplayLevel = 1;
        /// <summary>
        /// 
        /// </summary>
        public int FileLevel = 1;
        /// <summary>
        /// 
        /// </summary>
        public string FilePath = ".";
        /// <summary>
        /// 
        /// </summary>
        public string FileRoot = "dvmbridge";
    } // public class ConfigLogObject

    /// <summary>
    /// 
    /// </summary>
    public class ConfigurationObject
    {
        /// <summary>
        /// 
        /// </summary>
        public ConfigLogObject Log = new ConfigLogObject();

        /// <summary>
        /// Time in seconds between pings to peers.
        /// </summary>
        public int PingTime = 5;

        /// <summary>
        /// Flag indicating whether or not the router should debug display all packets received.
        /// </summary>
        public bool RawPacketTrace = false;

        /// <summary>
        /// PCM audio gain for received (from digital network) audio frames.
        /// </summary>
        /// <remarks>This is used to apply gain to the decoded IMBE/AMBE audio, post-decoding.</remarks>
        public float RxAudioGain = 1.0f;

        /// <summary>
        /// Vocoder audio gain for decoded (from digital network) audio frames.
        /// </summary>
        /// <remarks>This is used to apply gain to the decoded IMBE/AMBE audio in the vocoder. (Not used when utilizing external USB vocoder!)</remarks>
        public float VocoderDecoderAudioGain = 3.0f;

        /// <summary>
        /// Flag indicating AGC should be used for frames received/decoded.
        /// </summary>
        /// <remarks>This is used to apply automatic gain control to decoded IMBE/AMBE audio in the vocoder. (Not used when utilizing external USB vocoder!)</remarks>
        public bool VocoderDecoderAutoGain = false;

        /// <summary>
        /// PCM audio gain for transmitted (to digital network) audio frames.
        /// </summary>
        public float TxAudioGain = 1.0f;

        /// <summary>
        /// Vocoder audio gain for transmitted/encoded (to digital network) audio frames.
        /// </summary>
        /// <remarks>This is used to apply gain to the encoded IMBE/AMBE audio in the vocoder. (Not used when utilizing external USB vocoder!)</remarks>
        public float VocoderEncoderAudioGain = 3.0f;

        /// <summary>
        /// Audio transmit mode (1 - DMR, 2 - P25).
        /// </summary>
        public int TxMode = 1;

        /// <summary>
        /// Relative sample level for VOX to activate.
        /// </summary>
        public float VoxSampleLevel = 30.0f;
        /// <summary>
        /// Amount of time (ms) from loss of active VOX level to drop audio.
        /// </summary>
        public int DropTimeMs = 180;

        /// <summary>
        /// Enables detection of MDC1200 packets on the PCM side of the bridge.
        /// </summary>
        public bool DetectAnalogMDC1200 = false;

        /// <summary>
        /// Flag indicating whether the analog preamble leader is enabled.
        /// </summary>
        public bool PreambleLeaderTone = false;
        /// <summary>
        /// Frequency of preamble tone.
        /// </summary>
        public double PreambleTone = 2175d;
        /// <summary>
        /// Amount of time (ms) to transmit preamble tone.
        /// </summary>
        public int PreambleLength = 200;

        /// <summary>
        /// Flag indicating whether a network grant demand packet will be sent before audio.
        /// </summary>
        public bool GrantDemand = false;

        /// <summary>
        /// Enable local audio over speakers.
        /// </summary>
        public bool LocalAudio = true;

        /// <summary>
        /// Enable PCM audio over UDP.
        /// </summary>
        public bool UdpAudio = false;
        /// <summary>
        /// Enable PCM audio over UDP meta data.
        /// </summary>
        public bool UdpMetaData = true;

        /// <summary>
        /// PCM over UDP send port.
        /// </summary>
        public int UdpSendPort = 34001;
        /// <summary>
        /// PCM over UDP send address destination
        /// </summary>
        public string UdpSendAddress = "127.0.0.1";
        /// <summary>
        /// PCM over UDP receive port.
        /// </summary>
        public int UdpReceivePort = 32001;
        /// <summary>
        /// PCM over UDP reciver listening address
        /// </summary>
        public string UdpReceiveAddress = "127.0.0.1";

        /// <summary>
        /// Textual Name.
        /// </summary>
        public string Name = "BRIDGE";
        /// <summary>
        /// Network Peer ID.
        /// </summary>
        public uint PeerId;
        /// <summary>
        /// Hostname/IP address of FNE master to connect to.
        /// </summary>
        public string Address = "127.0.0.1";
        /// <summary>
        /// Port number to connect to.
        /// </summary>
        public int Port = 62031;
        /// <summary>
        /// FNE access password.
        /// </summary>
        public string Passphrase;

        /// <summary>
        /// Enable/Disable AES Wrapped UDP
        /// </summary>
        public bool Encrypted;
        /// <summary>
        /// Pre shared AES key for AES wrapped UDP
        /// </summary>
        public string PresharedKey;

        /// <summary>
        /// Source "Radio ID" for transmitted audio frames
        /// </summary>
        public int SourceId;

        /// <summary>
        /// Flag indicating the source "Radio ID" will be overridden from the detected MDC1200 pre- PTT ID.
        /// </summary>
        public bool OverrideSourceIdFromMDC = false;
        /// <summary>
        /// Flag indicating the source "Radio ID" will be overridden from the received UDP PTT ID.
        /// </summary>
        public bool OverrideSourceIdFromUDP = false;

        /// <summary>
        /// Talkgroup ID for transmitted/received audio frames.
        /// </summary>
        public int DestinationId;

        /// <summary>
        /// Slot for received/transmitted audio frames.
        /// </summary>
        public int Slot = 1;

        /*
        ** Methods
        */

        /// <summary>
        /// Helper to convert the <see cref="ConfigPeerObject"/> to a <see cref="PeerDetails"/> object.
        /// </summary>
        /// <param name="peer"></param>
        /// <returns></returns>
        public static PeerDetails ConvertToDetails(ConfigurationObject peer)
        {
            PeerDetails details = new PeerDetails();

            // identity
            details.Identity = peer.Name;
            details.RxFrequency = 0;
            details.TxFrequency = 0;

            // system info
            details.Latitude = 0.0d;
            details.Longitude = 0.0d;
            details.Height = 1;
            details.Location = "Digital Network";

            // channel data
            details.TxPower = 0;
            details.TxOffsetMhz = 0.0f;
            details.ChBandwidthKhz = 0.0f;
            details.ChannelID = 0;
            details.ChannelNo = 0;

            // RCON
            details.Password = "ABCD123";
            details.Port = 9990;

            details.Software = $"DVM_BRIDGE_R{AssemblyVersion._SEM_VERSION.Major.ToString("D2")}A{AssemblyVersion._SEM_VERSION.Minor.ToString("D2")}";//AssemblyVersion._VERSION;

            return details;
        }
    } // public class ConfigurationObject
} // namespace dvmbridge

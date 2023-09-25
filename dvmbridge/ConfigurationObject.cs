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
        /// 
        /// </summary>
        public int PingTime = 5;

        /// <summary>
        /// 
        /// </summary>
        public bool RawPacketTrace = false;
        
        /// <summary>
        /// 
        /// </summary>
        public float RxAudioGain = 3.0f;

        /// <summary>
        /// 
        /// </summary>
        public bool RxAutoGain = false;

        /// <summary>
        /// 
        /// </summary>
        public float TxAudioGain = 3.0f;

        /// <summary>
        /// 
        /// </summary>
        public int TxMode = 1;

        /// <summary>
        /// 
        /// </summary>
        public float VoxSampleLevel = 30.0f;
        /// <summary>
        /// 
        /// </summary>
        public int DropTimeMs = 180;

        /// <summary>
        /// 
        /// </summary>
        public bool DetectAnalogMDC1200 = false;

        /// <summary>
        /// 
        /// </summary>
        public bool PreambleLeaderTone = false;
        /// <summary>
        /// 
        /// </summary>
        public double PreambleTone = 2175d;
        /// <summary>
        /// 
        /// </summary>
        public int PreambleLength = 200;

        /// <summary>
        /// 
        /// </summary>
        public bool GrantDemand = false;

        /// <summary>
        /// 
        /// </summary>
        public bool LocalAudio = true;

        /// <summary>
        /// 
        /// </summary>
        public bool UdpAudio = false;

        /// <summary>
        /// 
        /// </summary>
        public int UdpSendPort;
        /// <summary>
        /// 
        /// </summary>
        public string UdpSendAddress;
        /// <summary>
        /// 
        /// </summary>
        public int UdpRecievePort;
        /// <summary>
        /// 
        /// </summary>
        public string UdpReceiveAddress;

        /// <summary>
        /// 
        /// </summary>
        public string Name = "BRIDGE";
        /// <summary>
        /// 
        /// </summary>
        public uint PeerId;
        /// <summary>
        /// 
        /// </summary>
        public string Address;
        /// <summary>
        /// 
        /// </summary>
        public int Port;
        /// <summary>
        /// 
        /// </summary>
        public string Passphrase;

        /// <summary>
        /// 
        /// </summary>
        public int SourceId;

        /// <summary>
        /// 
        /// </summary>
        public bool OverrideSourceIdFromMDC = false;

        /// <summary>
        /// 
        /// </summary>
        public int DestinationId;

        /// <summary>
        /// 
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

            details.Software = AssemblyVersion._VERSION;

            return details;
        }
    } // public class ConfigurationObject
} // namespace dvmbridge

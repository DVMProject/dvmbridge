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

namespace dvmbridge.FNE.P25
{
    /// <summary>
    /// P25 Data Unit ID
    /// </summary>
    public enum P25DUID : byte
    {
        /// <summary>
        /// Header Data Unit
        /// </summary>
        HDU = 0x00,
        /// <summary>
        /// Terminator Data Unit
        /// </summary>
        TDU = 0x03,
        /// <summary>
        /// Logical Data Unit 1
        /// </summary>
        LDU1 = 0x05,
        /// <summary>
        /// Trunking Signalling Data Unit
        /// </summary>
        TSDU = 0x07,
        /// <summary>
        /// Logical Data Unit 2
        /// </summary>
        LDU2 = 0x0A,
        /// <summary>
        /// Packet Data Unit
        /// </summary>
        PDU = 0x0C,
        /// <summary>
        /// Terminator Data Unit with Link Control
        /// </summary>
        TDULC = 0x0F
    } // public enum P25DUID : byte

    /// <summary>
    /// P25 Constants
    /// </summary>
    public class P25Defines
    {
        // LDUx/TDULC Link Control Opcode(s)
        public const byte LC_GROUP = 0x00;                   // GRP VCH USER - Group Voice Channel User
        public const byte LC_GROUP_UPDT = 0x02;              // GRP VCH UPDT - Group Voice Channel Update
        public const byte LC_PRIVATE = 0x03;                 // UU VCH USER - Unit-to-Unit Voice Channel User
        public const byte LC_UU_ANS_REQ = 0x05;              // UU ANS REQ - Unit to Unit Answer Request 
        public const byte LC_TEL_INT_VCH_USER = 0x06;        // TEL INT VCH USER - Telephone Interconnect Voice Channel User 
        public const byte LC_TEL_INT_ANS_RQST = 0x07;        // TEL INT ANS RQST - Telephone Interconnect Answer Request 
        public const byte LC_CALL_TERM = 0x0F;               // CALL TERM - Call Termination or Cancellation
        public const byte LC_IDEN_UP = 0x18;                 // IDEN UP - Channel Identifier Update
        public const byte LC_SYS_SRV_BCAST = 0x20;           // SYS SRV BCAST - System Service Broadcast
        public const byte LC_ADJ_STS_BCAST = 0x22;           // ADJ STS BCAST - Adjacent Site Status Broadcast
        public const byte LC_RFSS_STS_BCAST = 0x23;          // RFSS STS BCAST - RFSS Status Broadcast
        public const byte LC_NET_STS_BCAST = 0x24;           // NET STS BCAST - Network Status Broadcast
        public const byte LC_CONV_FALLBACK = 0x2A;           // CONV FALLBACK - Conventional Fallback

        // TSBK ISP/OSP Shared Opcode(s)
        public const byte TSBK_IOSP_GRP_VCH = 0x00;          // GRP VCH REQ - Group Voice Channel Request (ISP), GRP VCH GRANT - Group Voice Channel Grant (OSP)
        public const byte TSBK_IOSP_UU_VCH = 0x04;           // UU VCH REQ - Unit-to-Unit Voice Channel Request (ISP), UU VCH GRANT - Unit-to-Unit Voice Channel Grant (OSP)
        public const byte TSBK_IOSP_UU_ANS = 0x05;           // UU ANS RSP - Unit-to-Unit Answer Response (ISP), UU ANS REQ - Unit-to-Unit Answer Request (OSP)
        public const byte TSBK_IOSP_TELE_INT_DIAL = 0x08;    // TELE INT DIAL REQ - Telephone Interconnect Request - Explicit (ISP), TELE INT DIAL GRANT - Telephone Interconnect Grant (OSP)
        public const byte TSBK_IOSP_TELE_INT_ANS = 0x0A;     // TELE INT ANS RSP - Telephone Interconnect Answer Response (ISP), TELE INT ANS REQ - Telephone Interconnect Answer Request (OSP)
        public const byte TSBK_IOSP_STS_UPDT = 0x18;         // STS UPDT REQ - Status Update Request (ISP), STS UPDT - Status Update (OSP)
        public const byte TSBK_IOSP_STS_Q = 0x1A;            // STS Q REQ - Status Query Request (ISP), STS Q - Status Query (OSP)
        public const byte TSBK_IOSP_MSG_UPDT = 0x1C;         // MSG UPDT REQ - Message Update Request (ISP), MSG UPDT - Message Update (OSP)
        public const byte TSBK_IOSP_CALL_ALRT = 0x1F;        // CALL ALRT REQ - Call Alert Request (ISP), CALL ALRT - Call Alert (OSP)
        public const byte TSBK_IOSP_ACK_RSP = 0x20;          // ACK RSP U - Acknowledge Response - Unit (ISP), ACK RSP FNE - Acknowledge Response - FNE (OSP)
        public const byte TSBK_IOSP_EXT_FNCT = 0x24;         // EXT FNCT RSP - Extended Function Response (ISP), EXT FNCT CMD - Extended Function Command (OSP)
        public const byte TSBK_IOSP_GRP_AFF = 0x28;          // GRP AFF REQ - Group Affiliation Request (ISP), GRP AFF RSP - Group Affiliation Response (OSP)
        public const byte TSBK_IOSP_U_REG = 0x2C;            // U REG REQ - Unit Registration Request (ISP), U REG RSP - Unit Registration Response (OSP)

        // TSBK Inbound Signalling Packet (ISP) Opcode(s)
        public const byte TSBK_ISP_TELE_INT_PSTN_REQ = 0x09; // TELE INT PSTN REQ - Telephone Interconnect Request - Implicit
        public const byte TSBK_ISP_SNDCP_CH_REQ = 0x12;      // SNDCP CH REQ - SNDCP Data Channel Request
        public const byte TSBK_ISP_STS_Q_RSP = 0x19;         // STS Q RSP - Status Query Response
        public const byte TSBK_ISP_CAN_SRV_REQ = 0x23;       // CAN SRV REQ - Cancel Service Request
        public const byte TSBK_ISP_EMERG_ALRM_REQ = 0x27;    // EMERG ALRM REQ - Emergency Alarm Request
        public const byte TSBK_ISP_GRP_AFF_Q_RSP = 0x29;     // GRP AFF Q RSP - Group Affiliation Query Response
        public const byte TSBK_ISP_U_DEREG_REQ = 0x2B;       // U DE REG REQ - Unit De-Registration Request
        public const byte TSBK_ISP_LOC_REG_REQ = 0x2D;       // LOC REG REQ - Location Registration Request

        // TSBK Outbound Signalling Packet (OSP) Opcode(s)
        public const byte TSBK_OSP_GRP_VCH_GRANT_UPD = 0x02; // GRP VCH GRANT UPD - Group Voice Channel Grant Update
        public const byte TSBK_OSP_UU_VCH_GRANT_UPD = 0x06;  // UU VCH GRANT UPD - Unit-to-Unit Voice Channel Grant Update
        public const byte TSBK_OSP_SNDCP_CH_GNT = 0x14;      // SNDCP CH GNT - SNDCP Data Channel Grant
        public const byte TSBK_OSP_SNDCP_CH_ANN = 0x16;      // SNDCP CH ANN - SNDCP Data Channel Announcement
        public const byte TSBK_OSP_DENY_RSP = 0x27;          // DENY RSP - Deny Response
        public const byte TSBK_OSP_SCCB_EXP = 0x29;          // SCCB - Secondary Control Channel Broadcast - Explicit 
        public const byte TSBK_OSP_GRP_AFF_Q = 0x2A;         // GRP AFF Q - Group Affiliation Query
        public const byte TSBK_OSP_LOC_REG_RSP = 0x2B;       // LOC REG RSP - Location Registration Response
        public const byte TSBK_OSP_U_REG_CMD = 0x2D;         // U REG CMD - Unit Registration Command
        public const byte TSBK_OSP_U_DEREG_ACK = 0x2F;       // U DE REG ACK - Unit De-Registration Acknowledge
        public const byte TSBK_OSP_QUE_RSP = 0x33;           // QUE RSP - Queued Response
        public const byte TSBK_OSP_IDEN_UP_VU = 0x34;        // IDEN UP VU - Channel Identifier Update for VHF/UHF Bands
        public const byte TSBK_OSP_SYS_SRV_BCAST = 0x38;     // SYS SRV BCAST - System Service Broadcast
        public const byte TSBK_OSP_SCCB = 0x39;              // SCCB - Secondary Control Channel Broadcast
        public const byte TSBK_OSP_RFSS_STS_BCAST = 0x3A;    // RFSS STS BCAST - RFSS Status Broadcast
        public const byte TSBK_OSP_NET_STS_BCAST = 0x3B;     // NET STS BCAST - Network Status Broadcast
        public const byte TSBK_OSP_ADJ_STS_BCAST = 0x3C;     // ADJ STS BCAST - Adjacent Site Status Broadcast
        public const byte TSBK_OSP_IDEN_UP = 0x3D;           // IDEN UP - Channel Identifier Update

        // TSBK Motorola Outbound Signalling Packet (OSP) Opcode(s)
        public const byte TSBK_OSP_MOT_GRG_ADD = 0x00;       // MOT GRG ADD - Motorola / Group Regroup Add (Patch Supergroup)
        public const byte TSBK_OSP_MOT_GRG_DEL = 0x01;       // MOT GRG DEL - Motorola / Group Regroup Delete (Unpatch Supergroup)
        public const byte TSBK_OSP_MOT_GRG_VCH_GRANT = 0x02; // MOT GRG GROUP VCH GRANT / Group Regroup Voice Channel Grant
        public const byte TSBK_OSP_MOT_GRG_VCH_UPD = 0x03;   // MOT GRG GROUP VCH GRANT UPD / Group Regroup Voice Channel Grant Update
        public const byte TSBK_OSP_MOT_CC_BSI = 0x0B;        // MOT CC BSI - Motorola / Control Channel Base Station Identifier
        public const byte TSBK_OSP_MOT_PSH_CCH = 0x0E;       // MOT PSH CCH - Motorola / Planned Control Channel Shutdown

        // TSBK Motorola Outbound Signalling Packet (OSP) Opcode(s)
        public const byte TSBK_OSP_DVM_GIT_HASH = 0xFB;      //
    } // public class P25Defines
} // namespace dvmbridge.FNE.P25

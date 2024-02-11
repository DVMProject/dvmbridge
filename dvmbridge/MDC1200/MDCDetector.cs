// SPDX-License-Identifier: GPL-2.0-only
/**
* Digital Voice Modem - Audio Bridge
* AGPLv3 Open Source. Use is subject to license terms.
* DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
*
* @package DVM / Audio Bridge
* @derivedfrom MDCTool project (https://github.com/gatekeep/MDCTool)
* @license GPLv2 License (https://opensource.org/licenses/GPL-2.0)
*
*   Copyright (C) 2023 Bryan Biedenkapp, N2PLL
*
*/
using System;
using System.Collections.Generic;
using System.Linq;

/*-
 * mdc_decode.c
 *   Decodes a specific format of 1200 BPS MSK data burst
 *   from input audio samples.
 *
 * Author: Matthew Kaufman (matthew@eeph.com)
 *
 * Copyright (c) 2005, 2010  Matthew Kaufman  All rights reserved.
 * 
 *  This file is part of Matthew Kaufman's MDC Encoder/Decoder Library
 *
 *  The MDC Encoder/Decoder Library is free software; you can
 *  redistribute it and/or modify it under the terms of version 2 of
 *  the GNU General Public License as published by the Free Software
 *  Foundation.
 *
 *  If you cannot comply with the terms of this license, contact
 *  the author for alternative license arrangements or do not use
 *  or redistribute this software.
 *
 *  The MDC Encoder/Decoder Library is distributed in the hope
 *  that it will be useful, but WITHOUT ANY WARRANTY; without even the
 *  implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR
 *  PURPOSE.  See the GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this software; if not, write to the Free Software
 *  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301
 *  USA.
 *
 *  or see http://www.gnu.org/copyleft/gpl.html
 *
-*/

namespace dvmbridge.MDC1200
{
    /// <summary>
    /// 
    /// </summary>
    public struct MDCFrame
    {
        public uint thu;                       //

        public bool xorBit;                    // XOR bit flag
        public bool invertBit;                 // Invert bit flag

        public int shstate;                    //  
        public int shcount;                    //

        public int nlstep;                     //
        public double[] nlevel;                //

        public int[] bits;                     //

        public uint syncLow;                   //
        public uint syncHigh;                  //
    } // public struct MDCUnit

    /// <summary>
    /// Implements a detector/decoder for MDC-1200 packets in a audio stream.
    /// </summary>
    public class MDCDetector : MDCCRC
    {
        /// <summary>
        /// Number of MDC decoders
        /// </summary>
        public const int MDC_ND = 5;

        /// <summary>
        /// Threshold for the number of "good bits"
        /// </summary>
        public const int MDC_GDTHRESH = 5;

        /// <summary>
        /// Maximum number of bits in a single MDC1200 codeword.
        /// </summary>
        public const int MAX_MDC1200_BITS = 112;
        /// <summary>
        /// Maximum number of data bytes in a MDC1200 packet.
        /// </summary>
        public const int MAX_MDC1200_DATA = 14;

        private uint incru;                     //

        private int goodFrames;                 // mumber of good frames in MDC data
        private bool inDouble;                  //

        private MDCFrame[] frames;              //

        private MDCPacket first;                // First packet (used as the only or first in a doubled MDC1200 stream)
        private MDCPacket second;               // Second packet

        /*
        ** Events
        */

        /// <summary>
        /// Occurs when a MDC packet is successfully decoded.
        /// </summary>
        public event Action<int, MDCPacket, MDCPacket> DecoderCallback;

        /*
        ** Properties
        */

        /// <summary>
        /// Gets the sample block size.
        /// </summary>
        public static int SampleBlockSize { get; private set; }

        /*
        ** Methods
        */

        /// <summary>
        /// Initializes a new instance of the <see cref="MDCDetector"/> class.
        /// </summary>
        /// <param name="sampleRate">Audio Sample Rate</param>
        public MDCDetector(int sampleRate)
        {
/*
            if (sampleRate == 8000)
                this.incru = 644245094;
            else if (sampleRate == 16000)
                this.incru = 322122547;
            else if (sampleRate == 22050)
                this.incru = 233739716;
            else if (sampleRate == 32000)
                this.incru = 161061274;
            else if (sampleRate == 44100)
                this.incru = 116869858;
            else if (sampleRate == 48000)
                this.incru = 107374182;
            else
            {
*/
                // WARNING: lower precision than above
                this.incru = (uint)(1200 * 2 * (int.MaxValue / sampleRate) + 800);
/*
            }
*/
            SampleBlockSize = sampleRate * 400 / 1000;

            Clear();
        }

        /// <summary>
        /// Internal function to reset any internal data fields before doing any operations.
        /// </summary>
        private void Clear()
        {
            this.frames = new MDCFrame[MDC_ND];

            // clear old packet data
            first = new MDCPacket();
            second = new MDCPacket();

            this.goodFrames = 0;
            this.inDouble = false;

            // wipe all arrays
            for (int i = 0; i < MDC_ND; i++)
            {
                MDCFrame frame = new MDCFrame();
                frame.thu = (uint)(i * 2 * (0x80000000 / MDC_ND));
                frame.xorBit = false;
                frame.invertBit = false;

                frame.shstate = -1;
                frame.shcount = 0;

                frame.nlstep = i;

                frame.nlevel = new double[10];
                frame.bits = new int[MAX_MDC1200_BITS];
                ClearBits(frame);

                frames[i] = frame;
            }
        }

        /// <summary>
        /// Internal function to clear the bits array for the given MDC frame.
        /// </summary>
        /// <param name="frame"></param>
        private void ClearBits(MDCFrame frame)
        {
            // iterate through all the bits clearing them (set to zero)
            for (int j = 0; j < MAX_MDC1200_BITS; j++)
                frame.bits[j] = 0;
        }

        /// <summary>
        /// Internal function to return the number of bits set to true (1) in a unsigned integer.
        /// </summary>
        /// <param name="n">Unsigned integer to check</param>
        /// <returns>Number of bits set to true (1)</returns>
        private int OneBits(uint n)
        {
            int i = 0;
            while (n > 0) /* not sure what boolean value is supposed to be checked, assuming non-zero */
            {
                ++i;
                n &= (n - 1);
            }
            return i;
        }

        /// <summary>
        /// Process samples input byte array for an MDC-1200 packet.
        /// </summary>
        /// <param name="samples">Array containing samples</param>
        /// <returns>Number of MDC1200 frames processed</returns>
        public int ProcessSamples(IEnumerable<float> samples)
        {
            // clear existing data
            Clear();

            // iterate through the samples
            foreach (var sample in samples.Take(SampleBlockSize))
            {
                double value = sample;

                // decode through all available decoders
                for (int j = 0; j < MDC_ND; j++)
                {
                    uint lthu = frames[j].thu;
                    frames[j].thu += 5 * this.incru;

                    // wrap
                    if (frames[j].thu < lthu)
                    {
                        frames[j].nlstep++;
                        if (frames[j].nlstep > 9)
                            frames[j].nlstep = 0;
                        frames[j].nlevel[frames[j].nlstep] = value;

                        nlProcess(j);
                    }
                }
            }

            // if we have a good frames; return the number of MDC-1200 frames processed
            if (goodFrames > 0)
                return goodFrames;
            return 0;
        }

        /// <summary>
        /// Noise level processing
        /// </summary>
        /// <param name="idx">Decoder Index</param>
        private void nlProcess(int idx)
        {
            double vnow, vpast;

            switch (frames[idx].nlstep)
            {
                case 3:
                    vnow = ((-0.60 * frames[idx].nlevel[3]) + (.97 * frames[idx].nlevel[1]));
                    vpast = ((-0.60 * frames[idx].nlevel[7]) + (.97 * frames[idx].nlevel[9]));
                    break;
                case 8:
                    vnow = ((-0.60 * frames[idx].nlevel[8]) + (.97 * frames[idx].nlevel[6]));
                    vpast = ((-0.60 * frames[idx].nlevel[2]) + (.97 * frames[idx].nlevel[4]));
                    break;
                default:
                    return;
            }

            frames[idx].xorBit = (vnow > vpast) ? true : false;
            if (frames[idx].invertBit)
                frames[idx].xorBit = !(frames[idx].xorBit);

            ShiftIn(idx);
        }

        /// <summary>
        /// Internal function to bit shift the MDC1200 data accounting for sync data.
        /// </summary>
        /// <param name="idx">Decoder Index</param>
        private void ShiftIn(int idx)
        {
            bool bit = frames[idx].xorBit;
            int gcount;

            switch (frames[idx].shstate)
            {
                case -1:
                case 0:
                    {
                        if (frames[idx].shstate == -1)
                        {
                            frames[idx].syncHigh = 0;
                            frames[idx].syncLow = 0;
                            frames[idx].shstate = 0;
                        }

                        frames[idx].syncHigh <<= 1;
                        if ((frames[idx].syncLow & 0x80000000) != 0)
                            frames[idx].syncHigh |= 1;
                        frames[idx].syncLow <<= 1;
                        if (bit)
                            frames[idx].syncLow |= 1;

                        gcount = OneBits(0x000000FF & (0x00000007 ^ frames[idx].syncHigh));
                        gcount += OneBits(0x092A446F ^ frames[idx].syncLow);

                        // check if the "good" bits is less then the threshold
                        if (gcount <= MDC_GDTHRESH)
                        {
                            frames[idx].shstate = 1;
                            frames[idx].shcount = 0;
                            ClearBits(frames[idx]);
                        }
                        else if (gcount >= (40 - MDC_GDTHRESH))
                        {
                            frames[idx].shstate = 1;
                            frames[idx].shcount = 0;
                            frames[idx].xorBit = !(frames[idx].xorBit);
                            frames[idx].invertBit = !(frames[idx].invertBit);
                            ClearBits(frames[idx]);
                        }
                    }
                    return;

                case 1:
                case 2:
                    {
                        frames[idx].bits[frames[idx].shcount] = (byte)((bit) ? 1 : 0);
                        frames[idx].shcount++;

                        if (frames[idx].shcount > 111)
                            ProcessBits(idx);
                    }
                    return;

                default:
                    return;
            }
        }

        /// <summary>
        /// Internal helper to handle convolutional encoding for error detection.
        /// </summary>
        /// <param name="data"></param>
        private void ECC(ref byte[] data)
        {
            int b;
            int[] csr = new int[7];
            int syn;
            byte fixi, fixj;
            int ec;

            syn = 0;
            for (int i = 0; i < 7; i++)
                csr[i] = 0;

            for (byte i = 0; i < 7; i++)
            {
                for (byte j = 0; j <= 7; j++)
                {
                    for (int k = 6; k > 0; k--)
                        csr[k] = csr[k - 1];

                    csr[0] = (data[i] >> j) & 0x01;
                    b = csr[0] + csr[2] + csr[5] + csr[6];
                    syn <<= 1;
                    if (((b & 0x01) ^ ((data[i + 7] >> j) & 0x01)) > 0)
                        syn |= 1;

                    ec = 0;
                    if ((syn & 0x80) > 0)
                        ++ec;
                    if ((syn & 0x20) > 0)
                        ++ec;
                    if ((syn & 0x04) > 0)
                        ++ec;
                    if ((syn & 0x02) > 0)
                        ++ec;

                    if (ec >= 3)
                    {
                        syn ^= 0xa6;
                        fixi = i;
                        fixj = (byte)(j - 7);
                        if (fixj < 0)
                        {
                            --fixi;
                            fixj += 8;
                        }

                        if (fixi >= 0)
                            data[fixi] ^= (byte)(1 << fixj); // flip
                    }
                }
            }
        }

        /// <summary>
        /// Internal function to process the bits of a MDC1200 data stream.
        /// </summary>
        /// <param name="idx">Decoder Index</param>
        /// <returns>True, if CRC was matched and decoding succeeded, otherwise false</returns>
        private bool ProcessBits(int idx)
        {
            int[] lbits = new int[MAX_MDC1200_BITS];
            int lbc = 0;
            byte[] data = new byte[MAX_MDC1200_DATA];
            ushort ccrc, rcrc;

            // do deep magic bit manipulation
            for (int i = 0; i < 16; i++)
            {
                // loop through 8 bits
                for (int j = 0; j < 7; j++)
                {
                    int k = (j * 16) + i;
                    lbits[lbc] = frames[idx].bits[k];
                    ++lbc;
                }
            }

            for (int i = 0; i < MAX_MDC1200_DATA; i++)
            {
                data[i] = 0;

                // loop through 8 bits (get each byte)
                for (int j = 0; j < 8; j++)
                {
                    int k = (i * 8) + j;

                    if (lbits[k] != 0)
                        data[i] |= (byte)(1 << j);
                }
            }

            // perform ECC
            ECC(ref data);

            // compute CRC
            ccrc = ComputeCRC(data, 4);
            rcrc = (ushort)(data[5] << 8 | data[4]);

            // compare the computed CRC to the recieved CRC
            if (ccrc == rcrc)
            {
                byte crc;

                if (frames[idx].shstate == 2)
                {
                    // copy second packet data
                    second.Operation = data[0];
                    second.Argument = data[1];
                    second.Target = (ushort)((data[2] << 8) | data[3]);

                    // reset the states for all decoders
                    for (int k = 0; k < MDC_ND; k++)
                        frames[idx].shstate = 0;

                    this.goodFrames = 2;
                    this.inDouble = false;
                }
                else
                {
                    this.goodFrames = 1;

                    // copy first packet data
                    first.Operation = data[0];
                    first.Argument = data[1];
                    first.Target = (ushort)((data[2] << 8) | data[3]);
                    crc = (byte)((data[4] << 8) | data[5]);

                    if (!inDouble)
                    {
                        // check if the operation code is for a "double" packet
                        switch (data[0])
                        {
                            case OpType.DOUBLE_PACKET_TYPE1:
                            case OpType.DOUBLE_PACKET_TYPE2:
                                {
                                    // we have a double packet reset the frame count to 0
                                    // and set the state to reflect a double packet
                                    goodFrames = 0;
                                    inDouble = true;
                                    frames[idx].shstate = 2;
                                    frames[idx].shcount = 0;

                                    ClearBits(frames[idx]);
                                }
                                break;

                            default:
                                // reset the states for all decoders
                                for (int k = 0; k < MDC_ND; k++)
                                    frames[k].shstate = 0;
                                break;
                        }
                    }
                    else
                    {
                        frames[idx].shstate = 2;
                        frames[idx].shcount = 0;
                        ClearBits(frames[idx]);
                    }
                }

                // if our frame count is non-zero execute the decoded callback
                if (goodFrames > 0)
                {
                    if (inDouble && goodFrames <= 1)
                    {
                        this.goodFrames = 0;
                        return true;
                    }

                    // fire event
                    if (DecoderCallback != null)
                        DecoderCallback(goodFrames, first, second);

                    // reset frame count
                    goodFrames = 0;

                    // reset the states for all decoders
                    for (int k = 0; k < MDC_ND; k++)
                        frames[k].shstate = 0;
                }

                return true;
            }
            else
            {
                // since the CRC is bad reset the frame count
                this.goodFrames = 0;
                frames[idx].shstate = -1;
            }
            return false;
        }

        /// <summary>
        /// Translates a MDC1200 operation to a string
        /// </summary>
        /// <param name="packet">MDC Packet to decode</param>
        /// <returns>String containing parsed MDC1200 operation</returns>
        public static string ToString(MDCPacket packet)
        {
            string ret = string.Empty;
            string unit = packet.Target.ToString("X4");

            // check operation
            switch (packet.Operation)
            {
                /**
                 * Single Packet Operations
                 */
                case OpType.PTT_ID:
                    {
                        // check argument
                        switch (packet.Argument)
                        {
                            case ArgType.NO_ARG:
                                ret += "PTT ID: " + unit + " [Post- ID]";
                                break;

                            case ArgType.PTT_PRE:
                                ret += "PTT ID: " + unit + " [ Pre- ID]";
                                break;

                            default:
                                ret += "PTT ID: " + unit + " [Unkw- ID]";
                                break;
                        }
                    }
                    break;

                case OpType.EMERGENCY:
                    ret += "!!! EMERGENCY " + unit;
                    break;

                case OpType.EMERGENCY_ACK:
                    ret += "!!! EMERGENCY Acknowledge: " + unit;
                    break;

                case OpType.RADIO_CHECK:
                    ret += "Radio Check Unit: " + unit;
                    break;

                case OpType.RADIO_CHECK_ACK:
                    ret += "Radio Check Acknowledge: " + unit;
                    break;

                case OpType.REMOTE_MONITOR:
                    ret += "Remote Monitor Unit: " + unit;
                    break;

                case OpType.RAC:
                    ret += "Repeater Access Request: " + unit;
                    break;

                case OpType.RTT_1:
                case OpType.RTT_2:
                    ret += "RTT From: " + unit;
                    break;

                case OpType.RTT_ACK: // also OpType.MESSAGE_ACK
                    ret += "RTT/Message Acknowledge: " + unit;
                    break;

                case OpType.STATUS_REQUEST:
                    ret += "Status Request To: " + unit;
                    break;

                case OpType.STATUS_RESPONSE:
                case OpType.SIMPLE_STATUS:
                    ret += "Status Response From: " + unit + " Status: " + packet.Argument.ToString("X2");
                    break;

                case OpType.MESSAGE:
                case OpType.MESSAGE_WITH_ACK:
                    ret += "Message Request To: " + unit + " Message: " + packet.Argument.ToString("X2");
                    break;

                case OpType.RADIO_INHIBIT:
                    {
                        // check argument
                        switch (packet.Argument)
                        {
                            case ArgType.NO_ARG:
                                ret += "Stun/Inhibit Target: " + unit;
                                break;

                            case ArgType.CANCEL_INHIBIT:
                                ret += "Revive/Uninhibit Target: " + unit;
                                break;

                            default:
                                ret += "UNK Inhibit Target: " + unit;
                                break;
                        }
                    }
                    break;

                case OpType.RADIO_INHIBIT_ACK:
                    {
                        // check argument
                        switch (packet.Argument)
                        {
                            case ArgType.NO_ARG:
                                ret += "Stun/Inhibit: Acknowlege: " + unit;
                                break;

                            case ArgType.CANCEL_INHIBIT:
                                ret += "Revive/Uninhibit Acknowledge: " + unit;
                                break;

                            default:
                                ret += "UNK Inhibit Acknowledge: " + unit;
                                break;
                        }
                    }
                    break;

                /**
                 * Double Packet Operations
                 */
                case OpType.DOUBLE_PACKET_TYPE1:
                case OpType.DOUBLE_PACKET_TYPE2:
                    ret += "Request To: " + unit;
                    break;

                case OpType.CALL_ALERT_ACK_EXPECTED:
                case OpType.CALL_ALERT_NOACK_EXPECTED:
                    ret += "Call Alert/Page From: " + unit;
                    break;

                case OpType.CALL_ALERT_ACK:
                    if ((ArgType.OTAR_UNK1 == packet.Argument) || (ArgType.OTAR_UNK2 == packet.Argument))
                        ret += "OTAR Acknowledge ?: " + unit;
                    else
                        ret += "Call Alert/Page Acknowledge: " + unit;
                    break;

                case OpType.SELECTIVE_CALL_1:
                case OpType.SELECTIVE_CALL_2:
                    ret += "Sel-Call: From: " + unit;
                    break;

                case OpType.OTAR:
                    ret += "OTAR: " + unit;
                    break;

                default:
                    ret += "UNK Op " + packet.Operation.ToString("X2") + " Arg " + packet.Argument.ToString("X2") + " Target ID " + unit;
                    break;
            }

            return ret;
        }
    } // public class MDCDetector : MDCCRC
} // namespace dvmbridge.MDC1200

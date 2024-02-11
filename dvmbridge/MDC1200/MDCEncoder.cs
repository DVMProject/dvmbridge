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

/*-
 * mdc_encode.c
 *  Encodes a specific format from 1200 BPS MSK data burst
 *  to output audio samples.
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
    /// Implements a encoder for MDC-1200 packets in a audio stream.
    /// </summary>
    public class MDCEncoder : MDCCRC
    {
        private static byte[] sinTable = {
            127, 130, 133, 136, 139, 142, 145, 148, 151, 154, 157, 160, 163, 166, 169, 172,
            175, 178, 180, 183, 186, 189, 191, 194, 196, 199, 201, 204, 206, 209, 211, 213,
            215, 218, 220, 222, 224, 226, 227, 229, 231, 233, 234, 236, 237, 239, 240, 241,
            242, 244, 245, 246, 247, 247, 248, 249, 250, 250, 251, 251, 251, 252, 252, 252,
            252, 252, 252, 252, 251, 251, 251, 250, 250, 249, 248, 247, 247, 246, 245, 244,
            242, 241, 240, 239, 237, 236, 234, 233, 231, 229, 227, 226, 224, 222, 220, 218,
            215, 213, 211, 209, 206, 204, 201, 199, 196, 194, 191, 189, 186, 183, 180, 178,
            175, 172, 169, 166, 163, 160, 157, 154, 151, 148, 145, 142, 139, 136, 133, 130,
            127, 124, 121, 118, 115, 112, 109, 106, 103, 100,  97,  94,  91,  88,  85,  82,
             79,  76,  74,  71,  68,  65,  63,  60,  58,  55,  53,  50,  48,  45,  43,  41,
             39,  36,  34,  32,  30,  28,  27,  25,  23,  21,  20,  18,  17,  15,  14,  13,
             12,  10,   9,   8,   7,   7,   6,   5,   4,   4,   3,   3,   3,   2,   2,   2,
              2,   2,   2,   2,   3,   3,   3,   4,   4,   5,   6,   7,   7,   8,   9,  10,
             12,  13,  14,  15,  17,  18,  20,  21,  23,  25,  27,  28,  30,  32,  34,  36,
             39,  41,  43,  45, 48,  50,  53,  55,   58,  60,  63,  65,  68,  71,  74,  76,
             79,  82,  85,  88,  91,  94,  97, 100, 103, 106, 109, 112, 115, 118, 121, 124
        };

        /// <summary>
        /// MDC1200 Codewords are 14 bytes each
        /// </summary>
        public const int CODEWORD_LENGTH = 14;

        /// <summary>
        /// MDC1200 Bit Sync is 5 bytes
        /// </summary>
        public const int BITSYNC_LENGTH = 5;

        /// <summary>
        /// Preamble is 8 bytes
        /// </summary>
        public const int PREAMBLE_LENGTH = 7;

        /// <summary>
        /// Default number of preambles transmitted with all packets
        /// </summary>
        public const int DEFAULT_PREAMBLES = 2;

        private uint incru;
        private uint incru18;
        private uint thu;
        private uint tthu;

        private int bpos;
        private int ipos;

        private bool state;
        private int lb;

        private bool xorBit;

        private int loaded;

        private int sampleRate;

        /// <summary>
        /// MDC1200 Data Array
        /// </summary>
        /// <remarks>The MDC1200 data array, consists of a 7 byte pad leader, followed by a 5 byte sync block,
        /// followed by 2 consecutive blocks of 14 bytes of packet data. This makes a full MDC1200 double packet array
        /// a full 40 bytes.</remarks>
        private byte[] dataArray;

        /*
        ** Properties
        */

        /// <summary>
        /// Gets or sets the number of preambles to send in the MDC1200 data stream.
        /// </summary>
        public int NumberOfPreambles
        {
            get;
            set;
        }

        /*
        ** Methods
        */

        /// <summary>
        /// Initializes a new instance of the <see cref="MDCEncoder"/> class.
        /// </summary>
        /// <param name="sampleRate">Audio Sample Rate</param>
        public MDCEncoder(int sampleRate)
        {
/*
            if (sampleRate == 8000)
            {
                this.incru = 644245094;
                this.incru18 = 966367642;
            }
            else if (sampleRate == 16000)
            {
                this.incru = 322122547;
                this.incru18 = 483183820;
            }
            else if (sampleRate == 22050)
            {
                this.incru = 233739716;
                this.incru18 = 350609575;
            }
            else if (sampleRate == 32000)
            {
                this.incru = 161061274;
                this.incru18 = 241591910;
            }
            else if (sampleRate == 44100)
            {
                this.incru = 116869858;
                this.incru18 = 175304788;
            }
            else if (sampleRate == 48000)
            {
                this.incru = 107374182;
                this.incru18 = 161061274;
            }
            else
            {
*/
                // WARNING: lower precision than above
                this.incru = (uint)(1200 * 2 * (int.MaxValue / sampleRate) + 800);
                this.incru18 = (uint)(1800 * 2 * (int.MaxValue / sampleRate) + 800);
/*
            }
*/
            this.sampleRate = sampleRate;
            this.NumberOfPreambles = DEFAULT_PREAMBLES;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Reset()
        {
            this.loaded = 0;
            this.dataArray = new byte[0];
        }

        /// <summary>
        /// Internal function to generate and add a preamble.
        /// </summary>
        /// <param name="array">Array to load into</param>
        /// <param name="offset">Offset in array to add bit sync</param>
        /// <returns>Number of bytes loaded into array</returns>
        private int InsertPreamble(ref byte[] array, int offset)
        {
            // standard Mot 00-00-00-AA-AA-AA-AA
            array[offset] = 0x00;
            array[offset + 1] = 0x00;
            array[offset + 2] = 0x00;
            array[offset + 3] = 0xAA;
            array[offset + 4] = 0xAA;
            array[offset + 5] = 0xAA;
            array[offset + 6] = 0xAA;
            return 7;
        }

        /// <summary>
        /// Internal function to generate and add the bit sync data.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        private int InsertBitSync(ref byte[] array, int offset)
        {
            array[offset] = 0x07;
            array[offset + 1] = 0x09;
            array[offset + 2] = 0x2A;
            array[offset + 3] = 0x44;
            array[offset + 4] = 0x6F;
            return 5;
        }

        /// <summary>
        /// Internal function to generate the MDC1200 header (preambles + 40 bit sync codeword)
        /// </summary>
        /// <param name="array">Array to load header into</param>
        /// <param name="bytesLoaded">Number of bytes loaded</param>
        /// <param name="preambles">Number of 24-bit preambles to insert</param>
        /// <param name="singlePacket">Flag that determines whether or not this is a single or double packet MDC1200 stream</param>
        /// <param name="overrideLength"></param>
        private void GenerateHeader(ref byte[] array, ref int bytesLoaded, int preambles = 1, bool singlePacket = false, int overrideLength = 0)
        {
            // determine codeword length
            int codewordLength = 0;
            if (overrideLength != 0)
                codewordLength = (CODEWORD_LENGTH * overrideLength);
            else
            {
                if (singlePacket)
                    codewordLength = (CODEWORD_LENGTH);
                else
                    codewordLength = (CODEWORD_LENGTH * 2);
            }

            // determine data stream size
            int dataSize = (codewordLength) + (BITSYNC_LENGTH);
            if (overrideLength != 0)
                dataSize = (codewordLength) + (BITSYNC_LENGTH * overrideLength);
            if (preambles >= 1)
                dataSize += (PREAMBLE_LENGTH * preambles);

            // initialize a new data array
            this.dataArray = new byte[dataSize];
            this.loaded = 0;
            this.state = false;

            // insert preambles
            if (preambles > 0)
                for (int i = 0; i < preambles; i++)
                    bytesLoaded += InsertPreamble(ref array, bytesLoaded);

            // insert bit sync
            bytesLoaded += InsertBitSync(ref array, bytesLoaded);
        }

        /// <summary>
        /// Create a MDC1200 header.
        /// </summary>
        /// <param name="noPreamble"></param>
        /// <param name="overrideLength"></param>
        public void GenerateHeader(bool noPreamble = false, int overrideLength = 0)
        {
            // initialize MDC1200 header
            if (noPreamble)
                GenerateHeader(ref dataArray, ref loaded, 0, true, overrideLength);
            else
                GenerateHeader(ref dataArray, ref loaded, NumberOfPreambles, true, overrideLength);
        }

        /// <summary>
        /// 
        /// </summary>
        public void InjectSyncOnly()
        {
            loaded += InsertBitSync(ref dataArray, loaded);
        }

        /// <summary>
        /// Internal function to pack 32-bit data codewords.
        /// </summary>
        /// <param name="data">Data to pack</param>
        private void PackData(byte[] data)
        {
            ushort ccrc;
            int[] csr = new int[7];
            int[] lbits = new int[MDCDetector.MAX_MDC1200_BITS];

            // generate CRC and add data at position 4 and 5
            ccrc = ComputeCRC(data, 4);
            data[4] = (byte)(ccrc & 0x00ff);
            data[5] = (byte)((ccrc >> 8) & 0x00ff);

            // insert data pad at position 6
            data[6] = 0x00;

            // convolutional encoding for error detection - deep magic
            for (int i = 0; i < 7; i++)
                csr[i] = 0;

            for (int i = 0; i < 7; i++)
            {
                data[i + 7] = 0;
                for (int j = 0; j <= 7; j++)
                {
                    for (int k = 6; k > 0; k--)
                        csr[k] = csr[k - 1];

                    csr[0] = (data[i] >> j) & 0x01;
                    int b = csr[0] + csr[2] + csr[5] + csr[6];

                    data[i + 7] |= (byte)((b & 0x01) << j);
                }
            }

            // do deep magic bit manipulation
            int l = 0, m = 0;
            for (int i = 0; i < 14; i++)
            {
                // loop through 8 bits (for each byte)
                for (int j = 0; j <= 7; j++)
                {
                    // get if the bit is set
                    lbits[l] = 0x01 & (data[i] >> j);
                    l += 16;
                    if (l > 111)
                        l = ++m;
                }
            }

            l = 0;
            for (int i = 0; i < 14; i++)
            {
                data[i] = 0;

                // loop through 8 bits (for each byte)
                for (int j = 7; j >= 0; j--)
                {
                    // is this bit set?
                    if (lbits[l] != 0)
                        data[i] |= (byte)(1 << j);
                    ++l;
                }
            }
        }

        /// <summary>
        /// Create a single packet MDC1200 data array.
        /// </summary>
        /// <param name="packet">MDC packet to create</param>
        /// <param name="noPreamble"></param>
        public void CreateSingle(MDCPacket packet, bool noPreamble = false)
        {
            // initialize MDC1200 header
            if (noPreamble)
                GenerateHeader(ref dataArray, ref loaded, 0, true);
            else
                GenerateHeader(ref dataArray, ref loaded, NumberOfPreambles, true);

            CreateSingleNoHeader(packet);
        }

        /// <summary>
        /// Create a single packet MDC1200 data array.
        /// </summary>
        /// <param name="packet">MDC packet to create</param>
        public void CreateSingleNoHeader(MDCPacket packet)
        {
            // create a temporary array DATA_SIZE - 12 (stripping the size used by
            // the header)
            byte[] dp = new byte[CODEWORD_LENGTH];

            // fill the first bytes with the proper opcode and argument data
            dp[0] = packet.Operation;
            dp[1] = packet.Argument;
            dp[2] = (byte)((packet.Target >> 8) & 0x00FF);
            dp[3] = (byte)(packet.Target & 0x00FF);

            // pack data
            PackData(dp);

            // block copy new data into MDC1200 data array
            Buffer.BlockCopy(dp, 0, this.dataArray, loaded, dp.Length);
            this.loaded += 14;
        }

        /// <summary>
        /// Create a double packet MDC1200 data array.
        /// </summary>
        /// <param name="first">First MDC packet to create</param>
        /// <param name="second">First MDC packet to create</param>
        /// <param name="noPreamble"></param>
        public void CreateDouble(MDCPacket first, MDCPacket second, bool noPreamble = false)
        {
            // initialize MDC1200 header
            if (noPreamble)
                GenerateHeader(ref dataArray, ref loaded, 0);
            else
                GenerateHeader(ref dataArray, ref loaded, NumberOfPreambles);

            // create a temporary array DATA_SIZE - 12 (stripping the size used by
            // the header)
            byte[] dp = new byte[CODEWORD_LENGTH];

            // fill the first bytes with the proper opcode and argument data
            dp[0] = first.Operation;
            dp[1] = first.Argument;
            dp[2] = (byte)((first.Target >> 8) & 0x00FF);
            dp[3] = (byte)(first.Target & 0x00FF);

            // pack data
            PackData(dp);

            // block copy new data into MDC1200 data array
            Buffer.BlockCopy(dp, 0, this.dataArray, loaded, dp.Length);
            this.loaded += 14;

            // clear temporary buffer
            dp = new byte[CODEWORD_LENGTH];

            // fill the first bytes with the proper opcode and argument data
            dp[0] = second.Operation;
            dp[1] = second.Argument;
            dp[2] = (byte)((second.Target >> 8) & 0x00FF);
            dp[3] = (byte)(second.Target & 0x00FF);

            // pack data
            PackData(dp);

            // block copy new data into MDC1200 data array
            Buffer.BlockCopy(dp, 0, this.dataArray, loaded, dp.Length);
            this.loaded += 14;
        }

        /// <summary>
        /// Internal function to get a single audio sample.
        /// </summary>
        /// <returns>Single audio sample</returns>
        private byte GetSample()
        {
            int b, ofs;

            uint lthu = this.thu;
            this.thu += this.incru;

            // wrap
            if (this.thu < lthu)
            {
                this.ipos++;
                if (this.ipos > 7)
                {
                    this.ipos = 0;
                    this.bpos++;
                    if (bpos >= loaded)
                    {
                        this.state = false;
                        return sinTable[127];
                    }
                }

                b = 0x01 & (dataArray[bpos] >> (7 - (ipos)));
                if (b != this.lb)
                {
                    this.xorBit = true;
                    this.lb = b;
                }
                else
                    this.xorBit = false;
            }

            // are we XORing the bits?
            if (xorBit)
                this.tthu += incru18;
            else
                this.tthu += incru;

            ofs = (int)(tthu >> 24);
            return sinTable[ofs];
        }

        /// <summary>
        /// Get raw audio samples from the encoder.
        /// </summary>
        /// <returns></returns>
        public byte[] GetSamples()
        {
            byte[] tmpBuffer = new byte[262144];

            // make sure we have loaded data
            if (this.loaded <= 0)
                return null;

            // check the current state
            if (!this.state)
            {
                this.thu = 0;
                this.tthu = 0;
                this.bpos = 0;
                this.ipos = 0;
                this.state = true;
                this.xorBit = true;
                this.lb = 0;
            }

            int i = 0;
            while ((i < tmpBuffer.Length) && this.state)
                tmpBuffer[i++] = GetSample();

            byte[] buffer = new byte[i];
            Buffer.BlockCopy(tmpBuffer, 0, buffer, 0, i);

            if (this.state == false)
                this.loaded = 0;

            Reset();
            return buffer;
        }
    } // public class MDCEncoder : MDCCRC
} // namespace dvmbridge.MDC1200

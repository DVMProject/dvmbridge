﻿/**
* Digital Voice Modem - Bridge
* AGPLv3 Open Source. Use is subject to license terms.
* DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
*
* @package DVM / Bridge
*
*/
/*
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
using System.Runtime.InteropServices;

#if WIN32
namespace dvmbridge
{
    /// <summary>
    /// Implements P/Invoke to callback into external AMBE encoder/decoder library.
    /// </summary>
    /// <remarks>This is used to interface to a external library that talks to a DVSI USB-3000.</remarks>
    public class AmbeVocoder
    {
        private const int MBE_SAMPLES_LENGTH = 160;
        private const short NO_BIT_STEAL = 0;

        private const ushort ECMODE_NOISE_SUPPRESS = 0x40;
        private const ushort ECMODE_AGC = 0x2000;

        private const int DECSTATE_SIZE = 2048;
        private byte[] decoderState;
        private const int ENCSTATE_SIZE = 6144;
        private byte[] encoderState;

        /// <summary>
        /// 
        /// </summary>
        public enum AmbeMode : short
        {
            FULL_RATE = 0x00,
            HALF_RATE = 0x01,

            NOT_VALID = 0x03
        } // public enum AmbeMode

        private AmbeMode mode;
        private int frameLengthInBytes;
        private int frameLengthInBits;

        private ushort dcmode;
        private ushort ecmode;

        /*
        ** Properties
        */

        /// <summary>
        /// Gets the currently operating decoder mode.
        /// </summary>
        public AmbeMode DecoderMode
        {
            get
            {
                unsafe
                {
                    fixed (byte* state = decoderState)
                        return (AmbeMode)ambe_get_dec_mode((IntPtr)state);
                }
            }
        }

        /// <summary>
        /// Gets the currently operating encoder mode.
        /// </summary>
        public AmbeMode EncoderMode
        {
            get
            {
                unsafe
                {
                    fixed (byte* state = encoderState)
                        return (AmbeMode)ambe_get_enc_mode((IntPtr)state);
                }
            }
        }

        /*
        ** Methods
        */

        /// <summary>
        /// Initializes a new instance of the <see cref="AmbeVocoder"/> class.
        /// </summary>
        /// <param name="fullRate"></param>
        public AmbeVocoder(bool fullRate = true)
        {
            if (fullRate)
            {
                this.mode = AmbeMode.FULL_RATE;
                this.frameLengthInBits = 88;
                this.frameLengthInBytes = 11;
            }
            else
            {
                this.mode = AmbeMode.HALF_RATE;
                this.frameLengthInBits = 49;
                this.frameLengthInBytes = 7;
            }

            this.decoderState = new byte[DECSTATE_SIZE];
            this.dcmode = 0;

            this.encoderState = new byte[ENCSTATE_SIZE];
            this.ecmode = ECMODE_NOISE_SUPPRESS | ECMODE_AGC;

            unsafe
            {
                // initialize the AMBE decoder state
                fixed (byte* state = decoderState)
                    ambe_init_dec((IntPtr)state, (short)mode);

                // initialize the AMBE encoder state
                fixed (byte* state = encoderState)
                    ambe_init_enc((IntPtr)state, (short)mode, 1);
            }
        }

        /// <summary>
        /// Helper to unpack the codeword bytes into codeword bits for use with the AMBE decoder.
        /// </summary>
        /// <param name="codewordBits">Codeword bits.</param>
        /// <param name="codeword">Codeword bytes.</param>
        /// <param name="lengthBytes">Length of codeword in bytes.</param>
        /// <param name="lengthBits">Length of codeword in bits.</param>
        private void unpackBytesToBits(out short[] codewordBits, byte[] codeword, int lengthBytes, int lengthBits)
        {
            codewordBits = new short[this.frameLengthInBits * 2];

            int processed = 0, bitPtr = 0, bytePtr = 0;
            for (int i = 0; i < lengthBytes; i++)
            {
                for (int j = 7; -1 < j; j--)
                {
                    if (processed < lengthBits)
                    {
                        codewordBits[bitPtr] = (short)((codeword[bytePtr] >> ((byte)j & 0x1F)) & 1);
                        bitPtr++;
                    }

                    processed++;
                }

                bytePtr++;
            }
        }

        /// <summary>
        /// Decodes the given MBE codewords to PCM samples using the decoder mode.
        /// </summary>
        /// <param name="codeword"></param>
        /// <param name="samples"></param>
        public int decode(byte[] codeword, out short[] samples)
        {
            samples = new short[MBE_SAMPLES_LENGTH];

            if (codeword == null)
                throw new NullReferenceException("codeword");

            // unpack codeword from bytes to bits for use with external library
            short[] codewordBits = null;
            unpackBytesToBits(out codewordBits, codeword, frameLengthInBytes, frameLengthInBits);

            short[] n0 = new short[MBE_SAMPLES_LENGTH / 2];
            short[] n1 = new short[MBE_SAMPLES_LENGTH / 2];

            // perform P/Invoke callback and pointer pinning and callback into external library
            unsafe
            {
                fixed (short* c = codewordBits)
                fixed (byte* state = decoderState)
                {
                    IntPtr codewordPtr = (IntPtr)c;

                    // sample segment 1
                    GCHandle pinnedN0 = GCHandle.Alloc(n0, GCHandleType.Pinned);
                    IntPtr n0Ptr = pinnedN0.AddrOfPinnedObject();
                    ambe_voice_dec(n0Ptr, MBE_SAMPLES_LENGTH / 2, codewordPtr, NO_BIT_STEAL, dcmode, 0, (IntPtr)state);
                    pinnedN0.Free();

                    // sample segment 2
                    GCHandle pinnedN1 = GCHandle.Alloc(n1, GCHandleType.Pinned);
                    IntPtr n1Ptr = pinnedN1.AddrOfPinnedObject();
                    ambe_voice_dec(n1Ptr, MBE_SAMPLES_LENGTH / 2, codewordPtr, NO_BIT_STEAL, dcmode, 1, (IntPtr)state);
                    pinnedN1.Free();
                }
            }

            // combine sample segments into contiguous samples
            for (int i = 0; i < MBE_SAMPLES_LENGTH / 2; i++)
                samples[i] = n0[i];
            for (int i = 0; i < MBE_SAMPLES_LENGTH / 2; i++)
                samples[i + (MBE_SAMPLES_LENGTH / 2)] = n1[i];

            return 0;
        }

        /// <summary>
        /// Calls ambe_init_dec() in the external DLL.
        /// </summary>
        /// <param name="state">Buffer containing the decoder state to initialize.</param>
        /// <param name="mode">AMBE mode; FULL (0) or HALF (1).</param>
        [DllImport("AMBE.DLL", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern void ambe_init_dec([Out] IntPtr state, [In] short mode);

        /// <summary>
        /// Calls ambe_get_dec_mode() in the external DLL.
        /// </summary>
        /// <param name="state">Buffer containing the decoder state to initialize.</param>
        [DllImport("AMBE.DLL", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern short ambe_get_dec_mode([In] IntPtr state);

        /// <summary>
        /// Calls ambe_voice_dec() in the external DLL.
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="sampleLength"></param>
        /// <param name="codeword"></param>
        /// <param name="bitSteal"></param>
        /// <param name="cmode"></param>
        /// <param name="n"></param>
        /// <param name="state">Buffer containing the decoder state.</param>
        /// <returns></returns>
        [DllImport("AMBE.DLL", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern uint ambe_voice_dec([Out] IntPtr samples, [In] short sampleLength, [In] IntPtr codeword, [In] short bitSteal, [In] ushort cmode, [In] short n, [In] IntPtr state);

        /// <summary>
        /// Helper to pack the codeword bits into codeword bytes for use with the AMBE encoder.
        /// </summary>
        /// <param name="codewordBits">Codeword bits.</param>
        /// <param name="codeword">Codeword bytes.</param>
        /// <param name="lengthBytes">Length of codeword in bytes.</param>
        /// <param name="lengthBits">Length of codeword in bits.</param>
        private void packBitsToBytes(short[] codewordBits, out byte[] codeword, int lengthBytes, int lengthBits)
        {
            codeword = new byte[lengthBytes];

            int processed = 0, bitPtr = 0, bytePtr = 0;
            for (int i = 0; i < lengthBytes; i++)
            {
                codeword[i] = 0;
                for (int j = 7; -1 < j; j--)
                {
                    if (processed < lengthBits)
                    {
                        codeword[bytePtr] = (byte)(codeword[bytePtr] | (byte)((codewordBits[bitPtr] & 1) << ((byte)j & 0x1F)));
                        bitPtr++;
                    }

                    processed++;
                }

                bytePtr++;
            }
        }

        /// <summary>
        /// Encodes the given PCM samples using the encoder mode to MBE codewords.
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="codeword"></param>
        public void encode(short[] samples, out byte[] codeword)
        {
            codeword = new byte[this.frameLengthInBytes];

            if (samples == null)
                throw new NullReferenceException("samples");

            if (samples.Length > MBE_SAMPLES_LENGTH)
                throw new ArgumentOutOfRangeException($"Samples length is > {MBE_SAMPLES_LENGTH}");
            if (samples.Length < MBE_SAMPLES_LENGTH)
                throw new ArgumentOutOfRangeException($"Samples length is < {MBE_SAMPLES_LENGTH}");

            short[] codewordBits = new short[this.frameLengthInBits * 2];

            // split samples into 2 segments
            short[] n0 = new short[MBE_SAMPLES_LENGTH / 2];
            for (int i = 0; i < MBE_SAMPLES_LENGTH / 2; i++)
                n0[i] = samples[i];

            short[] n1 = new short[MBE_SAMPLES_LENGTH / 2];
            for (int i = 0; i < MBE_SAMPLES_LENGTH / 2; i++)
                n1[i] = samples[i + (MBE_SAMPLES_LENGTH / 2)];

            // perform P/Invoke callback and pointer pinning and callback into external library
            unsafe
            {
                fixed (short* c = codewordBits)
                fixed (byte* state = encoderState)
                {
                    IntPtr codewordPtr = (IntPtr)c;

                    // sample segment 1
                    GCHandle pinnedN0 = GCHandle.Alloc(n0, GCHandleType.Pinned);
                    IntPtr n0Ptr = pinnedN0.AddrOfPinnedObject();
                    ambe_voice_enc(codewordPtr, NO_BIT_STEAL, n0Ptr, MBE_SAMPLES_LENGTH / 2, ecmode, 0, 8192, (IntPtr)state);
                    pinnedN0.Free();

                    // sample segment 2
                    GCHandle pinnedN1 = GCHandle.Alloc(n1, GCHandleType.Pinned);
                    IntPtr n1Ptr = pinnedN1.AddrOfPinnedObject();
                    ambe_voice_enc(codewordPtr, NO_BIT_STEAL, n1Ptr, MBE_SAMPLES_LENGTH / 2, ecmode, 1, 8192, (IntPtr)state);
                    pinnedN1.Free();
                }
            }

            // pack codeword from bits to bytes for use with external library
            packBitsToBytes(codewordBits, out codeword, frameLengthInBytes, frameLengthInBits);
        }

        /// <summary>
        /// Calls ambe_init_enc() in the external DLL.
        /// </summary>
        /// <param name="state">Buffer containing the encoder state to initialize.</param>
        /// <param name="mode">AMBE mode; FULL (0) or HALF (1).</param>
        /// <param name="initialize">Flag to initialize encoder state fully, 1 to initialize, 0 to not.</param>
        [DllImport("AMBE.DLL", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern void ambe_init_enc([Out] IntPtr state, [In] short mode, [In] short initialize);

        /// <summary>
        /// Calls ambe_get_enc_mode() in the external DLL.
        /// </summary>
        /// <param name="state">Buffer containing the encoder state to initialize.</param>
        [DllImport("AMBE.DLL", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern short ambe_get_enc_mode([In] IntPtr state);

        /// <summary>
        /// Calls ambe_voice_enc() in the external DLL.
        /// </summary>
        /// <param name="codeword"></param>
        /// <param name="bitSteal"></param>
        /// <param name="samples"></param>
        /// <param name="sampleLength"></param>
        /// <param name="cmode"></param>
        /// <param name="n"></param>
        /// <param name="unk"></param>
        /// <param name="state">Buffer containing the encoder state.</param>
        /// <returns></returns>
        [DllImport("AMBE.DLL", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern uint ambe_voice_enc([Out] IntPtr codeword, [In] short bitSteal, [In] IntPtr samples, [In] short sampleLength, [In] ushort cmode, [In] short n, [In] short unk, [In] IntPtr state);
    } // public class AmbeVocoder
} // namespace dvmbridge
#endif
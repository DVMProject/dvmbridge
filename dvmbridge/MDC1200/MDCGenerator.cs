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

using NAudio.Wave;

namespace dvmbridge.MDC1200
{
    /// <summary>
    /// Implements a sample provider that synthesizes audio for MDC1200 packet(s).
    /// </summary>
    public class MDCGenerator : ISampleProvider
    {
        private const int PREAMBLE_LEN_MS = 3;
        private const int PACKET_LEN_MS = 173;
        private const int SILENCE_MS = 75;
        private const float GAIN = 0.95f;

        private MDCEncoder encoder;
        private readonly WaveFormat waveFormat;

        private bool doublePacket = false;
        private bool mdcGenerated = false;

        /*
        ** Properties
        */

        /// <summary>
        /// The waveformat of this WaveProvider (same as the source)
        /// </summary>
        public WaveFormat WaveFormat
        {
            get { return waveFormat; }
        }

        /// <summary>
        /// Gets or sets the number of preambles to send in the MDC1200 data stream.
        /// </summary>
        public int NumberOfPreambles
        {
            get { return encoder.NumberOfPreambles; }
            set { encoder.NumberOfPreambles = value; }
        }

        /// <summary>
        /// Flag indicating whether or not we should inject silence before the synthesized audio.
        /// </summary>
        public bool InjectSilenceLeader { get; set; }

        /// <summary>
        /// Flag indicating whether or not we should inject silence after the synthesized audio.
        /// </summary>
        public bool InjectSilenceTail { get; set; }

        /// <summary>
        /// Flag indicating whether or not we have synthesized samples queued.
        /// </summary>
        public bool HasSamples { get { return mdcGenerated; } }

        /*
        ** Methods
        */

        /// <summary>
        /// Initializes a new instance of the <see cref="DPLGenerator"/> class.
        /// </summary>
        /// <param name="sampleRate"></param>
        public MDCGenerator(int sampleRate)
        {
            waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1);
            this.encoder = new MDCEncoder(sampleRate);
            this.InjectSilenceLeader = false;
            this.InjectSilenceTail = false;
        }

        /// <summary>
        /// Synthesizes the given MDC packets.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="noPreamble"></param>
        public void GenerateMDC(MDCPacket first, MDCPacket second = null, bool noPreamble = false)
        {
            if (first == null)
                throw new ArgumentNullException();
            if (second != null)
            {
                encoder.CreateDouble(first, second, noPreamble);
                doublePacket = true;
            }
            else
            {
                encoder.CreateSingle(first, noPreamble);
                doublePacket = false;
            }
            mdcGenerated = true;
        }

        /// <summary>
        /// Gets a byte array of samples with the encoded MDC packet.
        /// </summary>
        /// <returns></returns>
        public byte[] GetSamples()
        {
            if (!mdcGenerated)
                return null;

            int duration = 0;
            if (doublePacket)
                duration = (PACKET_LEN_MS * 2);
            else
                duration = PACKET_LEN_MS;
            duration += (PREAMBLE_LEN_MS * NumberOfPreambles);

            if (InjectSilenceLeader)
                duration += SILENCE_MS;
            if (InjectSilenceTail)
                duration += SILENCE_MS;

            SampleToAudioProvider16 smpTo16 = new SampleToAudioProvider16(this);

            int sDuration = SampleTimeConvert.MSToSampleBytes(waveFormat, duration);
            byte[] sigBuf = new byte[sDuration];
            smpTo16.Read(sigBuf, 0, sDuration);

            mdcGenerated = false;
            return sigBuf;
        }

        /// <summary>
        /// Reads from this provider.
        /// </summary>
        public int Read(float[] buffer, int offset, int count)
        {
            if (!mdcGenerated)
                return 0;

            int outIndex = offset;
            int nSample = 0;

            // read encoder bytes
            byte[] mdcBuffer = encoder.GetSamples();
            if (mdcBuffer != null)
            {
                if (InjectSilenceLeader)
                {
                    byte[] tmp = new byte[mdcBuffer.Length];
                    Buffer.BlockCopy(mdcBuffer, 0, tmp, 0, mdcBuffer.Length);

                    // generate silence
                    int sDuration = SampleTimeConvert.MSToSampleBytes(WaveFormat, SILENCE_MS);
                    mdcBuffer = new byte[tmp.Length + sDuration];

                    for (int i = 0; i < sDuration; i++)
                        mdcBuffer[i] = 128; // 128 should be audio "zero" in byte format

                    // inject original MDC audio
                    Buffer.BlockCopy(tmp, 0, mdcBuffer, sDuration, tmp.Length);
                }

                if (InjectSilenceTail)
                {
                    byte[] tmp = new byte[mdcBuffer.Length];
                    Buffer.BlockCopy(mdcBuffer, 0, tmp, 0, mdcBuffer.Length);

                    // generate silence
                    int sDuration = SampleTimeConvert.MSToSampleBytes(WaveFormat, SILENCE_MS);
                    mdcBuffer = new byte[tmp.Length + 1 + sDuration];

                    // inject original MDC audio
                    Buffer.BlockCopy(tmp, 0, mdcBuffer, 0, tmp.Length);

                    // insert silence
                    for (int i = tmp.Length + 1; i < (tmp.Length + 1 + sDuration); i++)
                        mdcBuffer[i] = 128; // 128 should be audio "zero" in byte format
                }

                // complete buffer
                for (int sampleCount = 0; sampleCount < count / waveFormat.Channels; sampleCount++)
                {
                    if (nSample >= mdcBuffer.Length)
                        buffer[outIndex++] = 0;
                    else
                    {
                        // convert 8-bit MDC to 16-bit
                        buffer[outIndex++] = GAIN * (mdcBuffer[nSample] / 128f - 1.0f);
                        nSample++;
                    }
                }
                return count;
            }
            else
                return 0;
        }
    } // public class MDCGenerator : ISampleProvider
} // namespace dvmbridge.MDC1200

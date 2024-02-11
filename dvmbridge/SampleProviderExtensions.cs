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
*
*/
using System;

using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace dvmbridge
{
    /// <summary>
    /// Class extension for <see cref="ISampleProvider"/>.
    /// </summary>
    public static class SampleProviderExtensions
    {
        /**
         * Methods
         */
        /// <summary>
        /// Extension function to resample this provider into another sample rate.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sampleRate"></param>
        public static ISampleProvider SampleWith(this ISampleProvider source, int sampleRate)
        {
            return source.WaveFormat.SampleRate != sampleRate ? new WdlResamplingSampleProvider(source, sampleRate) : source;
        }

        /// <summary>
        /// Extension function to convert this sample provider to mono.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static ISampleProvider AsMono(this ISampleProvider source)
        {
            return source.WaveFormat.Channels != 1 ? new MultiplexingSampleProvider(new[] { source }, 1) : source;
        }
    } // public static class SampleProviderExtensions
} // namespace dvmbridge

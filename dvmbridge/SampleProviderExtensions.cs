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

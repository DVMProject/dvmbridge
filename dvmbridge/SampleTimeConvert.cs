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

namespace dvmbridge
{
    /// <summary>
    /// 
    /// </summary>
    public class SampleTimeConvert
    {
        /*
        ** Methods
        */

        /// <summary>
        /// (ms) to sample count conversion
        /// </summary>
        /// <param name="format">Wave format</param>
        /// <param name="ms">Number of milliseconds</param>
        /// <returns>Number of samples</returns>
        public static int ToSamples(WaveFormat format, int ms)
        {
            return (int)(((long)ms) * format.SampleRate * format.Channels / 1000);
        }

        /// <summary>
        /// Sample count to (ms) conversion
        /// </summary>
        /// <param name="format">Wave format</param>
        /// <param name="samples">Number of samples</param>
        /// <returns>Number of milliseconds</returns>
        public static int ToMS(WaveFormat format, int samples)
        {
            return (int)(((float)samples / (float)format.SampleRate / (float)format.Channels) * 1000);
        }

        /// <summary>
        /// samples to bytes conversion
        /// </summary>
        /// <param name="format">Wave format</param>
        /// <param name="samples">Number of samples</param>
        /// <returns>Number of bytes for the number of samples</returns>
        public static int ToBytes(WaveFormat format, int samples)
        {
            return samples * (format.BitsPerSample / 8);
        }

        /// <summary>
        /// (ms) to bytes conversion
        /// </summary>
        /// <param name="format">Wave format</param>
        /// <param name="ms">Number of milliseconds</param>
        /// <returns>Number of bytes for the amount of audio in (ms)</returns>
        public static int MSToSampleBytes(WaveFormat format, int ms)
        {
            return ToBytes(format, ToSamples(format, ms));
        }
    } // public class SamplesToMS
} // namespace dvmbridge

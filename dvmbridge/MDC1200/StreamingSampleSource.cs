﻿/**
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
using System.Collections.Generic;
using System.Threading;

using NAudio.Wave;

namespace dvmbridge.MDC1200
{
    /// <summary>
    /// Interface defines an NAudio sample source.
    /// </summary>
    public interface ISampleSource
    {
        /**
         * Fields
         */
        /// <summary>
        /// Flag indicating whether the sample source has samples.
        /// </summary>
        bool HasSamples { get; }

        /// <summary>
        /// Gets the raw samples.
        /// </summary>
        IEnumerable<float> Samples { get; }
    } // public interface ISampleSource

    /// <summary>
    /// Implements a streaming sample source.
    /// </summary>
    public class StreamingSampleSource : ISampleSource
    {
        private readonly BufferedWaveProvider sourceBuffer;
        private readonly ISampleProvider samples;

        /*
        ** Properties
        */

        /// <summary>
        /// Flag indicating whether or not this source has samples.
        /// </summary>
        public bool HasSamples { get; } = true;

        /// <summary>
        /// Gets the raw samples.
        /// </summary>
        public IEnumerable<float> Samples
        {
            get
            {
                var buffer = new float[1];

                while (HasSamples)
                {
                    int bytesPerSample = sourceBuffer.WaveFormat.BitsPerSample / 8 * sourceBuffer.WaveFormat.Channels;

                    // wait for samples
                    while (sourceBuffer.BufferedBytes < bytesPerSample)
                        Thread.Sleep(1);

                    samples.Read(buffer, 0, 1);
                    yield return buffer[0];
                }
            }
        }

        /*
        ** Methods
        */

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamingSampleSource"/> class.
        /// </summary>
        /// <param name="source"></param>
        public StreamingSampleSource(BufferedWaveProvider source, int sampleRate)
        {
            sourceBuffer = source;
            samples = source.ToSampleProvider().AsMono().SampleWith(sampleRate);
        }

        /// <summary>
        /// Reads a sample off the source.
        /// </summary>
        /// <returns></returns>
        public float GetSample()
        {
            var buffer = new float[1];

            samples.Read(buffer, 0, 1);
            return buffer[0];
        }
    } // public class StreamingSampleSource : ISampleSource
} // namespace RepeaterController

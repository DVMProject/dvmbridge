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
using System.IO;
using System.Threading;

using NAudio.Wave;

namespace dvmbridge.MDC1200
{
    /// <summary>
    /// Implements a worker that waits for and decodes samples for a MDC1200 packet(s).
    /// </summary>
    public class MDCWaveIn : IDisposable
    {
        private WaveInEvent waveIn;
        private bool disposed = false;
        private Thread captureWorker;

        private static BufferedWaveProvider sourceBuffer;
        private readonly ISampleSource source;
        private readonly MDCDetector mdcDetector;

        /*
        ** Properties
        */

        /// <summary>
        /// Milliseconds for the buffer. Recommended value is 100ms
        /// </summary>
        public int BufferMilliseconds
        {
            get { return waveIn.BufferMilliseconds; }
            set { waveIn.BufferMilliseconds = value; }
        }

        /// <summary>
        /// Number of Buffers to use (usually 2 or 3)
        /// </summary>
        public int NumberOfBuffers
        {
            get { return waveIn.NumberOfBuffers; }
            set { waveIn.NumberOfBuffers = value; }
        }

        /// <summary>
        /// Gets the sample rate of the MDC1200 detector.
        /// </summary>
        public static int SampleRate { get; } = 8000;

        /// <summary>
        /// Flag indicating whether the live analyzer is capturing samples.
        /// </summary>
        public bool IsRecording { get; private set; }

        /*
        ** Events
        */

        /// <summary>
        /// Occurs when a MDC packet is successfully decoded.
        /// </summary>
        public event Action<object, int, MDCPacket, MDCPacket> MDCPacketDetected;

        /*
        ** Methods
        */

        /// <summary>
        /// Initializes a new instance of the <see cref="MDCWaveIn"/> class.
        /// </summary>
        /// <param name="waveFormat"></param>
        /// <param name="deviceNumber"></param>
        public MDCWaveIn(WaveFormat waveFormat, int deviceNumber)
        {
            this.waveIn = new WaveInEvent();
            this.waveIn.WaveFormat = waveFormat;
            this.waveIn.DeviceNumber = deviceNumber;

            this.mdcDetector = new MDCDetector(SampleRate);
            this.mdcDetector.DecoderCallback += MdcDecoder_DecoderCallback;
            this.source = new StreamingSampleSource(Buffer(waveIn), SampleRate);
        }

        /// <summary>
        /// Event that occurs when an MDC1200 packet is decoded.
        /// </summary>
        /// <param name="goodFrames"></param>
        /// <param name="first"></param>
        /// <param name="second"></param>
        private void MdcDecoder_DecoderCallback(int goodFrames, MDCPacket first, MDCPacket second)
        {
            MDCPacketDetected?.Invoke(this, goodFrames, first, second);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                if (IsRecording)
                    this.StopRecording();

                if (waveIn != null)
                    waveIn.Dispose();

                if (captureWorker != null)
                {
                    if (captureWorker.IsAlive)
                    {
                        captureWorker.Abort();
                        captureWorker.Join();
                    }

                    captureWorker = null;
                }
            }

            disposed = true;
        }

        /// <summary>
        /// Starts capturing samples for analysis.
        /// </summary>
        public void StartRecording()
        {
            if (IsRecording)
                return;

            waveIn.StartRecording();

            IsRecording = true;
            captureWorker = new Thread(Detect);
            captureWorker.Name = "MDCWaveIn";
            captureWorker.Start();
        }

        /// <summary>
        /// Stops capturing samples for analysis.
        /// </summary>
        public void StopRecording()
        {
            if (!IsRecording)
                return;

            IsRecording = false;
            captureWorker.Abort();
            captureWorker.Join();

            waveIn.StopRecording();
        }

        /// <summary>
        /// Manually process raw sample input byte array for an MDC-1200 packet.
        /// </summary>
        /// <param name="buffer">Buffer source containing samples</param>
        /// <param name="eightBitConvert">Converts a 16-bit input into 8-bit</param>
        /// <returns>Number of MDC1200 frames processed</returns>
        public void ProcessSamples(byte[] buffer, bool eightBitConvert = false)
        {
            if (eightBitConvert)
            {
                // this is slow -- awful and bad, what we're doing here is basically taking the source
                // 16-bit PCM stream, slapping a standard "WAV" RIFF header on it -- then using
                // NAudio's conversion stuff to change the bit-rate from 16 to 8 for our MDC decoder...
                WaveFormat format = new WaveFormat(SampleRate, 16, 1);
                WaveFormat toFormat = new WaveFormat(SampleRate, 8, 1);

                MemoryStream _waveStream = new MemoryStream();
                BinaryWriter bw = new BinaryWriter(_waveStream);
                bw.Write(new char[4] { 'R', 'I', 'F', 'F' });

                int length = 36 + buffer.Length;
                bw.Write(length);

                bw.Write(new char[8] { 'W', 'A', 'V', 'E', 'f', 'm', 't', ' ' });

                format.Serialize(bw);
                bw.Write(new char[4] { 'd', 'a', 't', 'a' });
                bw.Write(buffer.Length);
                bw.Write(buffer, 0, buffer.Length);
                _waveStream.Position = 0;

                WaveFileReader reader = new WaveFileReader(_waveStream);
                WaveFormatConversionStream streamProvider = new WaveFormatConversionStream(toFormat, reader);

                int len = SampleTimeConvert.MSToSampleBytes(toFormat, (int)reader.TotalTime.TotalMilliseconds);
                byte[] bSamples = new byte[len];
                streamProvider.Read(bSamples, 0, bSamples.Length);

                sourceBuffer.AddSamples(bSamples, 0, bSamples.Length);
            }
            else
                sourceBuffer.AddSamples(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Thread method for detecting MDC1200 packets.
        /// </summary>
        private void Detect()
        {
            while (true)
            {
                while (source.HasSamples)
                    mdcDetector.ProcessSamples(source.Samples);
            }
        }

        /// <summary>
        /// Helper to generate a <see cref="AudioWaveProvider"/> wave source.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private static BufferedWaveProvider Buffer(IWaveIn source)
        {
            sourceBuffer = new BufferedWaveProvider(source.WaveFormat) { DiscardOnBufferOverflow = true };
            source.DataAvailable += (sender, e) => sourceBuffer.AddSamples(e.Buffer, 0, e.BytesRecorded);

            return sourceBuffer;
        }
    } // public class MDCWaveIn
} // namespace dvmbridge.MDC1200

/**
* Digital Voice Modem - MBE Vocoder
* GPLv2 Open Source. Use is subject to license terms.
* DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
*
* @package DVM / MBE Vocoder
*
*/
/*
*   Copyright (C) 2023 by Bryan Biedenkapp N2PLL
*
*   This program is free software: you can redistribute it and/or modify
*   it under the terms of the GNU General Public License as published by
*   the Free Software Foundation, either version 3 of the License, or
*   (at your option) any later version.
*
*   This program is distributed in the hope that it will be useful,
*   but WITHOUT ANY WARRANTY; without even the implied warranty of
*   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*   GNU General Public License for more details.
*
*   You should have received a copy of the GNU General Public License
*   along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

#include "vocoder/MBEDecoder.h"
#include "Common.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace vocoder
{
    // ---------------------------------------------------------------------------
    //  Class Declaration
    //      Implements MBE audio decoding.
    // ---------------------------------------------------------------------------

    public ref class MBEDecoderManaged
    {
    public:
        static const int PCM_SAMPLES = 160;
        static const int AMBE_CODEWORD_SAMPLES = 9;
        static const int IMBE_CODEWORD_SAMPLES = 11;

        /// <summary>Initializes a new instance of the MBEDecoderManaged class.</summary>
        MBEDecoderManaged(MBEMode mode) :
            m_mode(mode)
        {
            switch (mode) {
            case MBEMode::DMRAMBE:
                m_decoder = new vocoder::MBEDecoder(vocoder::DECODE_DMR_AMBE);
                break;
            case MBEMode::IMBE:
            default:
                m_decoder = new vocoder::MBEDecoder(vocoder::DECODE_88BIT_IMBE);
                break;
            }
        }
        /// <summary>Finalizes a instance of the MBEDecoderManaged class.</summary>
        ~MBEDecoderManaged()
        {
            delete m_decoder;
        }

        /// <summary>Gets/sets the gain adjust for the MBE decoder.</summary>
        property float GainAdjust
        {
            float get() { return m_decoder->getGainAdjust(); }
            void set(float value) { m_decoder->setGainAdjust(value); }
        }

        /// <summary>Decodes the given MBE codewords to PCM samples using the decoder mode.</summary>
        Int32 decodeF(array<Byte>^ codeword, [Out] array<float>^% samples)
        {
            samples = nullptr;

            if (codeword == nullptr) {
                throw gcnew System::NullReferenceException("codeword");
            }

            // error check codeword length based on mode
            switch (m_mode) {
            case MBEMode::DMRAMBE:
            {
                if (codeword->Length > AMBE_CODEWORD_SAMPLES) {
                    throw gcnew System::ArgumentOutOfRangeException("AMBE codeword length is > 9");
                }

                if (codeword->Length < AMBE_CODEWORD_SAMPLES) {
                    throw gcnew System::ArgumentOutOfRangeException("AMBE codeword length is < 9");
                }
            }
            break;
            case MBEMode::IMBE:
            default:
            {
                if (codeword->Length > IMBE_CODEWORD_SAMPLES) {
                    throw gcnew System::ArgumentOutOfRangeException("IMBE codeword length is > 11");
                }

                if (codeword->Length < IMBE_CODEWORD_SAMPLES) {
                    throw gcnew System::ArgumentOutOfRangeException("IMBE codeword length is < 11");
                }
            }
            break;
            }

            // pin codeword byte array and decode into PCM samples
            pin_ptr<Byte> ppCodeword = &codeword[0];
            uint8_t* pCodeword = ppCodeword;

            float pcmSamples[PCM_SAMPLES];
            ::memset(pcmSamples, 0x00U, PCM_SAMPLES);
            int errs = m_decoder->decodeF(pCodeword, pcmSamples);

            // copy decoded PCM samples into the managed array
            samples = gcnew array<float>(PCM_SAMPLES);
            pin_ptr<float> ppSamples = &samples[0];
            for (int n = 0; n < PCM_SAMPLES; n++) {
                *ppSamples = pcmSamples[n];
                ppSamples++;
            }

            return errs;
        }

        /// <summary>Decodes the given MBE codewords to PCM samples using the decoder mode.</summary>
        Int32 decode(array<Byte>^ codeword, [Out] array<Int16>^% samples)
        {
            samples = nullptr;

            if (codeword == nullptr) {
                throw gcnew System::NullReferenceException("codeword");
            }

            // error check codeword length based on mode
            switch (m_mode) {
            case MBEMode::DMRAMBE:
            {
                if (codeword->Length > AMBE_CODEWORD_SAMPLES) {
                    throw gcnew System::ArgumentOutOfRangeException("AMBE codeword length is > 9");
                }

                if (codeword->Length < AMBE_CODEWORD_SAMPLES) {
                    throw gcnew System::ArgumentOutOfRangeException("AMBE codeword length is < 9");
                }
            }
            break;
            case MBEMode::IMBE:
            default:
            {
                if (codeword->Length > IMBE_CODEWORD_SAMPLES) {
                    throw gcnew System::ArgumentOutOfRangeException("IMBE codeword length is > 11");
                }

                if (codeword->Length < IMBE_CODEWORD_SAMPLES) {
                    throw gcnew System::ArgumentOutOfRangeException("IMBE codeword length is < 11");
                }
            }
            break;
            }

            // pin codeword byte array and decode into PCM samples
            pin_ptr<Byte> ppCodeword = &codeword[0];
            uint8_t* pCodeword = ppCodeword;

            int16_t pcmSamples[PCM_SAMPLES];
            ::memset(pcmSamples, 0x00U, PCM_SAMPLES);
            int errs = m_decoder->decode(pCodeword, pcmSamples);

            // copy decoded PCM samples into the managed array
            samples = gcnew array<Int16>(PCM_SAMPLES);
            pin_ptr<Int16> ppSamples = &samples[0];
            for (int n = 0; n < PCM_SAMPLES; n++) {
                *ppSamples = pcmSamples[n];
                ppSamples++;
            }

            return errs;
        }
    private:
        vocoder::MBEDecoder* m_decoder;
        MBEMode m_mode;
    };
} // namespace vocoder

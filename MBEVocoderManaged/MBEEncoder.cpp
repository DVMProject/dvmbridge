/**
* Digital Voice Modem - Transcode Software
* GPLv2 Open Source. Use is subject to license terms.
* DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
*
* @package DVM / Transcode Software
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

#include "vocoder/MBEEncoder.h"
#include "Common.h"

using namespace System;

namespace vocoder
{
    // ---------------------------------------------------------------------------
    //  Class Declaration
    //      Implements MBE audio encoding.
    // ---------------------------------------------------------------------------

    public ref class MBEEncoderManaged
    {
    public:
        static const int PCM_SAMPLES = 160;
        static const int AMBE_CODEWORD_SAMPLES = 9;
        static const int IMBE_CODEWORD_SAMPLES = 11;

        /// <summary>Initializes a new instance of the MBEEncoderManaged class.</summary>
        MBEEncoderManaged(MBEMode mode) :
            m_mode(mode)
        {
            switch (mode) {
            case MBEMode::DMRAMBE:
                m_encoder = new vocoder::MBEEncoder(vocoder::ENCODE_DMR_AMBE);
                break;
            case MBEMode::IMBE:
            default:
                m_encoder = new vocoder::MBEEncoder(vocoder::ENCODE_88BIT_IMBE);
                break;
            }
        }
        /// <summary>Finalizes a instance of the MBEEncoderManaged class.</summary>
        ~MBEEncoderManaged()
        {
            delete m_encoder;
        }

        /// <summary>Gets/sets the gain adjust for the MBE encoder.</summary>
        property float GainAdjust
        {
            float get() { return m_encoder->getGainAdjust(); }
            void set(float value) { m_encoder->setGainAdjust(value); }
        }

        /// <summary>Encodes the given PCM samples using the encoder mode to MBE codewords.</summary>
        void encode(array<Int16>^ samples, [Runtime::InteropServices::Out] array<Byte>^ codeword)
        {
            codeword = nullptr;

            if (samples == nullptr) {
                throw gcnew System::NullReferenceException("samples");
            }

            // error check samples length
            if (codeword->Length > PCM_SAMPLES) {
                throw gcnew System::ArgumentOutOfRangeException("samples length is > 160");
            }

            if (codeword->Length < PCM_SAMPLES) {
                throw gcnew System::ArgumentOutOfRangeException("samples length is < 160");
            }

            // pin samples array and encode into codewords
            int16_t pcmSamples[PCM_SAMPLES];
            ::memset(pcmSamples, 0x00U, PCM_SAMPLES);
            pin_ptr<Int16> ppSamples = &samples[0];
            ::memcpy(pcmSamples, ppSamples, PCM_SAMPLES);

            // encode samples
            switch (m_mode) {
            case MBEMode::DMRAMBE:
            {
                uint8_t codewords[AMBE_CODEWORD_SAMPLES];
                m_encoder->encode(pcmSamples, codewords);
            
                // copy encoded codewords into the managed array
                codeword = gcnew array<Byte>(AMBE_CODEWORD_SAMPLES);
                pin_ptr<Byte> ppCodeword = &codeword[0];
                ::memcpy(ppCodeword, codewords, AMBE_CODEWORD_SAMPLES);
            }
            break;
            case MBEMode::IMBE:
            default:
            {
                uint8_t codewords[IMBE_CODEWORD_SAMPLES];
                m_encoder->encode(pcmSamples, codewords);

                // copy encoded codewords into the managed array
                codeword = gcnew array<Byte>(IMBE_CODEWORD_SAMPLES);
                pin_ptr<Byte> ppCodeword = &codeword[0];
                ::memcpy(ppCodeword, codewords, IMBE_CODEWORD_SAMPLES);
            }
            break;
            }
        }

    private:
        vocoder::MBEEncoder* m_encoder;
        MBEMode m_mode;
    };
} // namespace vocoder

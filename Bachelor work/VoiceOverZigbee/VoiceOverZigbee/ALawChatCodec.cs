using System;
using System.Linq;
using NAudio.Wave;
using NAudio.Codecs;
using System.ComponentModel.Composition;

//пространство имен для кодека сжатия голоса
namespace VoiceOverZigbee.Codec
{
    //экспортируем из библиотеки
    [Export(typeof(INetworkChatCodec))]
    //создаем класс для кодека
    class ALawChatCodec : INetworkChatCodec
    {
        //метод гет для имени кодека
        public string Name
        {
            get { return "G.711 a-law"; }
        }
        //метод для формата
        public WaveFormat RecordFormat
        {
            get { return new WaveFormat(8000, 16, 1); }
        }
        //метод кодирования данных
        public byte[] Encode(byte[] data, int offset, int length)
        {
            byte[] encoded = new byte[length / 2];
            int outIndex = 0;
            for (int n = 0; n < length; n += 2)
            {
                encoded[outIndex++] = ALawEncoder.LinearToALawSample(BitConverter.ToInt16(data, offset + n));
            }
            return encoded;
        }
        //метод декодирования данных
        public byte[] Decode(byte[] data, int offset, int length)
        {
            byte[] decoded = new byte[length * 2];
            int outIndex = 0;
            for (int n = 0; n < length; n++)
            {
                short decodedSample = ALawDecoder.ALawToLinearSample(data[n + offset]);
                decoded[outIndex++] = (byte)(decodedSample & 0xFF);
                decoded[outIndex++] = (byte)(decodedSample >> 8);
            }
            return decoded;
        }

        public void Dispose()
        {
        }
    }
}

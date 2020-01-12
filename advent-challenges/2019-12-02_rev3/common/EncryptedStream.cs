using System;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace common
{
    public static class Utils
    {
        public static async Task ReadNBytes(this NetworkStream stream, byte[] buf, int offset, int count)
        {
            int result;
            while ((result = await stream.ReadAsync(buf, offset, count)) < count)
            {
                if (result <= 0)
                {
                    throw new Exception("Stream is no longer available");
                }
                offset += result;
                count -= result;
            }
        }

        public static string HexDump(byte[] bytes, int num, int bytesPerLine = 16)
        {
            if (bytes == null) return "<null>";
            int bytesLength = num;

            char[] HexChars = "0123456789ABCDEF".ToCharArray();

            int firstHexColumn =
                  8                   // 8 characters for the address
                + 3;                  // 3 spaces

            int firstCharColumn = firstHexColumn
                + bytesPerLine * 3       // - 2 digit for the hexadecimal value and 1 space
                + (bytesPerLine - 1) / 8 // - 1 extra space every 8 characters from the 9th
                + 2;                  // 2 spaces 

            int lineLength = firstCharColumn
                + bytesPerLine           // - characters to show the ascii value
                + Environment.NewLine.Length; // Carriage return and line feed (should normally be 2)

            char[] line = (new String(' ', lineLength - Environment.NewLine.Length) + Environment.NewLine).ToCharArray();
            int expectedLines = (bytesLength + bytesPerLine - 1) / bytesPerLine;
            StringBuilder result = new StringBuilder(expectedLines * lineLength);

            for (int i = 0; i < bytesLength; i += bytesPerLine)
            {
                line[0] = HexChars[(i >> 28) & 0xF];
                line[1] = HexChars[(i >> 24) & 0xF];
                line[2] = HexChars[(i >> 20) & 0xF];
                line[3] = HexChars[(i >> 16) & 0xF];
                line[4] = HexChars[(i >> 12) & 0xF];
                line[5] = HexChars[(i >> 8) & 0xF];
                line[6] = HexChars[(i >> 4) & 0xF];
                line[7] = HexChars[(i >> 0) & 0xF];

                int hexColumn = firstHexColumn;
                int charColumn = firstCharColumn;

                for (int j = 0; j < bytesPerLine; j++)
                {
                    if (j > 0 && (j & 7) == 0) hexColumn++;
                    if (i + j >= bytesLength)
                    {
                        line[hexColumn] = ' ';
                        line[hexColumn + 1] = ' ';
                        line[charColumn] = ' ';
                    }
                    else
                    {
                        byte b = bytes[i + j];
                        line[hexColumn] = HexChars[(b >> 4) & 0xF];
                        line[hexColumn + 1] = HexChars[b & 0xF];
                        line[charColumn] = '·';
                    }
                    hexColumn += 3;
                    charColumn++;
                }
                result.Append(line);
            }
            return result.ToString();
        }
    }

    public class EncryptedStream
    {

        private ICryptoTransform _aes;
        private byte[] _xor = new byte[16];
        private byte[] _current = new byte[16];
        int _pos = 0;
        uint _counter = 0;

        public EncryptedStream(byte[] xor)
        {
            var aes = Aes.Create();
            aes.Key = new byte[] { 0x1, 0x4, 0x43, 0x58, 0x18, 0xde, 0xf1, 0xf8, 0x9a, 0xa1, 0x0, 0x7f, 0x33, 0x78, 0x99, 0xa0 };
            aes.Mode = CipherMode.ECB;
            _aes = aes.CreateEncryptor();
            _counter = 0;
            xor.CopyTo(_xor, 0);
            Prepare();
        }

        private void Prepare()
        {
            byte[] counter = new byte[16];
            BitConverter.GetBytes(_counter).CopyTo(counter, 0);
            _aes.TransformBlock(counter, 0, 16, _current, 0);
        }

        public byte Next()
        {
            byte result = (byte)(_current[_pos] ^ _xor[_pos]);
            _pos++;
            if (_pos == 16)
            {
                _pos = 0;
                _counter++;
                if (_counter > 65535)
                {
                    // Arbitrarily limit stream to 1MB each side.
                    throw new Exception("Stream too long");
                }
                Prepare();
            }
            return result;
        }

        public void Transform(byte[] buf, int size)
        {
            // Console.WriteLine("Input:\n" + Utils.HexDump(buf, size));
            for (int i = 0; i < size; i++)
            {
                buf[i] ^= Next();
            }
            // Console.WriteLine("Output:\n" + Utils.HexDump(buf, size));
        }
    }

}

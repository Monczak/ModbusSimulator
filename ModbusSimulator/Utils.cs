using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusSimulator
{
    static class Utils
    {
        private static Random random;

        public static byte[] HexToBytes(string input)
        {
            byte[] result = new byte[input.Length / 2];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Convert.ToByte(input.Substring(i * 2, 2), 16);
            }
            return result;
        }

        public static byte[] BinStringToBytes(string input)
        {
            byte[] result = new byte[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                result[i] = Convert.ToByte(input[i]);
            }
            return result;
        }

        public static string BytesToHex(byte[] input)
        {
            StringBuilder builder = new StringBuilder();
            foreach (byte b in input)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }

        public static string FlipRandomBit(string input)
        {
            if (random == null)
                random = new Random();

            int selectedIndex = random.Next(input.Length);

            StringBuilder builder = new StringBuilder(input);

            int charIndex = input[selectedIndex];
            charIndex ^= (1 << random.Next(8));
            builder[selectedIndex] = (char)charIndex;

            return builder.ToString();
        }

        public static byte[] FlipRandomBit(byte[] input)
        {
            if (random == null)
                random = new Random();

            int selectedIndex = random.Next(input.Length);
            input[selectedIndex] = FlipRandomBit(input[selectedIndex]);

            return input;
        }

        public static byte FlipRandomBit(byte input)
        {
            if (random == null)
                random = new Random();

            return (byte)(input ^ (1 << random.Next(8)));
        }

        public static ushort FlipRandomBit(ushort input)
        {
            if (random == null)
                random = new Random();

            return (ushort)(input ^ (1 << random.Next(16)));
        }
    }
}

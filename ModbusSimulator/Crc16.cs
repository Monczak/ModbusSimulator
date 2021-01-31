using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusSimulator
{
    static class Crc16
    {
        const ushort polynomial = 0xA001;

        static readonly ushort[] buffer = new ushort[256];

        public static ushort GetChecksum(byte[] bytes)
        {
            ushort checksum = 0xFFFF;
            for (int i = 0; i < bytes.Length; i++)
            {
                byte index = (byte)(checksum ^ bytes[i]);
                checksum = (ushort)((checksum >> 8) ^ buffer[index]);
            }
            return checksum;
        }

        static Crc16()
        {
            for (ushort i = 0; i < buffer.Length; i++) 
            {
                ushort value = 0, tmp = i;
                for (byte j = 0; j < 8; j++)
                {
                    if (((value ^ tmp) & 0x1) != 0)
                    {
                        value = (ushort)((value >> 1) ^ polynomial);
                    }
                    else
                    {
                        value >>= 1;
                    }
                    tmp >>= 1;
                }
                buffer[i] = value;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusSimulator
{
    struct ModbusMessage
    {
        public byte address;
        public byte function;

        public byte[] data;
        public ushort checksum;

        public bool isFromMaster;

        public ModbusMessage(byte _address, byte _function, byte[] _data)
        {
            address = _address;
            function = _function;
            data = _data;
            checksum = Crc16.GetChecksum(new byte[2] { address, function }.Concat(data).ToArray());
            isFromMaster = false;
        }

        public static ModbusMessage FromBinary(byte[] input, bool hasChecksum = true)
        {
            byte[] checksum = input.Skip(input.Length - 2).Take(2).ToArray();

            ModbusMessage message = new ModbusMessage
            {
                address = input[0],
                function = input[1],
                data = input.Skip(2).Take(input.Length - (hasChecksum ? 4 : 2)).ToArray()
            };

            if (hasChecksum) message.checksum = (ushort)(checksum[0] << 8 + checksum[1]);
            else message.RecalculateChecksum();

            return message;
        }

        public static ModbusMessage FromBinary(string input, bool hasChecksum = true)
        {
            return FromBinary(Utils.BinStringToBytes(input), hasChecksum);
        }

        public static ModbusMessage FromASCII(string input, bool hasChecksum = true)
        {
            return FromBinary(Utils.HexToBytes(input), hasChecksum);
        }

        public byte[] ToBinary()
        {
            byte[] result = new byte[data.Length + 4];
            result[0] = address;
            result[1] = function;
            result[result.Length - 2] = (byte)(checksum >> 8);
            result[result.Length - 1] = (byte)(checksum & 0xFF);

            for (int i = 0; i < data.Length; i++)
                result[i + 2] = data[i];

            return result;
        }
        
        public string ToASCII()
        {
            return Utils.BytesToHex(ToBinary());
        }

        public void RecalculateChecksum()
        {
            checksum = Crc16.GetChecksum(new byte[2] { address, function }.Concat(data).ToArray());
        }

        public bool VerifyChecksum(out ushort actualChecksum)
        {
            actualChecksum = Crc16.GetChecksum(new byte[2] { address, function }.Concat(data).ToArray());
            return actualChecksum == checksum;
        }

        public string PrettyPrint()
        {
            string str = ToASCII();

            string dataStr = str.Substring(4, str.Length - 8);
            StringBuilder dataStrBuilder = new StringBuilder();
            for (int i = 0; i < dataStr.Length; i += 2)
            {
                dataStrBuilder.Append(dataStr.Substring(i, 2));
                dataStrBuilder.Append(" ");
            }


            return $"{str.Substring(0, 2)} | {str.Substring(2, 2)} | {dataStrBuilder.ToString()}| {str.Substring(str.Length - 4)}";
        }

        public static ModbusMessage CreateException(ModbusMessage message, ModbusExceptionType type)
        {
            return new ModbusMessage
            {
                address = message.address,
                function = (byte)(message.function + 0x80),
                data = new byte[1] { (byte)type }
            };
        }
    }
}

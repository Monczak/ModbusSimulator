using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusSimulator
{
    abstract class Device
    {
        public Bus bus;

        public byte id;

        public Device(byte _id)
        {
            id = _id;
        }

        public abstract void OnMessageReceived(ModbusMessage message);
    }
}

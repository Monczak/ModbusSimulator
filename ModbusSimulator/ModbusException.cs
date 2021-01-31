using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusSimulator
{

    [Serializable]
    public class ModbusException : Exception
    {
        public ModbusExceptionType Type;

        public ModbusException() { }
        public ModbusException(ModbusExceptionType type) : base(type.ToString()) { Type = type; }
        public ModbusException(ModbusExceptionType type, Exception inner) : base(type.ToString(), inner) { Type = type; }
        protected ModbusException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public enum ModbusExceptionType 
    {
        IllegalFunction = 1,
        IllegalDataAddress = 2,
        IllegalDataValue = 3,
        SlaveDeviceFailure = 4,
        Acknowledge = 5,
        SlaveDeviceBusy = 6,
        NegativeAcknowledge = 7,
        MemoryParityError = 8
    }

}

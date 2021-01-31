using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusSimulator
{
    struct ModbusProperties
    {
        public int masterTimeout;
        public byte slaveCount;
        public int slaveProcessingTime;
        public int slaveProcessingTimeJitter;
        public bool useChecksum;
        public float corruptionChance;
        public int corruptionAttempts;

        public ModbusProperties(bool defaultSettings = true)
        {
            masterTimeout = 5000;
            slaveCount = 5;
            slaveProcessingTime = 0;
            slaveProcessingTimeJitter = 0;
            useChecksum = false;
            corruptionChance = 0.01f;
            corruptionAttempts = 10;
        }
    }
}

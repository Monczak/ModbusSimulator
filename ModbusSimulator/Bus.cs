using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusSimulator
{
    sealed class Bus
    {
        public MasterDevice master;
        public Dictionary<short, SlaveDevice> slaves;

        public delegate void OnMessageDelegate(ModbusMessage message);
        public event OnMessageDelegate OnMessage;

        public float corruptionChance;
        public int corruptionAttempts;

        private Random random;

        public Bus(MasterDevice _master, float corrChance, int corrAttempts)
        {
            master = _master;
            slaves = new Dictionary<short, SlaveDevice>();

            corruptionChance = corrChance;
            corruptionAttempts = corrAttempts;

            master.bus = this;
            OnMessage += master.OnMessageReceived;

            random = new Random();

            Logger.Log($"Bus created with master \"{master.GetType().Name}\" (ID {master.id})");
        }

        public void HandleMessage(ModbusMessage message)
        {
            for (int i = 0; i < corruptionAttempts; i++)
            {
                if (random.NextDouble() < corruptionChance / corruptionAttempts)
                    message.address = Utils.FlipRandomBit(message.address);

                if (random.NextDouble() < corruptionChance / corruptionAttempts)
                    message.function = Utils.FlipRandomBit(message.function);

                for (int j = 0; j < message.data.Length; j++)
                    if (random.NextDouble() < corruptionChance / corruptionAttempts)
                        message.data[j] = Utils.FlipRandomBit(message.data[j]);

                if (random.NextDouble() < corruptionChance / corruptionAttempts)
                    message.checksum = Utils.FlipRandomBit(message.checksum);
            }

            OnMessage?.Invoke(message);
        }

        public void SendMessage(ModbusMessage message, bool isFromMaster = false, bool recalcChecksum = true)
        {
            message.isFromMaster = isFromMaster;
            if (recalcChecksum) message.RecalculateChecksum();

            HandleMessage(message);
        }

        public void Connect(SlaveDevice newDevice)
        {
            if (!slaves.ContainsKey(newDevice.id) && master.id != newDevice.id)
            {
                Logger.Log($"Device \"{newDevice.GetType().Name}\" (ID {newDevice.id}) connected");
                slaves[newDevice.id] = newDevice;
                newDevice.bus = this;
                OnMessage += newDevice.OnMessageReceived;

                master.AddSlave(newDevice.id);
            }
            else
                Logger.Error($"Device with ID {newDevice.id} is already connected");
        }

        public void Disconnect(byte deviceId)
        {
            if (deviceId == master.id)
            {
                Logger.Error($"Cannot disconnect master device");
                return;
            }

            if (slaves.ContainsKey(deviceId))
            {
                Logger.Log($"Device \"{slaves[deviceId].GetType().Name}\" (ID {deviceId}) disconnected");
                OnMessage -= slaves[deviceId].OnMessageReceived;
                slaves[deviceId].bus = null;
                master.RemoveSlave(deviceId);

                slaves.Remove(deviceId);
            }
            else
                Logger.Error($"Device with ID {deviceId} is not connected");
        }
    }
}

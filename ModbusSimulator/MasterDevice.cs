using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusSimulator
{
    sealed class MasterDevice : Device
    {
        private ConcurrentDictionary<byte, bool> waitingForResponse;

        public int timeout = 5000;

        public MasterDevice(byte _id, int _timeout = 5000) : base(_id)
        {
            id = _id;
            timeout = _timeout;

            waitingForResponse = new ConcurrentDictionary<byte, bool>();
        }

        public override void OnMessageReceived(ModbusMessage message)
        {
            if (!message.isFromMaster)
            {
                if (message.VerifyChecksum(out _))
                {
                    Logger.Log($"Received response from slave {message.address}: {message.PrettyPrint()}");

                    waitingForResponse.TryUpdate(message.address, false, true);

                    if (message.function >= 0x80)
                    {
                        Logger.Warn($"Slave {message.address} threw an exception: {(ModbusExceptionType)message.data[0]}");
                    }
                }
                else
                {
                    Logger.Warn("Message has invalid checksum, ignoring");
                }
            }
        }

        public void AddSlave(byte deviceId)
        {
            waitingForResponse.TryAdd(deviceId, false);
        }

        public void RemoveSlave(byte deviceId)
        {
            waitingForResponse.TryRemove(deviceId, out _);
        }

        public void SendMessage(ModbusMessage message)
        {
            if (!message.VerifyChecksum(out ushort actualChecksum))
                Logger.Warn($"Sending message to device {message.address} with wrong checksum (is {message.checksum.ToString("x2")}, should be {actualChecksum.ToString("x2")})");
            
            bus.SendMessage(message, true);
            waitingForResponse.TryUpdate(message.address, true, false);

            Task.Factory.StartNew(() => CheckForResponse(message));
        }

        private void CheckForResponse(ModbusMessage message)
        {
            Thread.Sleep(timeout);
            waitingForResponse.TryGetValue(message.address, out bool waiting);

            if (waiting)
            {
                Logger.Warn($"Device {message.address} has not responded yet, resending message");
                SendMessage(message);
            }
        }
    }
}

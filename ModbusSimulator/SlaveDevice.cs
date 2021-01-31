using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusSimulator
{
    class SlaveDevice : Device
    {
        private BitArray coils, discreteInputs;
        private ushort[] inputRegisters, holdingRegisters;

        public int processingTime, processingTimeJitter;

        private bool isBusy;

        private Random random;

        public SlaveDevice(byte _id, int procTime = 0, int procTimeJitter = 0) : base(_id)
        {
            id = _id;
            coils = new BitArray(65536);
            discreteInputs = new BitArray(65536);
            inputRegisters = new ushort[65536];
            holdingRegisters = new ushort[65536];

            processingTime = procTime;
            processingTimeJitter = procTimeJitter;
            isBusy = false;

            random = new Random();
        }

        public override void OnMessageReceived(ModbusMessage message)
        {
            if (message.address == id && message.isFromMaster)
            {
                if (message.VerifyChecksum(out _))
                {
                    Logger.Log($"(Slave {id}) Received message from master: {message.PrettyPrint()}");

                    Task.Factory.StartNew(() => HandleMessage(message));
                }
                else
                {
                    Logger.Warn($"(Slave {id}) Message has invalid checksum, ignoring");
                }
            }
        }

        private void HandleMessage(ModbusMessage message)
        {
            if (isBusy)
            {
                bus.SendMessage(ModbusMessage.CreateException(message, ModbusExceptionType.SlaveDeviceBusy));
                return;
            }

            isBusy = true;

            Thread.Sleep(Math.Max(processingTime + random.Next(-processingTimeJitter, processingTimeJitter), 0));

            try
            {
                switch (message.function)
                {
                    case 1:     // Read coils
                        Func_ReadCoils(message);
                        break;

                    case 2:     // Read discrete inputs
                        Func_ReadDiscreteInputs(message);
                        break;

                    case 5:     // Write single coil
                        Func_WriteSingleCoil(message);
                        break;

                    case 15:    // Write multiple codes
                        Func_WriteMultipleCoils(message);
                        break;

                    case 3:     // Read holding registers
                        Func_ReadHoldingRegisters(message);
                        break;

                    case 4:     // Read input registers
                        Func_ReadInputRegisters(message);
                        break;

                    case 6:     // Write single holding register
                        Func_WriteHoldingRegister(message);
                        break;

                    case 16:    // Write multiple holding registers
                        Func_WriteMultipleHoldingRegisters(message);
                        break;

                    default:
                        RespondToMessage(message);
                        break;
                }
            }
            catch (NotImplementedException)
            {
                bus.SendMessage(ModbusMessage.CreateException(message, ModbusExceptionType.IllegalFunction));
            }
            catch (IndexOutOfRangeException)
            {
                bus.SendMessage(ModbusMessage.CreateException(message, ModbusExceptionType.IllegalDataAddress));
            }
            catch (ModbusException e)
            {
                bus.SendMessage(ModbusMessage.CreateException(message, e.Type));
            }
            catch (Exception)
            {
                bus.SendMessage(ModbusMessage.CreateException(message, ModbusExceptionType.SlaveDeviceFailure));
            }

            isBusy = false;
            
        }

        private void Func_WriteSingleCoil(ModbusMessage message)
       {
            int coilAddress = message.data[0] * 256 + message.data[1];

            coils.Set(coilAddress, message.data[2] * 256 + message.data[3] == 0xFF00);

            bus.SendMessage(message);
        }

        private void Func_ReadDiscreteInputs(ModbusMessage message)
        {
            int firstInputAddress = message.data[0] * 256 + message.data[1];
            int inputsToRead = message.data[2] * 256 + message.data[3];

            byte[] readData = new byte[inputsToRead / 8 + 1];
            int inputsLeft = inputsToRead;
            for (int i = 0; i < readData.Length; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    readData[i] |= (byte)((discreteInputs.Get(i * 8 + j + firstInputAddress) ? 1 : 0) << j);
                    if (--inputsLeft < 0) break;
                }
            }

            bus.SendMessage(new ModbusMessage
            {
                address = id,
                function = 2,
                data = new byte[1] { (byte)readData.Length }.Concat(readData).ToArray()
            });
        }

        private void Func_ReadCoils(ModbusMessage message)
        {
            int firstCoilAddress = message.data[0] * 256 + message.data[1];
            int coilsToRead = message.data[2] * 256 + message.data[3];

            byte[] readData = new byte[coilsToRead / 8 + 1];
            int coilsLeft = coilsToRead;
            for (int i = 0; i < readData.Length; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    readData[i] |= (byte)((coils.Get(i * 8 + j + firstCoilAddress) ? 1 : 0) << j);
                    if (--coilsLeft < 0) break;
                }
            }

            bus.SendMessage(new ModbusMessage
            {
                address = id,
                function = 1,
                data = new byte[1] { (byte)readData.Length }.Concat(readData).ToArray()
            });
        }

        private void Func_WriteMultipleCoils(ModbusMessage message)
        {
            int firstCoilAddress = message.data[0] * 256 + message.data[1];
            int coilsToWrite = message.data[2] * 256 + message.data[3];
            byte coilByteCount = message.data[4];
            byte[] coilBytes = message.data.Skip(5).Take(message.data.Length - 5).ToArray();

            int coilsLeft = coilsToWrite;

            for (int i = 0; i < coilByteCount; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    coils.Set(i * 8 - 1 - j + firstCoilAddress, (coilBytes[i] & (1 << j)) == 1);
                    if (--coilsLeft < 0) break;
                }
            }

            bus.SendMessage(new ModbusMessage
            {
                address = id,
                function = 15,
                data = new byte[4] { (byte)((firstCoilAddress & 0xFF00) >> 8), (byte)(firstCoilAddress & 0x00FF), (byte)((coilsToWrite & 0xFF00) >> 8), (byte)(coilsToWrite & 0x00FF) }
            });
        }

        private void Func_ReadHoldingRegisters(ModbusMessage message)
        {
            int firstRegisterAddress = message.data[0] * 256 + message.data[1];
            int registersToRead = message.data[2] * 256 + message.data[3];

            ushort[] registers = holdingRegisters.Skip(firstRegisterAddress).Take(registersToRead).ToArray();
            byte[] registerBytes = new byte[registersToRead * 2];

            for (int i = 0; i < registersToRead; i++)
            {
                registerBytes[i * 2] = (byte)((registers[i] & 0xFF00) >> 8);
                registerBytes[i * 2 + 1] = (byte)(registers[i] & 0x00FF);
            }

            bus.SendMessage(new ModbusMessage
            {
                address = id,
                function = 3,
                data = new byte[1] { (byte)registerBytes.Length }.Concat(registerBytes).ToArray()
            });
        }

        private void Func_ReadInputRegisters(ModbusMessage message)
        {
            int firstRegisterAddress = message.data[0] * 256 + message.data[1];
            int registersToRead = message.data[2] * 256 + message.data[3];

            ushort[] registers = inputRegisters.Skip(firstRegisterAddress).Take(registersToRead).ToArray();
            byte[] registerBytes = new byte[registersToRead * 2];

            for (int i = 0; i < registersToRead; i++)
            {
                registerBytes[i * 2] = (byte)((registers[i] & 0xFF00) >> 8);
                registerBytes[i * 2 + 1] = (byte)(registers[i] & 0x00FF);
            }

            bus.SendMessage(new ModbusMessage
            {
                address = id,
                function = 4,
                data = new byte[1] { (byte)registerBytes.Length }.Concat(registerBytes).ToArray()
            });
        }

        private void Func_WriteHoldingRegister(ModbusMessage message)
        {
            int registerAddress = message.data[0] * 256 + message.data[1];
            int value = message.data[2] * 256 + message.data[3];

            holdingRegisters[registerAddress] = (ushort)value;

            bus.SendMessage(message);
        }

        private void Func_WriteMultipleHoldingRegisters(ModbusMessage message)
        {
            int firstRegisterAddress = message.data[0] * 256 + message.data[1];
            int registersToWrite = message.data[2] * 256 + message.data[3];
            byte registerByteCount = message.data[4];
            byte[] registerBytes = message.data.Skip(5).Take(registerByteCount).ToArray();

            for (int i = 0; i < registersToWrite; i++)
            {
                holdingRegisters[i + firstRegisterAddress] = (ushort)(registerBytes[i] * 256 + registerBytes[i + 1]);
            }

            bus.SendMessage(new ModbusMessage
            {
                address = id,
                function = 16,
                data = new byte[4] { (byte)((firstRegisterAddress & 0xFF00) >> 8), (byte)(firstRegisterAddress & 0x00FF), (byte)((registerByteCount & 0xFF00) >> 8), (byte)(registerByteCount & 0x00FF) }
            });
        }

        public virtual void RespondToMessage(ModbusMessage message)
        {
            throw new NotImplementedException();
        }
    }
}

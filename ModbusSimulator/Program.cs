using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusSimulator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                Logger.Error("Command file not specified. Please provide a path to the command file.");
            }

            ModbusFileParser.LoadFile(args[0]);

            ModbusProperties properties = ModbusFileParser.GetProperties();

            Bus bus = new Bus(new MasterDevice(0, properties.masterTimeout), properties.corruptionChance, properties.corruptionAttempts);

            for (int i = 0; i < properties.slaveCount; i++)
            {
                bus.Connect(new SlaveDevice((byte)(i + 1), properties.slaveProcessingTime, properties.slaveProcessingTimeJitter));
            }

            foreach (string[] tokens in ModbusFileParser.GetInstructions())
            {
                if (tokens.Length >= 2)
                {
                    switch (tokens[0])
                    {
                        case "Delay":
                            Thread.Sleep(int.Parse(tokens[1]));
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    bus.master.SendMessage(ModbusMessage.FromASCII(tokens[0], properties.useChecksum));
                }
            }

            Logger.Log("Executed all instructions");

            Console.ReadKey();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusSimulator
{
    class ModbusFileParser
    {
        private static string[] lines;
        private static int lineIndex = 0;

        private static readonly string[] validInstructions = new string[] { "Delay" };

        public static void LoadFile(string path)
        {
            lines = File.ReadAllLines(path);
        }

        public static ModbusProperties GetProperties()
        {
            ModbusProperties properties = new ModbusProperties();

            lineIndex = 0;
            string currentLine;
            while ((currentLine = lines[lineIndex++].TrimEnd()) != "Begin")
            {
                if (currentLine == "" || currentLine.StartsWith("//")) continue;

                string[] tokens = currentLine.Split(' ');

                try
                {
                    switch (tokens[0])
                    {
                        case "MasterTimeout":
                            properties.masterTimeout = int.Parse(tokens[1]);
                            break;
                        case "Slaves":
                            properties.slaveCount = byte.Parse(tokens[1]);
                            break;
                        case "SlaveProcessingTime":
                            properties.slaveProcessingTime = int.Parse(tokens[1]);
                            break;
                        case "SlaveProcessingTimeJitter":
                            properties.slaveProcessingTimeJitter = int.Parse(tokens[1]);
                            break;
                        case "UseChecksum":
                            if (tokens[1] != "True" && tokens[1] != "False")
                                throw new Exception("UseChecksum neither True nor False");
                            properties.useChecksum = tokens[1] == "True";
                            break;
                        case "CorruptionChance":
                            properties.corruptionChance = float.Parse(tokens[1]);
                            if (properties.corruptionChance < 0 || properties.corruptionChance > 1)
                                throw new Exception("CorruptionChance must be a float between 0 and 1");
                            break;
                        case "CorruptionAttempts":
                            properties.corruptionAttempts = int.Parse(tokens[1]);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Logger.Error($"Parsing error in line {lineIndex}: {e.Message}");
                }
                
            }

            return properties;
        }

        public static IEnumerable<string[]> GetInstructions()
        {
            string currentLine;
            while (lineIndex < lines.Length)
            {
                currentLine = lines[lineIndex++].TrimEnd();

                if (currentLine == "" || currentLine.StartsWith("//")) continue;

                string[] tokens = currentLine.Split(' ');

                try
                {
                    if (tokens.Length >= 2)
                    {
                        if (!validInstructions.Contains(tokens[0]))
                            throw new Exception($"Unknown instruction {tokens[0]}");
                    }
                    else
                    {
                        if (!tokens[0].All("0123456789abcdefABCDEF".Contains))
                            throw new Exception($"Invalid message - not a hexadecimal string");
                    }
                }
                catch (Exception e)
                {
                    Logger.Error($"Parsing error in line {lineIndex}: {e.Message}");
                }

                yield return tokens;
            }
        }

    }
}

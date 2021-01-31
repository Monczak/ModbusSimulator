# ModbusSimulator
## A simple and expandable Modbus protocol simulator

### What is this?

This is an event-based Modbus protocol simulator that allows you to simulate interactions between virtual devices connected via a Modbus bus. It is able to load commands from a file and conduct a simulation where the master device talks to slave devices and does something cool.

### How to use
The simulator uses `.modbus` files which contain configuration information and a list of commands that the master device will broadcast to the bus.

To run the simulator with a `.modbus` file, type `ModbusSimulator.exe path/to/file` in a command interpreter.

### The .modbus file
The `.modbus` file uses a custom command format which allows the simulator to mimic a sort-of-real-life device configuration. 

The first few lines should contain configuration information, which allow you to tinker the simulation to fit your own needs or create weird/untypical situations. The second part of the file should contain command information, which consists of hexadecimal representations of message data, along with other special commands.

The instructions that the master device should execute must be placed after a `Begin` statement.

Comments are supported - they should start with `//`.

An example `.modbus` file is provided below and in the repository.
```
// Properties
MasterTimeout 2000
Slaves 10
SlaveProcessingTime 300
SlaveProcessingTimeJitter 50
UseChecksum False
CorruptionChance 0.01
CorruptionAttempts 3

// Instructions
Begin
0105DEADFF00
Delay 500
0101DEAA000A
Delay 1000
0101DEAA000A
0201DEAA000A
```

##### Properties
- `MasterTimeout` - how long the master device should wait for a response from the slave, in milliseconds. Defaults to 5000.
- `Slaves` - the number of slave devices initially connected to the network. Defaults to 5.
- `SlaveProcessingTime` - how long any slave device should process a received message, or at least pretend to process one, in milliseconds. Defaults to 0.
- `SlaveProcessingTimeJitter` - bounds of a random value added/subtracted from the slave processing time in order to increase randomness (with a proc time of 300 and a jitter of 50, the proc time would be a random value between 250 and 349), in milliseconds. Defaults to 0.
- `UseChecksum` - whether the hexadecimal commands in the second section contain checksums. Can be set to `True` or `False`. If set to `False`, checksums will be automatically generated using the proper CRC16/MODBUS algorithm. Defaults to False.
- `CorruptionChance` - the chance that a random bit will be flipped in a fragment of any message sent via the bus. Is a float between 0 and 1. Only small values (0.1 or less) are recommended. Defaults to 0.01.
- `CorruptionAttempts` - how many times a message corruption should be attempted. Defaults to 10.

If you don't provide a certain property in the configuration section, it will be set to the default value. The specified properties are overrides for the default values, listed above.

##### Special commands
- `Delay (x)` - makes the master device wait for x milliseconds before executing the next command.

### Extensibility
This simulator was designed with straightforward extensibility in mind. It should be simple to extend a `SlaveDevice` to create a custom device with custom functions, characteristics etc.
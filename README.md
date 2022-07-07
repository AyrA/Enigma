# C# Enigma Simulator

This application can simulate the german Enigma machine with predefined and custom rotors.
It can also generate code sheets.

## CAUTION!

The Enigma cipher has been broken.
Do not use this type of encryption for real world cryptographic needs.

## Features

- Encrypt and decrypt text
- Compatible with real Enigma machines
- Use pre-existing and custom rotors
- Generate code sheets for you and your friends that look almost exactly like the ones from WW2
- Generate random rotor and plugboard settings

## Usage

This is a command line utility using Windows command line syntax.
Use `/?` to get help.

## Code Structure

All Enigma components (rotor types and the plugboard) are their own classes and trivial to inspect.

## Limitations

This application will encrypt and decrypt but doesn't follows the Enigma protocol,
you have to do that yourself.
If you want to encrypt messages the way the germans did
you need to find a way to communicate the rotor start positions.

Depending on the content you want to transmit,
you also need to encode it because Enigma only supports A-Z but not spaces or other characters.

The Enigma Uhr is not supported.

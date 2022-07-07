using System;
using System.Collections.Generic;
using System.Linq;

namespace Enigma
{
    /// <summary>
    /// Represents an Enigma machine with rotors and plugboard
    /// </summary>
    public class EnigmaMachine
    {
        /// <summary>
        /// Rotors
        /// </summary>
        public RotorAssembly Rotors { get; }
        /// <summary>
        /// Plugboard
        /// </summary>
        public Plugboard Plugboard { get; }

        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="Rotors">Rotors</param>
        /// <param name="Plugboard">Plugboard</param>
        public EnigmaMachine(RotorAssembly Rotors, Plugboard Plugboard = null)
        {
            //Rotors cannot be null
            if (Rotors is null)
            {
                throw new ArgumentNullException(nameof(Rotors));
            }
            //A null plugboard is simply one without plugs
            if (Plugboard is null)
            {
                this.Plugboard = new Plugboard();
            }
            else
            {
                this.Plugboard = Plugboard;
            }

            this.Rotors = Rotors;
        }

        /// <summary>
        /// Encrypts/Decrypts a string and ratches the rotors forward accordingly
        /// </summary>
        /// <param name="Message">Message</param>
        /// <returns>Transformed message</returns>
        /// <remarks>Encryption and decryption are the same in enigma</remarks>
        public string Encrypt(string Message)
        {
            return Plugboard.Map(Rotors.Encrypt(Plugboard.Map(Message)));
        }

        /// <summary>
        /// Encrypts a single char and ratches the rotors forward accordingly
        /// </summary>
        /// <param name="C">Character</param>
        /// <returns>Transformed character</returns>
        /// <remarks>Encryption and decryption are the same in enigma</remarks>
        public char Encrypt(char C)
        {
            return Plugboard.Map(Rotors.Encrypt(Plugboard.Map(C)));
        }

        /// <summary>
        /// Gets the current state of the machine.
        /// This can be used in <see cref="Import(string)"/> to create a copy
        /// </summary>
        /// <returns>Machine state</returns>
        public string GetState()
        {
            return string.Join(";", Rotors.GetState()) + ";" + Plugboard.GetState(false);
        }

        /// <summary>
        /// Creates a machine from a string created earlier using <see cref="GetState"/>
        /// </summary>
        /// <param name="state">Machine state</param>
        /// <returns>Enigma machine</returns>
        public static EnigmaMachine Import(string state)
        {
            if (string.IsNullOrEmpty(state))
            {
                throw new ArgumentException($"'{nameof(state)}' cannot be null or empty.", nameof(state));
            }

            UKW ukw = null;
            ETW etw = null;
            List<Walze> rotors = new List<Walze>();
            Plugboard pb = null;

            var Params = state.Split(';');
            foreach (var P in Params)
            {
                var Segments = P.Split(':', 2);
                if (Segments.Length != 2)
                {
                    throw new ArgumentException($"'{P}' is an invalid setting");
                }
                switch (Segments[0].ToUpper())
                {
                    case "U":
                        if (ukw != null)
                        {
                            throw new ArgumentException("Setting contains multiple UKW");
                        }
                        //Try parsing existing rotor name, if not possible, prepare alphabet
                        if (!Tools.GetRotorName(Segments[1], out RotorName parsedUKW))
                        {
                            ukw = EnigmaRotors.GetCustomUKW(Tools.FixAlphabet(Segments[1]));
                        }
                        else
                        {
                            ukw = (UKW)EnigmaRotors.GetRotor(parsedUKW);
                        }
                        break;
                    case "E":
                        if (etw != null)
                        {
                            throw new ArgumentException("Setting contains multiple ETW");
                        }
                        //Try parsing existing rotor name, if not possible, prepare alphabet
                        if (!Tools.GetRotorName(Segments[1], out RotorName parsedETW))
                        {
                            etw = EnigmaRotors.GetCustomETW(Tools.FixAlphabet(Segments[1]));
                        }
                        else
                        {
                            etw = (ETW)EnigmaRotors.GetRotor(parsedETW);
                        }
                        break;
                    case "W":
                        //order: name/charset:ring:position:notches
                        var settings = Segments[1].Split(':');
                        int ring = 0;
                        int pos = 0;
                        int[] notches = null;
                        bool custom = false;
                        if (settings.Length > 4)
                        {
                            throw new ArgumentException($"Invalid rotor setting: '{P}'");
                        }
                        //Try parsing existing rotor name, if not possible, prepare alphabet
                        if (!Tools.GetRotorName(settings[0], out RotorName parsed))
                        {
                            custom = true;
                            settings[0] = Tools.FixAlphabet(settings[0]);
                            if (settings[0].Length != EnigmaRotors.CHARSET_SIZE)
                            {
                                throw new ArgumentException($"'{settings[0]}' is neither a valid rotor name nor alphabet");
                            }
                        }
                        else if (settings.Length == 4)
                        {
                            //Only custom rotors have custom notches
                            throw new ArgumentException("A custom notch setting is only valid for custom rotors");
                        }
                        if (settings.Length > 1)
                        {
                            //Parse ring
                            ring = Tools.AlphaToNum(settings[1]);
                            if (settings.Length > 2)
                            {
                                //Parse position
                                pos = Tools.AlphaToNum(settings[2]);
                                if (settings.Length > 3)
                                {
                                    //Parse notches
                                    notches = settings[3].Select(Tools.AlphaToNum).ToArray();
                                }
                            }
                        }

                        if (custom)
                        {
                            var r = EnigmaRotors.GetCustomRotor(settings[0], pos, ring);
                            if (notches != null)
                            {
                                r.Notches = notches;
                            }
                            rotors.Add(r);
                        }
                        else
                        {
                            var r = (Walze)EnigmaRotors.GetRotor(parsed, pos, ring);
                            rotors.Add(r);
                        }
                        break;
                    case "S":
                        pb = new Plugboard(Segments[1].Chunk(2).ToArray());
                        break;
                    default:
                        throw new ArgumentException($"'{P}' is an invalid setting");
                }
            }
            if (ukw == null)
            {
                throw new ArgumentException("Serialized data misses UKW");
            }
            if (etw == null)
            {
                throw new ArgumentException("Serialized data misses ETW");
            }
            if (rotors.Count == 0)
            {
                throw new ArgumentException("Serialized data misses rotors");
            }
            return new EnigmaMachine(new RotorAssembly(ukw, rotors, etw), pb);
        }
    }
}

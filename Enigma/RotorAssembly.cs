using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Enigma
{
    /// <summary>
    /// Represents a rotor stack in an Enigma machine
    /// </summary>
    public class RotorAssembly
    {
        /// <summary>
        /// Rotors.
        /// First is always <see cref="UKW"/>
        /// and last is always <see cref="ETW"/>.
        /// Others are <see cref="Walze"/>
        /// </summary>
        public BaseRotor[] Rotors { get; }

        /// <summary>
        /// Initializes a rotor assembly from the given rotors
        /// </summary>
        /// <param name="Rotors">Rotors</param>
        /// <remarks>
        /// Rotors must be specified as seen by the user from left to right.
        /// The first rotor must thus be an <see cref="UKW"/> and the last one an <see cref="ETW"/>
        /// </remarks>
        public RotorAssembly(params BaseRotor[] Rotors)
        {
            if (Rotors is null)
            {
                throw new ArgumentNullException(nameof(Rotors));
            }
            if (Rotors.Length < 3)
            {
                throw new ArgumentException("Must supply at least 3 rotors", nameof(Rotors));
            }
            if (!(Rotors[0] is UKW))
            {
                throw new ArgumentException("Leftmost rotor must be of UKW type", nameof(Rotors));
            }

            if (!(Rotors[^1] is ETW))
            {
                throw new ArgumentException("Rightmost rotor must be of ETW type", nameof(Rotors));
            }

            this.Rotors = Rotors;
        }

        /// <summary>
        /// Initializes an instance from different rotor types
        /// </summary>
        /// <param name="ukw">UKW</param>
        /// <param name="rotors">Rotors (in user viewed order from left to right)</param>
        /// <param name="etw">ETW</param>
        public RotorAssembly(UKW ukw, IEnumerable<Walze> rotors, ETW etw)
        {
            if (ukw is null)
            {
                throw new ArgumentNullException(nameof(ukw));
            }

            if (rotors is null)
            {
                throw new ArgumentNullException(nameof(rotors));
            }

            if (etw is null)
            {
                throw new ArgumentNullException(nameof(etw));
            }
            var combined = new List<BaseRotor>();
            combined.Add(ukw);
            combined.AddRange(rotors);
            combined.Add(etw);
            if (combined.Count == 2)
            {
                throw new ArgumentException("Must supply at least one rotor", nameof(rotors));
            }
            if (combined.Contains(null))
            {
                throw new ArgumentException("Enumeration must not contain 'null'", nameof(rotors));
            }

            Rotors = combined.ToArray();
        }

        /// <summary>
        /// Encrypts a character and prints the path it takes through the rotor stack
        /// </summary>
        /// <param name="C">Character</param>
        /// <returns>Transformed character</returns>
        /// <remarks>This will not call <see cref="Ratch"/></remarks>
        public char EncryptDebug(char C)
        {
            Console.Write("{0}", C);
            //Encrypt inwards
            foreach (var Rotor in Rotors.Reverse())
            {
                C = Rotor.EncryptIn(C);
                Console.Write(" --{1}--> {0}", C, Rotor.Name);
            }
            //Encrypt outwards
            for (var i = 1; i < Rotors.Length; i++)
            {
                C = Rotors[i].EncryptOut(C);
                Console.Write(" --{1}--> {0}", C, Rotors[i].Name);
            }
            Console.WriteLine();
            return C;
        }

        /// <summary>
        /// Encrypts multiple characters at once
        /// </summary>
        /// <param name="Text">Text</param>
        /// <returns>Transformed text</returns>
        /// <remarks>This will correctly call <see cref="Ratch"/></remarks>
        public string Encrypt(string Text)
        {
            if (string.IsNullOrEmpty(Text))
            {
                throw new ArgumentException($"'{nameof(Text)}' cannot be null or empty.", nameof(Text));
            }

            if (!Regex.IsMatch(Text, "^[A-Z]+$"))
            {
                throw new ArgumentException($"'{nameof(Text)}' must consist exclusively of A-Z.", nameof(Text));
            }
            return string.Concat(Text.Select(Encrypt).ToArray());
        }

        /// <summary>
        /// Encrypts a single character
        /// </summary>
        /// <param name="C">Character</param>
        /// <returns>Transformed character</returns>
        /// <remarks>This will correctly call <see cref="Ratch"/></remarks>
        public char Encrypt(char C)
        {
            Ratch();
            //Encrypt inwards
            foreach (var Rotor in Rotors.Reverse())
            {
                C = Rotor.EncryptIn(C);
            }
            //Encrypt outwards
            for (var i = 1; i < Rotors.Length; i++)
            {
                C = Rotors[i].EncryptOut(C);
            }
            return C;
        }

        /// <summary>
        /// Ratches the machine a step forwatd
        /// </summary>
        public void Ratch()
        {
            //I feel very smart for coming up with this loop that has no body
            for (var i = Rotors.Length - 1; i >= 0 && Rotors[i].Ratch(); i--) ;
        }

        /// <summary>
        /// Resets all rotors to the zero position.
        /// CAUTION! Resets position and ring to "A",
        /// and not the values they were given to this rotor stack instance.
        /// </summary>
        public void Reset()
        {
            foreach (var Rotor in Rotors.OfType<Walze>())
            {
                Rotor.Position = 0;
                Rotor.Ring = 0;
            }
        }

        /// <summary>
        /// Gets the state of all rotors
        /// </summary>
        /// <returns>Rotor states</returns>
        public string[] GetState()
        {
            return Rotors.Select(m => m.ExportState()).ToArray();
        }
    }
}

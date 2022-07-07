using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Enigma
{
    /// <summary>
    /// A base rotor.
    /// <see cref="Walze"/>, <see cref="UKW"/> and <see cref="ETW"/> derive from this
    /// </summary>
    public abstract class BaseRotor : ICloneable
    {
        /// <summary>
        /// Gets the charset of this rotor
        /// </summary>
        public abstract string Charset { get; }

        /// <summary>
        /// Gets the name of the rotor
        /// </summary>
        /// <remarks>For custom rotors, this is unset</remarks>
        public string Name { get; }

        /// <summary>
        /// Initializes a new instance with the given name
        /// </summary>
        /// <param name="Name">Rotor name. Should be null for custom rotors or import/export is not going to work</param>
        public BaseRotor(string Name = null)
        {
            this.Name = Name;
        }

        /// <summary>
        /// Encrypts a character that's on the way in (has not yet hit the <see cref="UKW"/>)
        /// </summary>
        /// <param name="c">Character</param>
        /// <returns>Transformed character</returns>
        /// <remarks>
        /// Encryption and decryption is the same.
        /// Use this for decryption too.
        /// This will not <see cref="Ratch"/> the rotor
        /// </remarks>
        public abstract char EncryptIn(char c);

        /// <summary>
        /// Encrypts a character that's on the way out (has hit the <see cref="UKW"/>)
        /// </summary>
        /// <param name="c">Character</param>
        /// <returns>Transformed character</returns>
        /// <remarks>
        /// Encryption and decryption is the same.
        /// Use this for decryption too.
        /// This will not <see cref="Ratch"/> the rotor
        /// </remarks>
        public abstract char EncryptOut(char c);

        /// <summary>
        /// Ratches the rotor one step forwards
        /// </summary>
        /// <returns>true, if the rotor to the left should ratch too</returns>
        public abstract bool Ratch();

        /// <summary>
        /// Exports the current rotor configuration to allow import at a later state
        /// </summary>
        /// <returns>Rotor configuration</returns>
        public abstract string ExportState();

        /// <summary>
        /// Rotates x around <see cref="EnigmaRotors.CHARSET_SIZE"/> using modular arithmatic
        /// </summary>
        /// <param name="x">Number</param>
        /// <returns>x % <see cref="EnigmaRotors.CHARSET_SIZE"/></returns>
        /// <remarks>
        /// This is not meant to deal with large negative numbers.
        /// To rotate characters, use <see cref="RotChar(int)"/>
        /// </remarks>
        public static int Rot(int x)
        {
            while (x < 0)
            {
                x += EnigmaRotors.CHARSET_SIZE;
            }
            return x % EnigmaRotors.CHARSET_SIZE;
        }

        /// <summary>
        /// Rotates a character that's between A and Z using modular arithmatic
        /// </summary>
        /// <param name="c">character</param>
        /// <returns>Rotated character</returns>
        /// <remarks>
        /// This is the text version of <see cref="Rot(int)"/>
        /// </remarks>
        public static char RotChar(int c)
        {
            return (char)(Rot(c - 'A') + 'A');
        }

        /// <summary>
        /// Checks if the supplied charset is valid.
        /// Throws if it's not.
        /// </summary>
        /// <param name="Charset">Charset</param>
        /// <returns><paramref name="Charset"/></returns>
        /// <remarks>
        /// Checks if all characters are unique, and between A-Z inclusive,
        /// and that there's 26 of them
        /// </remarks>
        public static string CheckCharset(string Charset)
        {
            if (string.IsNullOrEmpty(Charset))
            {
                throw new ArgumentException($"'{nameof(Charset)}' cannot be null or empty.", nameof(Charset));
            }

            if (Charset.Distinct().Count() != Charset.Length)
            {
                throw new ArgumentException("Duplicate letter in charset", nameof(Charset));
            }
            if (!Regex.IsMatch(Charset, "^[A-Z]{" + EnigmaRotors.CHARSET_SIZE + "}$"))
            {
                throw new ArgumentException("Charset must be made up of 26 letters A-Z only", nameof(Charset));
            }
            return Charset;
        }

        /// <summary>
        /// Clones this instance
        /// </summary>
        /// <returns>New, independent copy</returns>
        public abstract object Clone();
    }
}

using System;

namespace Enigma
{
    /// <summary>
    /// Represents the entry rotor.
    /// This doesn't rotates and is not user swappable.
    /// It connects to the <see cref="Plugboard"/> and the first <see cref="Walze"/>.
    /// Cryptographically, it's like a standard rotor
    /// </summary>
    public class ETW : BaseRotor
    {
        /// <summary>
        /// Charset of the rotor
        /// </summary>
        public override string Charset { get; }

        /// <summary>
        /// Creates a new entry rotor
        /// </summary>
        /// <param name="Charset">Charset</param>
        /// <param name="Name">Name</param>
        public ETW(string Charset, string Name = null) : base(Name)
        {
            this.Charset = CheckCharset(Charset);
        }

        /// <summary>
        /// Transforms a character on the way into the rotor stack
        /// </summary>
        /// <param name="c">Character</param>
        /// <returns>Transformed character</returns>
        public override char EncryptIn(char c)
        {
            if (c < 'A' || c > 'Z')
            {
                throw new ArgumentException("Character not in uppercase A-Z range");
            }
            var IndexIn = Rot(c - 'A');
            return Charset[IndexIn];
        }

        /// <summary>
        /// Transforms a character on the way out of the rotor stack
        /// </summary>
        /// <param name="c">Character</param>
        /// <returns>Transformed character</returns>
        public override char EncryptOut(char c)
        {
            if (c < 'A' || c > 'Z')
            {
                throw new ArgumentException("Character not in uppercase A-Z range");
            }
            var IndexOut = Charset.IndexOf(c);
            return (char)(IndexOut + 'A');
        }

        /// <summary>
        /// Ratches this rotor
        /// </summary>
        /// <returns>true</returns>
        /// <remarks>
        /// Because this doesn't rotates,
        /// it always returns true to essentially make the first real rotor always rotate
        /// </remarks>
        public override bool Ratch()
        {
            //ETW does not rotate
            //but always rotates the rotor next to it
            return true;
        }

        /// <summary>
        /// Exports ETW state
        /// </summary>
        /// <returns>ETW state</returns>
        public override string ExportState()
        {
            return "E:" + (string.IsNullOrEmpty(Name) ? Charset : Name);
        }

        /// <summary>
        /// Creates a copy
        /// </summary>
        /// <returns>ETW</returns>
        public override object Clone()
        {
            return new ETW(Charset, Name);
        }
    }
}

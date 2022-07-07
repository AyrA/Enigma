using System;
using System.Linq;

namespace Enigma
{
    /// <summary>
    /// Reflector at the end of the rotor stack
    /// </summary>
    public class UKW : BaseRotor
    {
        /// <summary>
        /// Reflector charset
        /// </summary>
        public override string Charset { get; }

        /// <summary>
        /// Internal plugboard representation
        /// </summary>
        /// <remarks>
        /// The reflector is basically a plugboard with all plugs present,
        /// so we just use a plugboard behind the scene.
        /// </remarks>
        private readonly Plugboard PB;

        /// <summary>
        /// Creates a new UKW
        /// </summary>
        /// <param name="Charset">UKW charset</param>
        /// <param name="Name">UKW name</param>
        /// <remarks>The charset must have the same format like the charset for a plugboard</remarks>
        public UKW(string Charset, string Name = null) : base(Name)
        {
            if (Charset is null)
            {
                throw new ArgumentNullException(nameof(Charset));
            }
            if (Charset.Length == 24)
            {
                var lastPair = new string(EnigmaRotors.CHARSET.Where(m => !Charset.Contains(m)).ToArray());
                if (lastPair.Length == 2)
                {
                    Charset += lastPair;
                }
            }
            this.Charset = CheckCharset(Charset);
            //An UKW is basically a plugboard with all plugs in use
            PB = new Plugboard(Charset.Chunk(2).ToArray());
        }

        /// <summary>
        /// Encrypt a character going into the rotor from the right side
        /// </summary>
        /// <param name="c">Character</param>
        /// <returns>Transformed character</returns>
        public override char EncryptIn(char c)
        {
            if (c < 'A' || c > 'Z')
            {
                throw new ArgumentException("Character not in uppercase A-Z range");
            }
            return PB.Map(c);
        }

        /// <summary>
        /// Throws an exception
        /// </summary>
        /// <param name="c">ignored</param>
        /// <returns>N/A</returns>
        /// <remarks>
        /// It's not possible for a signal to arrive from the left side</remarks>
        public override char EncryptOut(char c)
        {
            throw new InvalidOperationException("Use EncryptIn() in UKW only. UKW not the last rotor?");
        }

        /// <summary>
        /// Does nothing
        /// </summary>
        /// <returns>false</returns>
        public override bool Ratch()
        {
            //UKW does not rotate and as the final rotor,
            //will never ratch another.
            //Note that this is not 100% correct.
            //Aparently the thin UKW can rotate but I did not implement this here
            return false;
        }

        /// <summary>
        /// Exports UKW state
        /// </summary>
        /// <returns>UKW state</returns>
        public override string ExportState()
        {
            return "U:" + (string.IsNullOrEmpty(Name) ? string.Join("-", Charset.Chunk(2).SkipLast(1)) : Name);
        }

        /// <summary>
        /// Creates a copy of this instance
        /// </summary>
        /// <returns>UKW</returns>
        public override object Clone()
        {
            return new UKW(Charset, Name);
        }
    }
}

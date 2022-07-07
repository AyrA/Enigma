using System;
using System.Linq;

namespace Enigma
{
    /// <summary>
    /// Represents a standard rotor
    /// </summary>
    public class Walze : BaseRotor
    {
        /// <summary>
        /// Notches to tick the next rotor over
        /// </summary>
        private int[] notches;
        /// <summary>
        /// Ring position
        /// </summary>
        private int ring;
        /// <summary>
        /// Rotor position
        /// </summary>
        private int position;

        /// <summary>
        /// Gets or sets the rotor position
        /// </summary>
        /// <remarks>Use <see cref="Ratch"/> for regular rotor movement instead</remarks>
        public int Position
        {
            get => position;
            set
            {
                if (value < 0 || value >= EnigmaRotors.CHARSET_SIZE)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                position = value;
            }
        }

        /// <summary>
        /// Gets or sets the ring position
        /// </summary>
        public int Ring
        {
            get => ring;
            set
            {
                if (value < 0 || value >= EnigmaRotors.CHARSET_SIZE)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                ring = value;
            }
        }

        /// <summary>
        /// Gets the rotor charser
        /// </summary>
        public override string Charset { get; }

        /// <summary>
        /// Gets or sets the turnover notches for the next rotor
        /// </summary>
        public int[] Notches
        {
            get => (int[])notches.Clone();
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                notches = (int[])value.Clone();
            }
        }

        /// <summary>
        /// Gets if the next call to <see cref="Ratch"/> will turn the rotor to the left
        /// </summary>
        public bool WillRatchNext { get => notches.Contains(position); }

        /// <summary>
        /// Creates a new rotor
        /// </summary>
        /// <param name="Charset">Rotor charset</param>
        /// <param name="Notches">Turnover notches</param>
        /// <param name="StartPosition">Rotor start position</param>
        /// <param name="RingPosition">Ring position</param>
        /// <param name="Name">Rotor name</param>
        public Walze(string Charset, int[] Notches, int StartPosition, int RingPosition, string Name = null) : base(Name)
        {
            Position = StartPosition;
            Ring = RingPosition;
            notches = Notches == null ? new int[0] : (int[])Notches.Clone();
            this.Charset = CheckCharset(Charset);
        }

        /// <summary>
        /// Encrypts a character going into the rotor from the right side
        /// </summary>
        /// <param name="c">Character</param>
        /// <returns>Transformed character</returns>
        public override char EncryptIn(char c)
        {
            if (c < 'A' || c > 'Z')
            {
                throw new ArgumentException("Character not in uppercase A-Z range");
            }
            var OffsetIn = RotChar(c + position - ring) - 'A';
            var CharOut = Charset[OffsetIn];
            CharOut = RotChar(CharOut - position + ring);
            return CharOut;
        }

        /// <summary>
        /// Encrypts a character going into the rotor from the left side
        /// </summary>
        /// <param name="c">Character</param>
        /// <returns>Transformed character</returns>
        public override char EncryptOut(char c)
        {
            if (c < 'A' || c > 'Z')
            {
                throw new ArgumentException("Character not in uppercase A-Z range");
            }
            var CharIn = RotChar(c + position - ring);
            var CharOut = Charset.IndexOf(CharIn);
            return RotChar('A' + CharOut - position + ring);
        }

        /// <summary>
        /// Ratches the rotor to the next position
        /// </summary>
        /// <returns>true, if rotor to the left should ratch too</returns>
        public override bool Ratch()
        {
            var RatchNext = notches.Contains(position);
            position = Rot(++position);
            return RatchNext;
        }

        /// <summary>
        /// Exports the current rotor state
        /// </summary>
        /// <returns>Rotor state</returns>
        public override string ExportState()
        {
            var Ret = string.IsNullOrEmpty(Name) ? Charset : Name;
            var checkNotches = Ret == Charset;
            if (Ring > 0 || Position > 0 || (checkNotches && Notches.Length > 0))
            {
                Ret += ":" + (Ring + 1);
            }
            if (Position > 0 || (checkNotches && Notches.Length > 0))
            {
                Ret += ":" + (char)('A' + Position);
            }
            if (checkNotches && Notches.Length > 0)
            {
                Ret += ":" + new string(Notches.Select(m => (char)('A' + m)).ToArray());
            }
            return "W:" + Ret;
        }

        /// <summary>
        /// Clones this rotor
        /// </summary>
        /// <returns>Copy</returns>
        public override object Clone()
        {
            return new Walze(Charset, Notches, Position, Ring, Name);
        }
    }
}

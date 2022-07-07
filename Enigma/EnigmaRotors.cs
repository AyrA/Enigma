using System;
using System.Collections.Generic;
using System.Linq;

namespace Enigma
{
    /// <summary>
    /// Provides easy access to standard andcustom rotors
    /// </summary>
    public static class EnigmaRotors
    {
        /// <summary>
        /// The charset of enigma
        /// </summary>
        /// <remarks>
        /// If you change this you also need to change <see cref="CHARSET_SIZE"/>.
        /// You also need to rewrite code everywhere that check if a char is between A-Z
        /// </remarks>
        public const string CHARSET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        /// <summary>
        /// Length of <see cref="CHARSET"/>
        /// </summary>
        public const int CHARSET_SIZE = 26;

        /// <summary>
        /// All known rotors to exist
        /// </summary>
        /// <seealso cref="https://en.wikipedia.org/wiki/Enigma_rotor_details#Rotor_wiring_tables" />
        private static readonly Dictionary<RotorName, string> _charsets = new Dictionary<RotorName, string>
        {
            //Commercially sold enigma
            { RotorName.Comm_R1, "DMTWSILRUYQNKFEJCAZBPGXOHV" },
            { RotorName.Comm_R2, "HQZGPJTMOBLNCIFDYAWVEUSRKX" },
            { RotorName.Comm_R3, "UQNTLSZFMREHDPXKIBVYGJCWOA" },
            //German railway
            { RotorName.Rocket_I,   "JGDQOXUSCAMIFRVTPNEWKBLZYH" },
            { RotorName.Rocket_II,  "NTZPSFBOKMWRCJDIVLAEYUXHGQ" },
            { RotorName.Rocket_III, "JVIUBHTCDYAKEQZPOSGXNRMWFL" },
            { RotorName.Rocket_UKW, "QYHOGNECVPUZTFDJAXWMKISRBL" },
            { RotorName.Rocket_ETW, "QWERTZUIOASDFGHJKPYXCVBNML" },
            //Swiss "Enigma-K"
            { RotorName.Swiss_I,   "PEZUOHXSCVFMTBGLRINQJWAYDK" },
            { RotorName.Swiss_II,  "ZOUESYDKFWPCIQXHMVBLGNJRAT" },
            { RotorName.Swiss_III, "EHRVXGAOBQUSIMZFLYNWKTPDJC" },
            { RotorName.Swiss_UKW, "IMETCGFRAYSQBZXWLHKDVUPOJN" },
            { RotorName.Swiss_ETW, "QWERTZUIOASDFGHJKPYXCVBNML" },
            //WW2 machines
            { RotorName.WW2_I,          "EKMFLGDQVZNTOWYHXUSPAIBRCJ" },
            { RotorName.WW2_II,         "AJDKSIRUXBLHWTMCQGZNPYFVOE" },
            { RotorName.WW2_III,        "BDFHJLCPRTXVZNYEIWGAKMUSQO" },
            { RotorName.WW2_IV,         "ESOVPZJAYQUIRHXLNFTGKDCMWB" },
            { RotorName.WW2_V,          "VZBRGITYUPSDNHLXAWMJQOFECK" },
            { RotorName.WW2_VI,         "JPGVOUMFYQBENHZRDKASXLICTW" },
            { RotorName.WW2_VII,        "NZJHGRCXMYSWBOUFAIVLPEKQDT" },
            { RotorName.WW2_VIII,       "FKQHTLXOCBJSPDZRAMEWNIUYGV" },
            { RotorName.WW2_UKW_A,      "EJMZALYXVBWFCRQUONTSPIKHGD" },
            { RotorName.WW2_UKW_B,      "YRUHQSLDPXNGOKMIEBFZCWVJAT" },
            { RotorName.WW2_UKW_C,      "FVPJIAOYEDRZXWGCTKUQSBNMHL" },
            { RotorName.WW2_UKW_B_Thin, "ENKQAUYWJICOPBLMDXZVFTHRGS" },
            { RotorName.WW2_UKW_C_Thin, "RDOBJNTKVEHMLFCWZAXGYIPSUQ" },
            { RotorName.WW2_Beta,       "LEYJVCNIXWPBQMDRTAKZGFUHOS" },
            { RotorName.WW2_Gamma,      "FSOKANUERHMBTIYCWLQPZXVGJD" },
            { RotorName.WW2_ETW,        CHARSET },
        };

        /// <summary>
        /// All known notch positions
        /// </summary>
        /// <seealso cref="https://en.wikipedia.org/wiki/Enigma_rotor_details#Turnover_notch_positions" />
        private static readonly Dictionary<RotorName, int[]> _notches = new Dictionary<RotorName, int[]>
        {
            //Notches for standard rotors. I could only find those for WW2 rotors (see link)
            //The rotor to the left rotates when the current rotor goes **past** the letter with the notch.
            { RotorName.WW2_I,    new int[]{ 'Q' - 'A' } },
            { RotorName.WW2_II,   new int[]{ 'E' - 'A' } },
            { RotorName.WW2_III,  new int[]{ 'V' - 'A' } },
            { RotorName.WW2_IV,   new int[]{ 'J' - 'A' } },
            { RotorName.WW2_V,    new int[]{ 'Z' - 'A' } },
            { RotorName.WW2_VI,   new int[]{ 'Z' - 'A', 'M' - 'A' } },
            { RotorName.WW2_VII,  new int[]{ 'Z' - 'A', 'M' - 'A' } },
            { RotorName.WW2_VIII, new int[]{ 'Z' - 'A', 'M' - 'A' } },
        };

        /// <summary>
        /// Reflectors
        /// </summary>
        private static readonly RotorName[] _ukw = new RotorName[]
        {
            RotorName.Rocket_UKW, RotorName.Swiss_UKW,
            RotorName.WW2_UKW_A, RotorName.WW2_UKW_B, RotorName.WW2_UKW_C,
            RotorName.WW2_UKW_B_Thin, RotorName.WW2_UKW_C_Thin
        };

        /// <summary>
        /// Entry rotors
        /// </summary>
        private static readonly RotorName[] _etw = new RotorName[]
        {
            RotorName.WW2_ETW, RotorName.Swiss_ETW, RotorName.Rocket_ETW
        };

        /// <summary>
        /// Gets all known rotor charsets
        /// </summary>
        /// <remarks>
        /// Use <see cref="GetRotor(RotorName)"/> to get an initialized rotor
        /// for <see cref="RotorAssembly"/>
        /// </remarks>
        public static Dictionary<RotorName, string> Charsets
        {
            get => _charsets.ToDictionary(m => m.Key, m => m.Value);
        }

        /// <summary>
        /// Gets all known reflectors
        /// </summary>
        public static RotorName[] UKW { get => (RotorName[])_ukw.Clone(); }

        /// <summary>
        /// Gets all known entry rotors
        /// </summary>
        public static RotorName[] ETW { get => (RotorName[])_etw.Clone(); }

        /// <summary>
        /// Gets all standard rotors
        /// </summary>
        public static RotorName[] Standard
        {
            get
            {
                return Enum
                    .GetValues(typeof(RotorName))
                    .OfType<RotorName>()
                    .Where(m => !_etw.Contains(m) && !_ukw.Contains(m))
                    .ToArray();
            }
        }

        /// <summary>
        /// Checks <see cref="CHARSET"/> against <see cref="CHARSET_SIZE"/>
        /// </summary>
        static EnigmaRotors()
        {
            if (CHARSET.Length != CHARSET_SIZE)
            {
                throw new Exception("consts CHARSET and CHARSET_SIZE do not match up");
            }
        }

        /// <summary>
        /// Gets an initialized rotor
        /// </summary>
        /// <param name="Name">Rotor name</param>
        /// <returns>Rotor</returns>
        /// <remarks>
        /// Will correctly return <see cref="ETW"/> or <see cref="UKW"/> type for those names.
        /// </remarks>
        public static BaseRotor GetRotor(RotorName Name, int StartPosition = 0, int RingPosition = 0)
        {
            if (!Enum.IsDefined(typeof(RotorName), Name))
            {
                throw new ArgumentException("Unknown rotor", nameof(Name));
            }
            var Chars = _charsets[Name];
            if (_ukw.Contains(Name))
            {
                return new UKW(TranslateUKW(Chars), Name.ToString());
            }
            if (_etw.Contains(Name))
            {
                return new ETW(Chars, Name.ToString());
            }
            var Notches = _notches.ContainsKey(Name) ? _notches[Name] : new int[0];
            return new Walze(Chars, Notches, StartPosition, RingPosition, Name.ToString());
        }

        /// <summary>
        /// Creates a custom rotor
        /// </summary>
        /// <param name="Alphabet">Rotor alphabet</param>
        /// <param name="StartPosition">Start position (0=A,25=Z)</param>
        /// <param name="RingPosition">Ring position (0=A,25=Z)</param>
        /// <returns>Rotor</returns>
        public static Walze GetCustomRotor(string Alphabet, int StartPosition = 0, int RingPosition = 0)
        {
            return new Walze(Alphabet, null, StartPosition, RingPosition);
        }

        /// <summary>
        /// Creates a custom entry rotor
        /// </summary>
        /// <param name="Alphabet">Alphabet</param>
        /// <returns>Entry rotor</returns>
        public static ETW GetCustomETW(string Alphabet)
        {
            return new ETW(Alphabet);
        }

        /// <summary>
        /// Gets a custom UKW
        /// </summary>
        /// <param name="Alphabet">Alphabet</param>
        /// <returns>UKW</returns>
        /// <remarks>
        /// The alphabet may omit one wire pair.
        /// The alphabet may need preparation if not in Plugboard format.
        /// See <see cref="TranslateUKW(string)"/> for more info.
        /// </remarks>
        public static UKW GetCustomUKW(string Alphabet)
        {
            return new UKW(Alphabet);
        }

        /// <summary>
        /// Gets a mixed alphabet of the given length
        /// </summary>
        /// <param name="Count">Character count, at most <see cref="CHARSET_SIZE"/></param>
        /// <returns>Mixed alphabet</returns>
        /// <remarks>
        /// The complete <see cref="CHARSET"/> is mixed
        /// before the first <paramref name="Count"/> chars are taken
        /// </remarks>
        public static string GetCryptoAlphabet(int Count = CHARSET_SIZE)
        {
            if (Count < 1 || Count > CHARSET_SIZE)
            {
                throw new ArgumentOutOfRangeException(nameof(Count));
            }
            var Chars = Tools.Randomize(CHARSET);
            return new string(Chars.Take(Count).ToArray());
        }

        /// <summary>
        /// Translates an <see cref="UKW"/> alphabet into the format used by this implementation
        /// </summary>
        /// <param name="Alphabet"><see cref="UKW"/> alphabet</param>
        /// <returns>Translated alphabet</returns>
        /// <remarks>
        /// UKW alphabets are special in how they match.
        /// For example, if it starts with "I",
        /// you will find "A" in the position where "I" is in <see cref="CHARSET"/>.
        /// The standard alphabet strings build reversible pairs when you line it up against <see cref="CHARSET"/>,
        /// but this implementation wants the two related characters together (like the Plugboard).
        /// This function translates it into said format.
        /// Note: You cannot translate all alphabets, only those with the matchin pairs.
        /// If this is difficult to understand, put the WW2_UKW_B charset right below <see cref="CHARSET"/>
        /// and you can see how these letter pairs appear twice, so if "Y" is below "A", then "A" will be below "Y"
        /// </remarks>
        public static string TranslateUKW(string Alphabet)
        {
            var Dict = new Dictionary<char, char>();
            for (var i = 0; i < Alphabet.Length; i++)
            {
                var offset = Alphabet[i] - 'A';
                Dict[(char)(Math.Min(offset, i) + 'A')] = (char)(Math.Max(offset, i) + 'A');
            }
            var Map = Dict.Select(m => $"{m.Key}{m.Value}").ToArray();
            return string.Concat(Map);
        }
    }

    /// <summary>
    /// All known and implemented rotor names
    /// </summary>
    public enum RotorName
    {
        //Commercial rotor types
        Comm_R1, Comm_R2, Comm_R3,

        //Types used in WW2
        WW2_I, WW2_II, WW2_III,
        WW2_IV, WW2_V, WW2_VI,
        WW2_VII, WW2_VIII,
        WW2_UKW_A, WW2_UKW_B, WW2_UKW_C,
        WW2_UKW_B_Thin, WW2_UKW_C_Thin,
        WW2_ETW, WW2_Beta, WW2_Gamma,

        //Types used by the german railway
        Rocket_I, Rocket_II, Rocket_III,
        Rocket_UKW, Rocket_ETW,

        //Types used by the Swiss-K system
        Swiss_I, Swiss_II, Swiss_III,
        Swiss_UKW, Swiss_ETW
    }
}

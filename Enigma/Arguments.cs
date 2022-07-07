using System;
using System.Collections.Generic;
using System.Linq;

namespace Enigma
{
    /// <summary>
    /// Parses command line arguments
    /// </summary>
    public class Arguments
    {
        /// <summary>
        /// Enigma rotors (excluding ETW and UKW)
        /// </summary>
        private List<Walze> rotors;

        /// <summary>
        /// Operation mode of the tool
        /// </summary>
        public ArgumentMode Mode { get; set; }

        /// <summary>
        /// Number of standard rotor codes to generate
        /// </summary>
        public int RandomCodeCount { get; set; }

        /// <summary>
        /// Year of the code sheet
        /// </summary>
        public int CodesheetYear { get; set; }

        /// <summary>
        /// Month of the code sheet
        /// </summary>
        public int CodesheetMonth { get; set; }

        /// <summary>
        /// File to write the codesheet to
        /// </summary>
        /// <remarks>If absent, it's written to the terminal</remarks>
        public string CodesheetFile { get; set; }

        /// <summary>
        /// Export the machine state before and after encryption
        /// </summary>
        public bool ExportState { get; set; }

        /// <summary>
        /// Print bare code sheet table without other HTML before or after it
        /// </summary>
        public bool CodesheetBare { get; set; }

        /// <summary>
        /// Filter unsupported characters instead of throwing exception
        /// </summary>
        public bool Filter { get; set; } = false;

        /// <summary>
        /// Group output into chunks of 5 characters
        /// </summary>
        /// <remarks>
        /// This will pad the input with junk bytes if needed at the very end
        /// </remarks>
        public bool Group { get; set; } = false;

        /// <summary>
        /// Translate digits into german names
        /// </summary>
        public bool TranslateNumbers { get; set; } = false;

        /// <summary>
        /// Plugboard configuration
        /// </summary>
        public Plugboard Plugboard { get; set; }

        /// <summary>
        /// UKW configuration
        /// </summary>
        public UKW UKW { get; set; }

        /// <summary>
        /// ETW configuration
        /// </summary>
        public ETW ETW { get; set; }

        /// <summary>
        /// Rotors (without UKW or ETW)
        /// </summary>
        /// <remarks>
        /// This returns a new copy each time to not modify the original values
        /// </remarks>
        public List<Walze> Rotors
        {
            get
            {
                return rotors.Select(m => (Walze)m.Clone()).ToList();
            }
            set
            {
                if (value == null)
                {
                    rotors = null;
                }
                else
                {
                    rotors = value.ToList();
                }
            }
        }

        /// <summary>
        /// Construct instance and parse arguments
        /// </summary>
        /// <param name="args">Command line arguments</param>
        public Arguments(string[] args)
        {
            //Check help. Help overrides all other arguments so the user can use /? at any given time.
            if (args == null || args.Length == 0 || args.Contains("/?"))
            {
                Mode = ArgumentMode.Help;
                return;
            }

            //Check random mode
            if (args.Any(m => m.ToUpper() == "/R"))
            {
                HandleRandomMode(args);
                return;
            }
            //checked codesheet mode
            if (args.Any(m => m.ToUpper() == "/C"))
            {
                HandleCodesheetMode(args);
                return;
            }
            //checked import mode
            if (args.Any(m => m.ToUpper() == "/I"))
            {
                HandleImportMode(args);
                return;
            }

            //Is neither mode so far. Default to encryption mode
            Mode = ArgumentMode.Encrypt;
            rotors = new List<Walze>();

            for (var i = 0; i < args.Length; i++)
            {
                var Current = args[i];
                var Next = i < args.Length - 1 ? args[i + 1] : null;

                switch (Current.ToUpper())
                {
                    case "/B":
                        throw new ArgumentException("/B is only valid together with /C");
                    case "/E":
                        ETW = CreateETW(Next);
                        ++i;
                        break;
                    case "/U":
                        UKW = CreateUKW(Next);
                        ++i;
                        break;
                    case "/W":
                        rotors.Add(CreateRotor(Next));
                        ++i;
                        break;
                    case "/S":
                        Plugboard = CreatePlugboard(Next);
                        ++i;
                        break;
                    case "/Z":
                        if (TranslateNumbers)
                        {
                            throw new ArgumentException("Duplicate /Z");
                        }
                        TranslateNumbers = true;
                        break;
                    case "/F":
                        if (Filter)
                        {
                            throw new ArgumentException("Duplicate /F");
                        }
                        Filter = true;
                        break;
                    case "/G":
                        if (Group)
                        {
                            throw new ArgumentException("Duplicate /G");
                        }
                        Group = true;
                        break;
                    case "/EX":
                        if (ExportState)
                        {
                            throw new ArgumentException("Duplicate /EX");
                        }
                        ExportState = true;
                        break;
                    default:
                        throw new ArgumentException($"Unknown argument: {Current}");
                }
            }
            //Validate combinations
            if (UKW == null)
            {
                throw new ArgumentException("Required argument /U is missing");
            }
            if (ETW == null)
            {
                ETW = (ETW)EnigmaRotors.GetRotor(RotorName.WW2_ETW);
            }
            if (rotors.Count == 0)
            {
                throw new ArgumentException("Required argument /W is missing");
            }
        }

        /// <summary>
        /// Processes arguments for /I
        /// </summary>
        /// <param name="args">Arguments</param>
        private void HandleImportMode(string[] args)
        {
            var Params = args.Select(m => m.ToUpper()).ToList();
            Params.Remove("/I");

            ExportState = Params.Remove("/EX");
            Filter = Params.Remove("/F");
            TranslateNumbers = Params.Remove("/Z");
            Group = Params.Remove("/G");

            if (Params.Count != 1)
            {
                throw new ArgumentException("/I requires exactly one parameter");
            }
            var M = EnigmaMachine.Import(Params[0]);
            UKW = M.Rotors.Rotors.OfType<UKW>().First();
            ETW = M.Rotors.Rotors.OfType<ETW>().Last();
            rotors = M.Rotors.Rotors.OfType<Walze>().ToList();
        }

        /// <summary>
        /// Processes arguments for /C
        /// </summary>
        /// <param name="args">Arguments</param>
        private void HandleCodesheetMode(string[] args)
        {
            for (var i = 0; i < args.Length; i++)
            {
                var Current = args[i];
                var next = i < args.Length - 1 ? args[i + 1] : null;
                switch (Current.ToUpper())
                {
                    case "/C":
                        if (CodesheetMonth > 0)
                        {
                            throw new ArgumentException("Duplicate /C argument");
                        }
                        if (next == null)
                        {
                            throw new ArgumentException("/C requires a parameter");
                        }
                        if (next.ToUpper() == "CURRENT")
                        {
                            var DT = DateTime.Now;
                            CodesheetYear = DT.Year;
                            CodesheetMonth = DT.Month;
                        }
                        else
                        {
                            DateTime DT;
                            var parts = next.Split('-');
                            if (parts.Length != 2)
                            {
                                throw new ArgumentException("/C: Date format must be YYYY-MM");
                            }
                            if (!int.TryParse(parts[0], out int y) || y < DateTime.MinValue.Year || y > DateTime.MaxValue.Year)
                            {
                                throw new ArgumentException("/C: Year is invalid. Format must be YYYYY");
                            }
                            if (!int.TryParse(parts[1], out int m) || m < 1 || m > 12)
                            {
                                throw new ArgumentException("/C: Month is invalid. Possible values: 1 to 12 inclusive");
                            }
                            try
                            {
                                //Try instantiating a date and checking the month length
                                //to see if it fits within the acceptable range.
                                DT = new DateTime(y, m, 1, 0, 0, 0, DateTimeKind.Local);
                                DateTime.DaysInMonth(y, m);
                            }
                            catch (Exception ex)
                            {
                                throw new ArgumentException("/C: Date outside of permitted range", ex);
                            }
                            CodesheetMonth = DT.Month;
                            CodesheetYear = DT.Year;
                        }
                        ++i;
                        break;
                    case "/B":
                        if (CodesheetBare)
                        {
                            throw new ArgumentException("Duplicate /B");
                        }
                        CodesheetBare = true;
                        break;
                    case "/O":
                        if (CodesheetFile != null)
                        {
                            throw new ArgumentException("Duplicate /O");
                        }
                        if (string.IsNullOrEmpty(next))
                        {
                            throw new ArgumentException("/O requires a parameter");
                        }
                        CodesheetFile = next;
                        ++i;
                        break;
                    default:
                        throw new ArgumentException($"Unknown argument: '{Current}'");
                }
            }
            Mode = ArgumentMode.CodeSheet;
        }

        /// <summary>
        /// Processes arguments for /R
        /// </summary>
        /// <param name="args">Arguments</param>
        private void HandleRandomMode(string[] args)
        {
            var Params = args.Select(m => m.ToUpper()).ToList();
            Params.Remove("/R");
            if (Params.Count == 0)
            {
                RandomCodeCount = 3;
            }
            else if (Params.Count == 1)
            {
                //We limit the number of codes to not stress the cryptographgic RNG too much
                if (!int.TryParse(Params[0], out int result) || result < 1 || result > 99)
                {
                    throw new ArgumentOutOfRangeException(nameof(args), "Rotor count must be in the range of 1 to 99 inclusive");
                }
                RandomCodeCount = result;
            }
            else
            {
                throw new ArgumentException("/R has at most one argument");
            }
            Mode = ArgumentMode.Random;
        }

        /// <summary>
        /// Gets an initialized Enigma machine from the arguments
        /// </summary>
        /// <returns>Enigma machine</returns>
        public EnigmaMachine GetMachine()
        {
            if (Mode != ArgumentMode.Encrypt || UKW == null || ETW == null)
            {
                throw new InvalidOperationException("Cannot create machine. Invalid mode or missing parameters");
            }
            return new EnigmaMachine(new RotorAssembly(UKW, Rotors, ETW), Plugboard);
        }

        /// <summary>
        /// Creates a plugboard instance from the given configuration
        /// </summary>
        /// <param name="next">Configuration</param>
        /// <returns>Plugboard</returns>
        private Plugboard CreatePlugboard(string next)
        {
            if (string.IsNullOrWhiteSpace(next))
            {
                throw new ArgumentException("/W requires an argument");
            }
            if (Plugboard != null)
            {
                throw new ArgumentException("Duplicate /S");
            }
            next = Tools.FixAlphabet(next);
            if (next.Length % 2 != 0)
            {
                throw new ArgumentException("Plugboard requires an even number of letters");
            }
            return new Plugboard(next.Chunk(2).ToArray());
        }

        /// <summary>
        /// Creates a standard rotor from the given configuration
        /// </summary>
        /// <param name="next">Configuration</param>
        /// <returns>Rotor</returns>
        private Walze CreateRotor(string next)
        {
            if (string.IsNullOrWhiteSpace(next))
            {
                throw new ArgumentException("/W requires an argument");
            }
            var parts = next.Split(':');
            var Ring = 0;
            var Offset = 0;
            var Notch = new int[0];

            parts[0] = Tools.FixAlphabet(parts[0]);

            if (parts.Length > 1)
            {
                Ring = Tools.AlphaToNum(parts[1]);
                if (parts.Length > 2)
                {
                    Offset = Tools.AlphaToNum(parts[2]);
                    if (parts.Length > 3)
                    {
                        try
                        {
                            Notch = parts[3]
                                .Select(m => Tools.AlphaToNum(m))
                                .ToArray();
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Notch setting is invalid. " + ex.Message, ex);
                        }
                    }
                }
            }
            if (parts[0].Length == EnigmaRotors.CHARSET_SIZE)
            {
                var CustomRotor = EnigmaRotors.GetCustomRotor(parts[0], Offset, Ring);
                CustomRotor.Notches = Notch;
                return CustomRotor;
            }
            if (!Tools.GetRotorName(parts[0], out RotorName parsed))
            {
                throw new ArgumentException($"'{parts[0]}' is neither a valid alphabet nor rotor name");
            }
            if (!EnigmaRotors.Standard.Contains(parsed))
            {
                throw new ArgumentException($"'{parts[0]}' is not a standard rotor.");
            }
            if (Notch.Length > 0)
            {
                throw new ArgumentException("Custom notch setting is only supported on custom rotors.");
            }
            return (Walze)EnigmaRotors.GetRotor(parsed, Offset, Ring);
        }

        /// <summary>
        /// Creates an UKW from the given configuration
        /// </summary>
        /// <param name="next">Configuration</param>
        /// <returns>UKW</returns>
        private UKW CreateUKW(string next)
        {
            if (string.IsNullOrWhiteSpace(next))
            {
                throw new ArgumentException("/U requires an argument");
            }
            if (UKW != null)
            {
                throw new ArgumentException("Duplicate /U");
            }
            next = Tools.FixAlphabet(next);
            if (next.Length == EnigmaRotors.CHARSET_SIZE || next.Length == EnigmaRotors.CHARSET_SIZE - 2)
            {
                return EnigmaRotors.GetCustomUKW(next);
            }
            if (!Tools.GetRotorName(next, out RotorName parsed))
            {
                throw new ArgumentException($"'{next}' is neither a valid alphabet nor rotor name");
            }
            if (!EnigmaRotors.UKW.Contains(parsed))
            {
                throw new ArgumentException($"'{next}' is not an UKW");
            }
            return (UKW)EnigmaRotors.GetRotor(parsed);
        }

        /// <summary>
        /// Creates an ETW from the given configuration
        /// </summary>
        /// <param name="next">Configuration</param>
        /// <returns>ETW</returns>
        private ETW CreateETW(string next)
        {
            if (string.IsNullOrWhiteSpace(next))
            {
                throw new ArgumentException("/E requires an argument");
            }
            if (ETW != null)
            {
                throw new ArgumentException("Duplicate /E");
            }
            next = Tools.FixAlphabet(next);
            if (next.Length == EnigmaRotors.CHARSET_SIZE)
            {
                return EnigmaRotors.GetCustomETW(next);
            }
            if (!Tools.GetRotorName(next, out RotorName parsed))
            {
                throw new ArgumentException($"'{next}' is neither a valid alphabet nor rotor name");
            }
            if (!EnigmaRotors.ETW.Contains(parsed))
            {
                throw new ArgumentException($"'{next}' is not an ETW");
            }
            return (ETW)EnigmaRotors.GetRotor(parsed);
        }
    }

    /// <summary>
    /// Operation modes
    /// </summary>
    public enum ArgumentMode
    {
        /// <summary>
        /// Encryption and decryption
        /// </summary>
        Encrypt,
        /// <summary>
        /// Random rotor generation
        /// </summary>
        Random,
        /// <summary>
        /// Code sheet generation
        /// </summary>
        CodeSheet,
        /// <summary>
        /// Command line help
        /// </summary>
        Help
    }
}

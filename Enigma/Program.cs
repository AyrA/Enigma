using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Enigma
{
    /// <summary>
    /// Main class
    /// </summary>
    class Program
    {
        /// <summary>
        /// Return value
        /// </summary>
        private struct RET
        {
            /// <summary>
            /// Program exits without errors
            /// </summary>
            public const int SUCCESS = 0;
            /// <summary>
            /// Encryption/Decryption failure
            /// </summary>
            public const int ENCRYPTION_FAIL = 1;
            /// <summary>
            /// Help request
            /// </summary>
            public const int HELP = 255;
            /// <summary>
            /// Command line parser failure
            /// </summary>
            public const int ARG_FAIL = 254;
        }

        /// <summary>
        /// Main entry point
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <returns>Exit code</returns>
        static int Main(string[] args)
        {
            //Try to parse arguments
            Arguments A;
            try
            {
                A = new Arguments(args);
            }
            catch (Exception ex)
            {
                ReportError(ex, "Error parsing command line arguments.");
                return RET.ARG_FAIL;
            }
            //Handle various operation modes
            switch (A.Mode)
            {
                case ArgumentMode.Encrypt:
                    return HandleEncryption(A);
                case ArgumentMode.Random:
                    return HandleRandom(A);
                case ArgumentMode.CodeSheet:
                    return HandleCodesheet(A);
                case ArgumentMode.Help:
                    Help();
                    //Show possible rotor names
                    WL("ETW: " + string.Join(", ", EnigmaRotors.ETW));
                    WL("UKW: " + string.Join(", ", EnigmaRotors.UKW));
                    WL("Walze: " + string.Join(", ", EnigmaRotors.Standard));
                    return RET.HELP;
            }
            throw new NotImplementedException($"Unknown mode (developer error?): {A.Mode}");
        }

        /// <summary>
        /// Generates a code sheet
        /// </summary>
        /// <param name="A">Code sheet arguments</param>
        /// <returns><see cref="RET.SUCCESS"/></returns>
        private static int HandleCodesheet(Arguments A)
        {
            var Days = DateTime.DaysInMonth(A.CodesheetYear, A.CodesheetMonth);

            //Calculate the days the UKW setting changes.
            //This was every 8 days, but the code sheet is in reverse
            var UKW_Days = new Dictionary<int, int>();
            for (var i = 0; i < Days; i += 8)
            {
                var Age = Math.Min(8, Days - i);
                UKW_Days[i + Age] = Age;
            }
            var Table = new Table(7);
            var Headers = "Tag|Walzenlage|Ringstellung|Umkehrwalze|Steckerbrett|Kenngruppen"
                .Split('|')
                .Select(m => new TableCell(m))
                .ToArray();
            Headers[^1].Colspan = 2;
            Table.SetHeader(Headers);
            while (Days > 0)
            {
                var Row = new List<TableCell>();
                Row.Add(new TableCell(Days));

                //Select 3 random rotors from the list of poaaible rotor names
                var Rotors = Tools.Randomize("I|II|III|IV|V".Split('|')).Take(3);
                Row.Add(new TableCell(string.Join(" ", Rotors)));

                //Ring setting with leading zeros
                Row.Add(new TableCell(string.Format("{0:00} {1:00} {2:00}", Tools.Random(EnigmaRotors.CHARSET_SIZE) + 1, Tools.Random(EnigmaRotors.CHARSET_SIZE) + 1, Tools.Random(EnigmaRotors.CHARSET_SIZE) + 1)));

                //Do UKW every 8 days
                if (UKW_Days.ContainsKey(Days))
                {
                    //UKW
                    var UKW = string.Join(" ", EnigmaRotors.GetCryptoAlphabet(EnigmaRotors.CHARSET_SIZE - 2).Chunk(2));
                    var Lines = string.Join(Environment.NewLine, UKW.Chunk((UKW.Length + 1) / 3).Select(m => m.Trim()));
                    Row.Add(new TableCell(Lines, 1, UKW_Days[Days]));
                }
                //Plugboard with 10 settings.
                //If you chance the charset you may want to have more or less plugs here
                Row.Add(new TableCell(string.Join(" ", EnigmaRotors.GetCryptoAlphabet()[0..20].Chunk(2))));

                //4 Kenngruppen in two cells
                string[] KG;
                //Ensure all 4 keys are different
                do
                {
                    KG = new string[]
                    {
                        new string(EnigmaRotors.GetCryptoAlphabet(3).OrderBy(m => m).ToArray()),
                        new string(EnigmaRotors.GetCryptoAlphabet(3).OrderBy(m => m).ToArray()),
                        new string(EnigmaRotors.GetCryptoAlphabet(3).OrderBy(m => m).ToArray()),
                        new string(EnigmaRotors.GetCryptoAlphabet(3).OrderBy(m => m).ToArray())
                    };
                } while (KG.Length != KG.Distinct().Count());

                for (var i = 0; i < 2; i++)
                {
                    Row.Add(new TableCell(string.Join(" ", KG[0 + i * 2], KG[1 + i * 2]).ToLower()));
                }

                Table.AddRow(Row.ToArray());
                --Days;
            }
            //Fake key number for Months starting at 1900
            int KeyNumber;
            if (A.CodesheetYear < 1900)
            {
                KeyNumber = Math.Abs(A.CodesheetYear) * 100 + A.CodesheetMonth;
            }
            else
            {
                KeyNumber = (A.CodesheetYear - 1900) * 12 + A.CodesheetMonth;
            }
            using (var outfile = A.CodesheetFile == null ? Console.Out : File.CreateText(A.CodesheetFile))
            {
                if (A.CodesheetBare)
                {
                    Table.Render(outfile);
                }
                else
                {
                    Tools.RenderHTML(Table, outfile, KeyNumber, A.CodesheetMonth, A.CodesheetYear);
                }
            }
            return RET.SUCCESS;
        }

        /// <summary>
        /// Prints random charsets for the various rotor types and plugboard
        /// </summary>
        /// <param name="A">Random command arguments</param>
        /// <returns><see cref="RET.SUCCESS"/></returns>
        private static int HandleRandom(Arguments A)
        {
            //Rotors and ETW
            for (var i = 0; i < A.RandomCodeCount; i++)
            {
                Console.WriteLine("Walze + ETW: {0}", EnigmaRotors.GetCryptoAlphabet());
            }
            //UKW
            Console.WriteLine("UKW        : {0}", string.Join("-", EnigmaRotors.GetCryptoAlphabet().Substring(2).Chunk(2)));
            //Plugboard
            Console.WriteLine("Steckbrett : {0}", string.Join("-", EnigmaRotors.GetCryptoAlphabet().Substring(6).Chunk(2)));
            return RET.SUCCESS;
        }

        /// <summary>
        /// Handles encryption and decryption
        /// </summary>
        /// <param name="A">Encryption command line arguments</param>
        /// <returns>Exit code</returns>
        private static int HandleEncryption(Arguments A)
        {
            var Machine = A.GetMachine();
            string Output = string.Empty;
            int Groups = 0;
            if (A.ExportState)
            {
                Console.Error.WriteLine(Machine.GetState());
            }
            while (true)
            {
                try
                {
                    var Line = Console.ReadLine();
                    if (Line == null)
                    {
                        //Fill output
                        if (Output.Length % 5 > 0)
                        {
                            Output += EnigmaRotors.GetCryptoAlphabet(5 - (Output.Length % 5));
                        }
                        //Write remaining data
                        Output = WriteGroup(Output, ref Groups);
                        break;
                    }
                    //Filter whitespace and do ÄÖÜ
                    Line = new string(Line.ToUpper()
                        .Replace("Ä", "AE")
                        .Replace("Ö", "OE")
                        .Replace("Ü", "UE")
                        .Where(m => !char.IsWhiteSpace(m))
                        .ToArray());
                    //Translate digits
                    if (A.TranslateNumbers)
                    {
                        Line = Line
                            .Replace("0", "NULL")
                            .Replace("1", "EINS")
                            .Replace("2", "ZWEI")
                            .Replace("3", "DREI")
                            .Replace("4", "VIER")
                            .Replace("5", "FUNF")
                            .Replace("6", "SECHS")
                            .Replace("7", "SIEBEN")
                            .Replace("8", "ACHT")
                            .Replace("9", "NEUN");
                    }
                    if (A.Filter)
                    {
                        Line = new string(Line.Where(m => m >= 'A' && m <= 'Z').ToArray());
                    }
                    else if (Line.Any(m => m < 'A' || m > 'Z'))
                    {
                        throw new InvalidDataException($"The line '{Line}' contains characters outside of the enigma charset (A-Z). " +
                            "Tip: Use the filter option (/F) to remove unwanted characters, and the number option (/Z) to replace digits with words.");
                    }
                    Line = Machine.Encrypt(Line);
                    if (A.Group)
                    {
                        Output = WriteGroup(Output + Line, ref Groups);
                    }
                    else
                    {
                        Console.Write(Line);
                    }
                }
                catch (Exception ex)
                {
                    ReportError(ex, "Cryptographic error");
                    return RET.ENCRYPTION_FAIL;
                }
            }
            Console.WriteLine();
            if (A.ExportState)
            {
                Console.Error.WriteLine(Machine.GetState());
            }
            return RET.SUCCESS;
        }

        /// <summary>
        /// Writes output in group of 5 characters
        /// </summary>
        /// <param name="output">Text to write</param>
        /// <param name="groups">Number of groups written (modulo 10)</param>
        /// <returns>Potentially remaining characters of <paramref name="groups"/></returns>
        private static string WriteGroup(string output, ref int groups)
        {
            while (output.Length >= 5)
            {
                Console.Write(output.Substring(0, 5));
                output = output[5..];
                if (++groups == 10)
                {
                    Console.Write(Environment.NewLine);
                    groups = 0;
                }
                else
                {
                    Console.Write(' ');
                }
            }
            return output;
        }

        /// <summary>
        /// Writes error message to the console
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="Title">Error title</param>
        private static void ReportError(Exception ex, string Title)
        {
            Console.Error.WriteLine(Title);
            if (ex == null)
            {
                Console.Error.WriteLine("Unknown error occured");
            }
            while (ex != null)
            {
                Console.Error.WriteLine("{0}: {1}", ex.GetType(), ex.Message);
                ex = ex.InnerException;
            }
        }

        /// <summary>
        /// Prints the excessively long help text
        /// </summary>
        private static void Help()
        {
            Console.WriteLine(@"Enigma.exe [/E <etw>] /U <ukw> [/W <walze[:Ring[:Pos[:Kerbe]]]>] /S <Steckerbrett> [/F] [/G] [/Z] [/EX]
Enigma.exe /R [count]
Enigma.exe /C {YYYY-MM|current} [/B] [/O <filename>]
Enigma.exe /I <data> [/F] [/G] [/Z] [/EX]

Encrypts and decrypts text using the german enigma mechanism.
--> Do not use for real world applications. Enigma has been broken im world war 2

It reads text from the keyboard (or redirected input) and writes transformed text to the output.
Note: Encryption and decryption for an enigma are the same.
To decrypt, simply run the tool with the same arguments and feed it the encrypted text.

Required parameters:
====================
/U  : Umkehrwalze: Either a name, or a string representing character pairs.

/W  : Walze: Either a name, or a 26 character string.
      The name/setting can be followed by the ring and position offset.
      The offsets can be given using A-Z or 1-26.
      Finally (for custom rotors only) you can specify where the turnover
      notch is (A-Z). Multiple values can be specified: 'AMR' will rotate
      the rotor to the left whenever your rotor moves from A to B, M to N
      or R to S.
      /W is repeatable for as often as you want.
      Standard WW2 enigma has 3 rotors. Some had 4.
      This emulator also allows you to use the same rotor multiple times.

      Example: /W Swiss_I,5,F
      Uses the Swiss_I rotor with ring offset 5 (=E) and start offset F (=6).
      Example: /W IV
      Uses WW2 rotor four with no ring or start offset.
      Example: /W LVUQKTGZYOJHBCXRPAMWEFNID:Q:E:AMT
      Uses a custom rotor with ring at Q and offset E.
      The turnover of the next rotor happens when moving past A, M, and T.

      CAUTION! The rotors are defined from left to right as seen by a user.
      This is the way they were also written in the codebook.
      However, the signal enters (and exits) on the right side of the rotors.

/R  : Generate and print random charsets:
      a 24 character alphabet for the UKW
      a 20 character alphabet for the plugboard (10 plugs)
      26 character alphabets for a rotor.
      Prints as many of these as the parameter specifies.
      If the parameter is not present, will print 3.

/C  : Create a code sheet like they used in WW2.
      The parameter specifies the year and month the sheet is for
      and affects how many days the sheet will contain.
      The tool uses the current month and year if 'current' is used instead.
      The sheet is for a 3 rotor machine with the WW2_I to WW2_V rotors.
      The output will be HTML and should be redirected to a file
      or another application. It uses a print-friendly design.

      Caution! The code sheet is different each time, even if you
      generate multiple sheets for the same date.

/I  : Import from a state previously exported using /EX

      Example:
      /I U:UKW_B;R:II:5:Q;E:ETW;S:TMOVNLCEWRDAQYZPHJSI
      This is equal to:
      /U UKW_B /W II:5:Q /E ETW /S TM-OV-NL-CE-WR-DA-QY-ZP-HJ-SI

Optional parameters:
====================
/E  : Eintrittswalze: Either a name, or a 26 character string.
      If not specified, it uses one that has A-Z in order.
      This was the most common type.

      Example: /E WW2_ETW
      Uses the ETW from WW2 enigma machines.
      Example: /E NPXHZMSQYEVTJLIOCARFKUWG
      Uses a custom ETW with the given alphabet.

/S  : Steckerbrett: Configure which plugs to use.
      The argument should be up to 13 unique letter pairs.

      Example: /S AQ-DS-WR
      Plugs A with Q, D with S, and W with R.

/F  : This turns lowercase in the input into uppercase and removes
      unsupported characters from the input.
      Whitespace is always removed, even if this is not supplied.

/G  : This prints the output with characters grouped to a length of 5.
      This format was commonly used back in WW2.
      If the last group happens to be shorter than 5 characters,
      it's padded with junk. This in turn means that decryption
      will also have just as many junk characters at the very end.

/Z  : This converts digits into german words for them.
      Will only convert individual digits. '11' will not become 'ELF'
      but 'EINSEINS'

/O  : When creating a code sheet, write to the given file instead of the
      terminal. This will fix encoding issues if your äöü looks messed up.
      Another solution would be to set your terminal to UTF-8.

/B  : When creating a code sheet, output the bare HTML table without any
      other code around it. Without this it outputs a full HTML document.

/EX : Exports machine state to standard error before and after the message.
      The string can be used later with /I to import an existing state.

Rotor and plugboard strings:
============================
This emulator supports custom rotors for all three types.
Regardless of where it's used, you can pad it with dashes and whitespace.
When using whitespace you must enclose the charset in quotes.
This means all these are identical:
ABCD
AB-CD
--A---BC---D

ETW and standard rotors:
------------------------
The charset explains the mapping from A-Z. A charset that starts with
WQB will map an incomming A to W, B to Q, and C to B.
The charset must consist of 26 letters A-Z, each one used exactly once.
supplying ABCD....WXYZ will create a rotor that won't swap any letters.
The standard ETW is wired this way.

UKW and plugboard:
------------------
The charset represents pairs of letters to swap. A charset starting with
NPXHZM will swap N with P, X with H and Z with M.
You cannot swap a letter with itself or one that's already used in another
swap pair. The plugboard will have as many plugs as you specify pairs (0-13).
Ten plugs were used in WW2.
Note: For the UKW you may skip one of the cable settings and the tool will
automatically fill it in for you. Original german codesheets made the same
assumption.

Translations:
=============
Walze: A rotor that encrypts/decrypts a letter and potentially rotates as the machine is used.
UKW, Umkehrwalze: The rotor at the end that reflects the signal back into the rotors. It's stationary
ETW, Eintrittswalze: The first rotor. It's stationary and not user swappable.
Steckerbrett: The plugboard, where up to 13 plugs can connect letters in pairs to swap them
Kerbe: Notch. Indentation that makes a rotor turn the one on the left too.

Rotors:
=======
The following short names are supported pre-built rotors.
Note: Rotor names that start with 'WW2_' can be used without this prefix.
'WW2_IV' and 'IV' are thus identical.
");
        }

        /// <summary>
        /// Writes text but tries to avoid line breaks within words
        /// </summary>
        /// <param name="Text">Text</param>
        private static void W(string Text)
        {
            if (string.IsNullOrEmpty(Text))
            {
                return;
            }
            var LineLength = Console.BufferWidth;
            var LinePos = Console.CursorLeft;
            var Matches = Regex.Matches(Text, @"\w+").OfType<Match>().ToArray();
            var SB = new StringBuilder();
            for (var i = 0; i < Text.Length; i++)
            {
                var M = Matches.FirstOrDefault(m => m.Index <= i && m.Index + m.Length > i);
                if (M == null)
                {
                    SB.Append(Text[i]);
                    if (Text[i] == '\n')
                    {
                        LinePos = 0;
                    }
                    else
                    {
                        ++LinePos;
                    }
                }
                else
                {
                    if (LineLength - LinePos <= M.Length)
                    {
                        SB.Append(Environment.NewLine);
                        LinePos = 0;
                    }
                    SB.Append(M.Value);
                    LinePos += M.Length;
                    i += M.Length - 1;
                }
            }
            Console.Write(SB);
        }

        /// <summary>
        /// Calls <see cref="W(string)"/> and then writes a line break
        /// </summary>
        /// <param name="Text">Text</param>
        private static void WL(string Text)
        {
            if (string.IsNullOrEmpty(Text))
            {
                Console.WriteLine();
                return;
            }
            W(Text);
            Console.WriteLine();
        }
    }
}

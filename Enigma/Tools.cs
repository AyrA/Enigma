using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Enigma
{
    /// <summary>
    /// Generic/Global tools and extension methods
    /// </summary>
    public static class Tools
    {
        #region HTML

        /// <summary>
        /// CSS code for the HTML framework.
        /// </summary>
        /// <remarks>
        /// This is in a separate constant due to its size and to allow $"{...}" in the renderer.
        /// It renders the table nice on screen and even fancier on a printer.
        /// The result should fit common paper formats (A4) and the backwards US formats (legal, letter) too.
        /// The rules print in landscape.
        /// Note that the fancy "gothic" font is technically not correct, but widely available.
        /// </remarks>
        private const string CSS = @"
body{
    font-family:Times New Roman,Serif;
}
a{
    color:#F00;
}
th,td{
    font-family:Courier New,Monospace;
    text-align:center;
    padding:0 1em;
}
.attr{
    font-family:Times New Roman,Serif;
}
@media print{
    @page {
        /*
            If you want to irritate americans even more
            than the completely german output will already do,
            specify 'A4 landscape' so they have to fiddle with
            their printer each time they want to print this.
        */
        size: landscape;
    }
    body{
        text-align:center;
        /*
            'Old English Text MT' is not the correct german font as the name implies,
            but it's an acceptable substitute and seems to be included in later versions of Windows.
            'Fraktur' is still added first just in case someone has it.
        */
        font-family:Fraktur,Old English Text MT,Times New Roman,Serif;
    }
    .hidden-print{
        display:none;
    }
    h1,p{
        /*Removing text margins to fit the table on the page*/
        margin:0;
    }
    td,th{
        font-size:9pt;
        border:2px solid #000;
    }
    table{
        /*The table ignores the text-align:center of the parent. The auto margin fixes that*/
        margin:auto;
        border-collapse:collapse;
        border:2px solid #000;
        border-top:5px solid #000;
    }
    a{
        text-decoration:none;
        color:#000;
    }
}";

        #endregion

        /// <summary>
        /// Generates an unbiased random number between 0 inclusive and <paramref name="maxExcl"/> exclusive
        /// </summary>
        /// <param name="maxExcl">The exclusive upper bound</param>
        /// <returns>Number that is at least 0 but less than <paramref name="maxExcl"/></returns>
        /// <remarks>
        /// To have a different minimum value, use var x = min + Random(max + min)
        /// This method uses a discard-and-retry strategy
        /// to avoid the low value bias that a simple modular
        /// arithmetic operation would produce.
        /// </remarks>
        public static int Random(int maxExcl)
        {
            return RandomNumberGenerator.GetInt32(maxExcl);
        }

        /// <summary>
        /// Converts A-Z into 0-25
        /// </summary>
        /// <param name="s">Input string</param>
        /// <returns>Number</returns>
        /// <remarks>
        /// The input string is first attempted to be parsed as integer in the 1-26 range
        /// before resorting to converting the A-Z range.
        /// In the latter case, the string must consist of only one character.
        /// Empty strings and null is treated as zero.
        /// 1 is subtracted from the 1-26 integer to make it equal to the A-Z result of 0-25
        /// </remarks>
        public static int AlphaToNum(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return 0;
            }
            if (int.TryParse(s, out int i) && i >= 1 && i <= EnigmaRotors.CHARSET_SIZE)
            {
                return i - 1;
            }
            if (s.Length == 1)
            {
                var c = s.ToUpper()[0];
                if (c >= 'A' && c <= 'Z')
                {
                    return c - 'A';
                }
            }
            throw new ArgumentException($"Value not 1-26 or A-Z: '{s}'");
        }

        /// <summary>
        /// Converts A-Z into 0-25
        /// </summary>
        /// <param name="c">Character</param>
        /// <returns>0-25</returns>
        public static int AlphaToNum(char c)
        {
            if (c >= 'a' && c <= 'z')
            {
                return c - 'a';
            }
            if (c >= 'A' && c <= 'Z')
            {
                return c - 'A';
            }
            throw new ArgumentException($"Character '{c}' is not within A-Z");
        }

        /// <summary>
        /// Fixes various alphabet formatting.
        /// Currently removes dashes and whitespace,
        /// and converts to uppercase
        /// </summary>
        /// <param name="Alphabet">Alphabet</param>
        /// <returns>Filtered alphabet</returns>
        /// <remarks>This does not validate the alphabet</remarks>
        public static string FixAlphabet(string Alphabet)
        {
            if (string.IsNullOrEmpty(Alphabet))
            {
                return Alphabet;
            }
            return new string(Alphabet.Where(m => m != '-' && !char.IsWhiteSpace(m)).ToArray()).ToUpper();
        }

        /// <summary>
        /// Tries to parse the supplied string as a known rotor name.
        /// The "WW2_" prefix is optional,
        /// and the name is case insensitive
        /// </summary>
        /// <param name="name">Rotor name</param>
        /// <param name="value">Parsed value</param>
        /// <returns>Parse success result</returns>
        /// <remarks>Tries to parse as exact name before trying with an added "WW2_" prefix</remarks>
        public static bool GetRotorName(string name, out RotorName value)
        {
            return
                //Parse exactly first, then with added "WW2_"
                (Enum.TryParse(name, true, out value) || Enum.TryParse($"WW2_{name}", true, out value)) &&
                //Ensure the parsed value actually exists (it may not if the supplied string is a number)
                Enum.IsDefined(typeof(RotorName), value);
        }

        /// <summary>
        /// Randomizes the order of elements in the argument
        /// </summary>
        /// <typeparam name="T">Argument type</typeparam>
        /// <param name="Source">Sequence to be randomized</param>
        /// <returns>Randomized list</returns>
        public static List<T> Randomize<T>(IEnumerable<T> Source)
        {
            var Ret = new List<T>();
            var Data = Source.ToList();
            while (Data.Count > 0)
            {
                Ret.Add(Data.GetAndRemove(Random(Data.Count)));
            }
            return Ret;
        }

        /// <summary>
        /// Renders the supplied table as an entire HTML page
        /// </summary>
        /// <param name="T">HTML Table</param>
        /// <param name="TW">Output writer</param>
        /// <param name="KeyNumber">Key number used in the title</param>
        /// <param name="Month">Month printed below the table</param>
        /// <param name="Year">Year printed below the table</param>
        /// <remarks>
        /// <paramref name="KeyNumber"/>,
        /// <paramref name="Month"/>, and
        /// <paramref name="Year"/> are purely for aesthetics
        /// </remarks>
        public static void RenderHTML(Table T, TextWriter TW, int KeyNumber, int Month, int Year)
        {
            var MonthName = "Januar,Februar,März,April,Mai,Juni,Juli,August,September,Oktober,November,Dezember".Split(',')[Month - 1];
            TW.Write(@$"<!DOCTYPE html>
<html lang=de>
<!--
    Created by using this image as template:
    https://en.wikipedia.org/wiki/File:Enigma_keylist_3_rotor.jpg
-->
<head>
<meta charset=utf-8 />
<title>Enigma Geheimschlüssel Nr. {KeyNumber}</title>
<style>{CSS}</style>
</head>
<body>
<p class=hidden-print><button onclick=window.print()>Drucken</button></p>
<p>
    <u>Geheime Kommandosache!</u>
    Jeder <u>einzelne</u> Tagesschlüssel ist geheim.
    Mitnahme im Flugzeug verboten!
</p>
<h1>Mashinen-Geheimschlüssel {KeyNumber}</h1>
<p>
    <b>Achtung!</b>
    Schlüsselmittel dürfen nicht unversehrt in Feindeshand fallen.
    Bei Gefahr restlos und frühzeitig vernichten.
</p>
");
            T.Render(TW);
            TW.Write(@$"
<p>Verbrauchte Tagesschlüssel sind abzutrennen und zu vernichten!</p>
<p>Tabelle Zeitraum: {MonthName}, {Year}</p>
<p class=attr>
    Erstellt von
    <a href=https://github.com/AyrA/Enigma>https://github.com/AyrA/Enigma</a>
</p>
</body></html>");
        }

        /// <summary>
        /// Get an item from the list and remove it
        /// </summary>
        /// <typeparam name="T">List type</typeparam>
        /// <param name="List">List</param>
        /// <param name="Index">Item index</param>
        /// <returns>Removed item</returns>
        /// <remarks>This is not thread safe</remarks>
        public static T GetAndRemove<T>(this List<T> List, int Index)
        {
            T Value = List[Index];
            List.RemoveAt(Index);
            return Value;
        }

        /// <summary>
        /// Splits a string into evenly sized chunks.
        /// </summary>
        /// <param name="s">String to split</param>
        /// <param name="ChunkLength">Chunk length</param>
        /// <returns>Chunked string</returns>
        /// <remarks>
        /// The last chunk may be shorter if it doesn't exactly fits the string length
        /// </remarks>
        public static IEnumerable<string> Chunk(this string s, int ChunkLength)
        {
            int i = 0;
            int l = s.Length;
            while (i < l)
            {
                int chunk = Math.Min(ChunkLength, l - i);
                yield return s.Substring(i, chunk);
                i += ChunkLength;
            }
        }
    }
}
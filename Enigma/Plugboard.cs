using System;
using System.Collections.Generic;
using System.Linq;

namespace Enigma
{
    /// <summary>
    /// Represents the Enigma plugboard,
    /// which may or may not swap letters around.
    /// </summary>
    /// <remarks>
    /// From an implementation perspective,
    /// the <see cref="UKW"/> and plugboard are very similar.
    /// </remarks>
    public class Plugboard
    {
        /// <summary>
        /// Character mappings
        /// </summary>
        private readonly Dictionary<char, char> _plugs;

        /// <summary>
        /// Character pairs
        /// </summary>
        private readonly string[] pairs;

        /// <summary>
        /// Gets the plug pairs of this instance
        /// </summary>
        public string[] Pairs => (string[])pairs.Clone();

        /// <summary>
        /// Creates a plugboard with the given plug pairs
        /// </summary>
        /// <param name="Pairs">
        /// Anywhere from 0 to 13 pairs.
        /// Null is acceptable too for a completely unused plugboard.
        /// </param>
        public Plugboard(params string[] Pairs)
        {
            _plugs = new Dictionary<char, char>();
            if (Pairs != null)
            {
                foreach (var P in Pairs)
                {
                    //A pair must consist of exactly two different letters that have not yet been used
                    if (!ValidPair(P))
                    {
                        throw new ArgumentException($"Invalid plugboard setting '{P}'", nameof(Pairs));
                    }
                    if (_plugs.Remove(P[0]) || _plugs.Remove(P[1]))
                    {
                        throw new ArgumentException($"Invalid plugboard setting '{P}'. At least one letter is already used", nameof(Pairs));
                    }
                    //Create map
                    _plugs[P[0]] = P[1];
                    _plugs[P[1]] = P[0];
                }
            }
            //Create straight map for unused characters
            for (var c = 'A'; c <= 'Z'; c++)
            {
                if (!_plugs.ContainsKey(c))
                {
                    _plugs[c] = c;
                }
            }
            pairs = (string[])Pairs.Clone();
        }

        /// <summary>
        /// Checks if the given pair is valid
        /// </summary>
        /// <param name="Pair">Pair</param>
        /// <returns>true, if valid</returns>
        /// <remarks>
        /// Doesn't validates againse existing pairs,
        /// just validates formal correctness
        /// </remarks>
        private bool ValidPair(string Pair)
        {

            return
                Pair != null && Pair.Length == 2 &&
                EnigmaRotors.CHARSET.Contains(Pair[0]) &&
                EnigmaRotors.CHARSET.Contains(Pair[1]);
        }

        /// <summary>
        /// Maps a character to another
        /// </summary>
        /// <param name="c">Character</param>
        /// <returns>Mapped characters</returns>
        /// <remarks>
        /// Mapping an unplugged character returns the character itself.
        /// Mappings are reflective, so if "A" maps to "S", then "S" maps back to "A"
        /// </remarks>
        public char Map(char c)
        {
            if (_plugs.ContainsKey(c))
            {
                return _plugs[c];
            }
            throw new ArgumentException($"Character '{c}' is not valid");
        }

        /// <summary>
        /// Maps an entire string of characters
        /// </summary>
        /// <param name="s">Source</param>
        /// <returns>Mapped string</returns>
        public string Map(string s)
        {
            return string.Concat(s.Select(Map));
        }

        /// <summary>
        /// Gets the plugboard state for export purposes
        /// </summary>
        /// <param name="OmitDashes">Omit the dashes between pairs</param>
        /// <returns>"S:" and plug settings</returns>
        public string GetState(bool OmitDashes)
        {
            var ret = string.Empty;
            foreach (var KV in _plugs)
            {
                if (KV.Key != KV.Value && !ret.Contains(KV.Key))
                {
                    ret += KV.Key.ToString() + KV.Value.ToString();
                }
            }
            return "S:" + (OmitDashes ? ret : string.Join("-", ret.Chunk(2)));
        }
    }
}

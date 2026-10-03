#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace UniverseGeneration
{
    /// <summary>
    /// Where an object sits: generator version, seed text, the level it was generated from, and the path below it, as a
    /// URL-safe text such as <c>v1-my-seed/galaxy/system/31/planet/2</c> (plan D17). The address alone regenerates the
    /// object, and its <see cref="ObjectSeed"/> is the same however the object was reached.
    /// </summary>
    internal sealed class Address
    {
        private readonly (string Label, int Index)[] _path;

        public Address(int version, string seed, string root)
            : this(version, seed, root, Array.Empty<(string, int)>())
        {
        }

        private Address(int version, string seed, string root, (string Label, int Index)[] path)
        {
            if (version < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(version), version, "Generator versions start at 1.");
            }

            if (seed is null)
            {
                throw new ArgumentNullException(nameof(seed));
            }

            if (!IsLabel(root))
            {
                throw new ArgumentException($"A level name is lowercase letters a to z; got \"{root}\".", nameof(root));
            }

            Version = version;
            Seed = seed;
            Root = root;
            _path = path;
        }

        public int Version { get; }

        /// <summary>The seed text as the caller gave it (unescaped).</summary>
        public string Seed { get; }

        /// <summary>The level generated from the seed: universe, cluster, galaxy, system or planet.</summary>
        public string Root { get; }

        public IReadOnlyList<(string Label, int Index)> Path => _path;

        /// <summary>The seed of the object at this address.</summary>
        public ulong ObjectSeed
        {
            get
            {
                var seed = RootSeed(Version, Seed, Root);
                foreach (var (label, index) in _path)
                {
                    seed = Seeds.Child(seed, label, index);
                }

                return seed;
            }
        }

        /// <summary>The seed of an object generated directly from a seed text at the level <paramref name="root"/>.</summary>
        public static ulong RootSeed(int version, string seed, string root) =>
            Seeds.Child(Seeds.FromText(seed, version), root, 0);

        public Address Child(string label, int index)
        {
            if (!IsLabel(label))
            {
                throw new ArgumentException($"A label is lowercase letters a to z; got \"{label}\".", nameof(label));
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, "An index cannot be negative.");
            }

            var path = new (string, int)[_path.Length + 1];
            Array.Copy(_path, path, _path.Length);
            path[_path.Length] = (label, index);
            return new Address(Version, Seed, Root, path);
        }

        public override string ToString()
        {
            var text = new StringBuilder();
            text.Append('v').Append(Version.ToString(CultureInfo.InvariantCulture)).Append('-');
            EscapeSeed(Seed, text);
            text.Append('/').Append(Root);
            foreach (var (label, index) in _path)
            {
                text.Append('/').Append(label).Append('/').Append(index.ToString(CultureInfo.InvariantCulture));
            }

            return text.ToString();
        }

        /// <summary>Reads an address; on failure <paramref name="error"/> says what is wrong in words.</summary>
        public static bool TryParse(string? text, out Address? address, out string error)
        {
            address = null;
            if (string.IsNullOrEmpty(text))
            {
                error = "The address is empty.";
                return false;
            }

            var parts = text!.Split('/');
            var head = parts[0];
            var dash = head.IndexOf('-');
            if (head.Length < 3 || head[0] != 'v' || dash < 2 || !TryReadIndex(head.Substring(1, dash - 1), out var version) || version < 1)
            {
                error = $"An address starts with v, the generator version and a dash, as in v1-my-seed/galaxy; got \"{head}\".";
                return false;
            }

            if (!TryUnescapeSeed(head.Substring(dash + 1), out var seed))
            {
                error = "The seed part of the address has a character that must be percent-encoded, or a broken %XX sequence.";
                return false;
            }

            if (parts.Length < 2 || !IsLabel(parts[1]))
            {
                error = "After the seed comes the level it was generated from, as in v1-my-seed/galaxy.";
                return false;
            }

            if (parts.Length % 2 != 0)
            {
                error = "Below the level, the address alternates a name and an index, as in /system/31/planet/2.";
                return false;
            }

            var path = new (string, int)[(parts.Length - 2) / 2];
            for (var i = 0; i < path.Length; i++)
            {
                var label = parts[2 + 2 * i];
                var indexText = parts[3 + 2 * i];
                if (!IsLabel(label) || !TryReadIndex(indexText, out var index))
                {
                    error = $"\"{label}/{indexText}\" is not a name followed by an index (0, 1, 2, ... without leading zeros).";
                    return false;
                }

                path[i] = (label, index);
            }

            address = new Address(version, seed, parts[1], path);
            error = "";
            return true;
        }

        private static bool IsLabel(string? s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return false;
            }

            foreach (var c in s!)
            {
                if (c < 'a' || c > 'z')
                {
                    return false;
                }
            }

            return true;
        }

        private static bool TryReadIndex(string s, out int value)
        {
            value = 0;
            if (s.Length == 0 || s.Length > 10 || (s.Length > 1 && s[0] == '0'))
            {
                return false;
            }

            long v = 0;
            foreach (var c in s)
            {
                if (c < '0' || c > '9')
                {
                    return false;
                }

                v = v * 10 + (c - '0');
            }

            if (v > int.MaxValue)
            {
                return false;
            }

            value = (int)v;
            return true;
        }

        private static bool IsUnreserved(int c) =>
            (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '-' || c == '.' || c == '_' || c == '~';

        /// <summary>RFC 3986 unreserved characters stay; everything else becomes %XX of its UTF-8 bytes (a lone surrogate as U+FFFD).</summary>
        private static void EscapeSeed(string seed, StringBuilder text)
        {
            const string Hex = "0123456789ABCDEF";
            var bytes = Encode(seed);
            foreach (var b in bytes)
            {
                if (IsUnreserved(b))
                {
                    text.Append((char)b);
                }
                else
                {
                    text.Append('%').Append(Hex[b >> 4]).Append(Hex[b & 15]);
                }
            }
        }

        private static bool TryUnescapeSeed(string escaped, out string seed)
        {
            seed = "";
            var bytes = new List<byte>(escaped.Length);
            for (var i = 0; i < escaped.Length; i++)
            {
                var c = escaped[i];
                if (c == '%')
                {
                    if (i + 2 >= escaped.Length)
                    {
                        return false;
                    }

                    var high = HexValue(escaped[i + 1]);
                    var low = HexValue(escaped[i + 2]);
                    if (high < 0 || low < 0)
                    {
                        return false;
                    }

                    bytes.Add((byte)(high * 16 + low));
                    i += 2;
                }
                else if (IsUnreserved(c))
                {
                    bytes.Add((byte)c);
                }
                else
                {
                    return false;
                }
            }

            var array = bytes.ToArray();
            seed = Decode(array);
            var again = Encode(seed);
            if (again.Length != array.Length)
            {
                return false;
            }

            for (var i = 0; i < again.Length; i++)
            {
                if (again[i] != array[i])
                {
                    return false;
                }
            }

            return true;
        }

        private static int HexValue(char c) =>
            c >= '0' && c <= '9' ? c - '0' : c >= 'A' && c <= 'F' ? c - 'A' + 10 : c >= 'a' && c <= 'f' ? c - 'a' + 10 : -1;

        private static byte[] Encode(string s)
        {
            var bytes = new List<byte>(s.Length);
            for (var i = 0; i < s.Length; i++)
            {
                int c = s[i];
                if (c >= 0xD800 && c <= 0xDBFF && i + 1 < s.Length && s[i + 1] >= 0xDC00 && s[i + 1] <= 0xDFFF)
                {
                    c = 0x10000 + ((c - 0xD800) << 10) + (s[i + 1] - 0xDC00);
                    i++;
                }
                else if (c >= 0xD800 && c <= 0xDFFF)
                {
                    c = 0xFFFD;
                }

                if (c < 0x80)
                {
                    bytes.Add((byte)c);
                }
                else if (c < 0x800)
                {
                    bytes.Add((byte)(0xC0 | (c >> 6)));
                    bytes.Add((byte)(0x80 | (c & 0x3F)));
                }
                else if (c < 0x10000)
                {
                    bytes.Add((byte)(0xE0 | (c >> 12)));
                    bytes.Add((byte)(0x80 | ((c >> 6) & 0x3F)));
                    bytes.Add((byte)(0x80 | (c & 0x3F)));
                }
                else
                {
                    bytes.Add((byte)(0xF0 | (c >> 18)));
                    bytes.Add((byte)(0x80 | ((c >> 12) & 0x3F)));
                    bytes.Add((byte)(0x80 | ((c >> 6) & 0x3F)));
                    bytes.Add((byte)(0x80 | (c & 0x3F)));
                }
            }

            return bytes.ToArray();
        }

        private static string Decode(byte[] bytes) => new UTF8Encoding(false, false).GetString(bytes);
    }
}

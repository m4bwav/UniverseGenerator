using System.Collections.Generic;
using System.Text;

namespace UniverseGeneration.Tests
{
    /// <summary>
    /// Removes named keys (and their values) from compact JSON written by <see cref="JsonWriter"/>. export.json holds the
    /// export as it was when the file was written; a field added later is filtered out here and gets its own golden file,
    /// so export.json never changes (AGENTS.md, the seed promise).
    /// </summary>
    internal static class JsonKeyFilter
    {
        /// <summary>Keys added to the export after export.json was written, each with the golden file that holds it.</summary>
        public static readonly HashSet<string> AddedLater = new HashSet<string>
        {
            "eccentricity", "inclination", "periapsisAngle", // orbital-elements.json
            "warnings", // diagnostics.json
        };

        public static string Without(string json, ISet<string> keys)
        {
            var text = new StringBuilder(json.Length);
            var i = 0;
            while (i < json.Length)
            {
                var c = json[i];
                if (c == '"')
                {
                    var end = StringEnd(json, i);
                    var isKey = end + 1 < json.Length && json[end + 1] == ':';
                    if (isKey && keys.Contains(json.Substring(i + 1, end - i - 1)))
                    {
                        // Drop "key":value and the comma on one side of it.
                        var after = ValueEnd(json, end + 2);
                        if (after < json.Length && json[after] == ',')
                        {
                            after++;
                        }
                        else if (text.Length > 0 && text[text.Length - 1] == ',')
                        {
                            text.Length--;
                        }

                        i = after;
                        continue;
                    }

                    text.Append(json, i, end - i + 1);
                    i = end + 1;
                    continue;
                }

                text.Append(c);
                i++;
            }

            return text.ToString();
        }

        private static int StringEnd(string json, int start)
        {
            var i = start + 1;
            while (json[i] != '"')
            {
                i += json[i] == '\\' ? 2 : 1;
            }

            return i;
        }

        private static int ValueEnd(string json, int start)
        {
            var depth = 0;
            var i = start;
            while (i < json.Length)
            {
                var c = json[i];
                if (c == '"')
                {
                    i = StringEnd(json, i) + 1;
                    continue;
                }

                if (c == '{' || c == '[')
                {
                    depth++;
                }
                else if (c == '}' || c == ']')
                {
                    if (depth == 0)
                    {
                        return i;
                    }

                    depth--;
                }
                else if (c == ',' && depth == 0)
                {
                    return i;
                }

                i++;
            }

            return i;
        }
    }
}

#nullable enable
using System;
using System.Globalization;
using System.Text;

namespace UniverseGeneration
{
    /// <summary>
    /// A small hand-written JSON reader for <see cref="GeneratorTables.FromJson"/>: objects, arrays, strings and whole
    /// numbers, read in order by the caller (no reflection, no tree). Errors name the character position.
    /// </summary>
    internal sealed class JsonReader
    {
        private readonly string _text;
        private int _at;

        public JsonReader(string text)
        {
            _text = text;
        }

        /// <summary>Reads an object, calling <paramref name="value"/> with each key; it must read that key's value.</summary>
        public void ReadObject(Action<string> value)
        {
            Expect('{');
            if (Peek() == '}')
            {
                _at++;
                return;
            }

            while (true)
            {
                var key = ReadString();
                Expect(':');
                value(key);
                var c = Next();
                if (c == '}')
                {
                    return;
                }

                if (c != ',')
                {
                    throw Error("expected , or } after a value");
                }
            }
        }

        /// <summary>Reads an array, calling <paramref name="item"/> once per item; it must read the item.</summary>
        public void ReadArray(Action item)
        {
            Expect('[');
            if (Peek() == ']')
            {
                _at++;
                return;
            }

            while (true)
            {
                item();
                var c = Next();
                if (c == ']')
                {
                    return;
                }

                if (c != ',')
                {
                    throw Error("expected , or ] after an item");
                }
            }
        }

        public string ReadString()
        {
            Expect('"');
            var text = new StringBuilder();
            while (true)
            {
                if (_at >= _text.Length)
                {
                    throw Error("a string is not closed");
                }

                var c = _text[_at++];
                if (c == '"')
                {
                    return text.ToString();
                }

                if (c < ' ')
                {
                    throw Error("a control character inside a string");
                }

                if (c != '\\')
                {
                    text.Append(c);
                    continue;
                }

                if (_at >= _text.Length)
                {
                    throw Error("a string is not closed");
                }

                var e = _text[_at++];
                switch (e)
                {
                    case '"': text.Append('"'); break;
                    case '\\': text.Append('\\'); break;
                    case '/': text.Append('/'); break;
                    case 'b': text.Append('\b'); break;
                    case 'f': text.Append('\f'); break;
                    case 'n': text.Append('\n'); break;
                    case 'r': text.Append('\r'); break;
                    case 't': text.Append('\t'); break;
                    case 'u':
                        var code = 0;
                        for (var k = 0; k < 4; k++)
                        {
                            var h = _at + k < _text.Length ? Hex(_text[_at + k]) : -1;
                            if (h < 0)
                            {
                                throw Error("a \\u escape needs four hex digits");
                            }

                            code = code * 16 + h;
                        }

                        text.Append((char)code);
                        _at += 4;
                        break;
                    default:
                        throw Error($"\"\\{e}\" is not a JSON escape");
                }
            }
        }

        /// <summary>Reads a whole number that fits in an int; fractions and exponents are refused.</summary>
        public int ReadInt()
        {
            SkipSpace();
            var start = _at;
            if (_at < _text.Length && _text[_at] == '-')
            {
                _at++;
            }

            var digits = _at;
            while (_at < _text.Length && _text[_at] >= '0' && _text[_at] <= '9')
            {
                _at++;
            }

            if (_at == digits || _at - digits > 10 || (_at < _text.Length && (_text[_at] == '.' || _text[_at] == 'e' || _text[_at] == 'E')))
            {
                throw Error("expected a whole number");
            }

            long value = 0;
            for (var k = digits; k < _at; k++)
            {
                value = value * 10 + (_text[k] - '0');
            }

            value = digits > start ? -value : value;
            if (value < int.MinValue || value > int.MaxValue)
            {
                throw Error("the number is too large");
            }

            return (int)value;
        }

        /// <summary>Checks that only white space follows.</summary>
        public void End()
        {
            SkipSpace();
            if (_at < _text.Length)
            {
                throw Error("unexpected text after the end");
            }
        }

        public FormatException Error(string what) =>
            new FormatException($"Tables JSON, at character {_at.ToString(CultureInfo.InvariantCulture)}: {what}.");

        private static int Hex(char c) =>
            c >= '0' && c <= '9' ? c - '0' : c >= 'a' && c <= 'f' ? c - 'a' + 10 : c >= 'A' && c <= 'F' ? c - 'A' + 10 : -1;

        private void Expect(char c)
        {
            if (Peek() != c)
            {
                throw Error($"expected {c}");
            }

            _at++;
        }

        private char Next()
        {
            SkipSpace();
            return _at < _text.Length ? _text[_at++] : '\0';
        }

        private char Peek()
        {
            SkipSpace();
            return _at < _text.Length ? _text[_at] : '\0';
        }

        private void SkipSpace()
        {
            while (_at < _text.Length && (_text[_at] == ' ' || _text[_at] == '\t' || _text[_at] == '\n' || _text[_at] == '\r'))
            {
                _at++;
            }
        }
    }
}

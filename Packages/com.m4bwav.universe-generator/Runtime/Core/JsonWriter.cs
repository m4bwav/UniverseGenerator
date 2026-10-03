#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace UniverseGeneration
{
    /// <summary>
    /// A small hand-written JSON writer: the same text on every runtime (no serializer version, culture or reflection in
    /// the way), LF line ends, and numbers printed from integers at a fixed number of decimals.
    /// </summary>
    internal sealed class JsonWriter
    {
        private readonly StringBuilder _text = new StringBuilder();
        private readonly bool _indented;
        private readonly Stack<bool> _hasItems = new Stack<bool>();
        private bool _afterName;

        public JsonWriter(bool indented)
        {
            _indented = indented;
        }

        public JsonWriter BeginObject() => Open('{');

        public JsonWriter EndObject() => Close('}');

        public JsonWriter BeginArray() => Open('[');

        public JsonWriter EndArray() => Close(']');

        public JsonWriter Name(string name)
        {
            BeforeValue();
            AppendString(name);
            _text.Append(_indented ? ": " : ":");
            _afterName = true;
            return this;
        }

        public JsonWriter String(string? value)
        {
            if (value is null)
            {
                return Null();
            }

            BeforeValue();
            AppendString(value);
            return this;
        }

        public JsonWriter Int(long value)
        {
            BeforeValue();
            _text.Append(value.ToString(CultureInfo.InvariantCulture));
            return this;
        }

        /// <summary>
        /// A number rounded to <paramref name="decimals"/> places (0 to 15) as <see cref="DMath.Round"/> does, printed
        /// without trailing zeros. NaN and infinities are written as null.
        /// </summary>
        public JsonWriter Number(double value, int decimals)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                return Null();
            }

            BeforeValue();
            var p = DMath.PowerOfTen(decimals);
            var scaled = Math.Floor(Math.Abs(value) * p + 0.5);
            if (scaled >= 9007199254740992.0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "The value is too large for its number of decimals.");
            }

            var digits = ((long)scaled).ToString(CultureInfo.InvariantCulture);
            if (scaled != 0 && value < 0)
            {
                _text.Append('-');
            }

            if (decimals == 0)
            {
                _text.Append(digits);
                return this;
            }

            digits = digits.PadLeft(decimals + 1, '0');
            var whole = digits.Substring(0, digits.Length - decimals);
            var fraction = digits.Substring(digits.Length - decimals).TrimEnd('0');
            _text.Append(whole);
            if (fraction.Length > 0)
            {
                _text.Append('.').Append(fraction);
            }

            return this;
        }

        public JsonWriter Bool(bool value)
        {
            BeforeValue();
            _text.Append(value ? "true" : "false");
            return this;
        }

        public JsonWriter Null()
        {
            BeforeValue();
            _text.Append("null");
            return this;
        }

        /// <summary>The text written so far; an indented document ends with a line feed.</summary>
        public override string ToString() => _indented && _hasItems.Count == 0 && _text.Length > 0 ? _text + "\n" : _text.ToString();

        private JsonWriter Open(char bracket)
        {
            BeforeValue();
            _text.Append(bracket);
            _hasItems.Push(false);
            return this;
        }

        private JsonWriter Close(char bracket)
        {
            var hadItems = _hasItems.Pop();
            if (hadItems)
            {
                NewLine();
            }

            _text.Append(bracket);
            return this;
        }

        private void BeforeValue()
        {
            if (_afterName)
            {
                _afterName = false;
                return;
            }

            if (_hasItems.Count == 0)
            {
                return;
            }

            if (_hasItems.Peek())
            {
                _text.Append(',');
            }
            else
            {
                _hasItems.Pop();
                _hasItems.Push(true);
            }

            NewLine();
        }

        private void NewLine()
        {
            if (_indented)
            {
                _text.Append('\n').Append(' ', 2 * _hasItems.Count);
            }
        }

        private void AppendString(string value)
        {
            const char Backslash = '\\';
            _text.Append('"');
            foreach (var c in value)
            {
                switch (c)
                {
                    case '"': _text.Append(Backslash).Append('"'); break;
                    case Backslash: _text.Append(Backslash).Append(Backslash); break;
                    case '\n': _text.Append(Backslash).Append('n'); break;
                    case '\r': _text.Append(Backslash).Append('r'); break;
                    case '\t': _text.Append(Backslash).Append('t'); break;
                    default:
                        if (c < 0x20 || (c >= 0xD800 && c <= 0xDFFF))
                        {
                            // Control characters, and every surrogate half so a lone one stays valid JSON text.
                            _text.Append(Backslash).Append('u').Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            _text.Append(c);
                        }

                        break;
                }
            }

            _text.Append('"');
        }
    }
}

// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace Sejil.Data.Query.Internal
{
    internal class Scanner
    {
        private readonly string _source;
        private readonly List<Token> _tokens = new();
        private readonly Dictionary<string, TokenType> _keywords = new()
        {
            { "and", TokenType.And },
            { "or", TokenType.Or },
            { "like", TokenType.Like },
            { "true", TokenType.True },
            { "false", TokenType.False },
        };

        private int _start;
        private int _current;

        private bool IsAtEnd => _current >= _source.Length;

        public Scanner(string source) => _source = source;

        public List<Token> Scan()
        {
            while (!IsAtEnd)
            {
                _start = _current;
                ScanToken();
            }

            _tokens.Add(new Token(TokenType.Eol, _current, "", null));
            return _tokens;
        }

        private void ScanToken()
        {
            var c = Advance();
            switch (c)
            {
                case '(':
                    AddToken(TokenType.OpenParenthesis);
                    break;
                case ')':
                    AddToken(TokenType.CloseParenthesis);
                    break;
                case '=':
                    AddToken(TokenType.Equal);
                    break;
                case '!':
                    if (Match('='))
                    {
                        AddToken(TokenType.NotEqual);
                    }
                    else
                    {
                        throw new QueryEngineException($"Unexpected character at position '{_current}'.");
                    }
                    break;
                case '\'':
                    ReadString();
                    break;
                case ' ':
                case '\t':
                case '\r':
                case '\n':
                    break;
                default:
                    if (char.IsDigit(c))
                    {
                        ReadNumber();
                    }
                    else if (char.IsLetter(c))
                    {
                        ReadIdentifier();
                    }
                    else
                    {
                        throw new QueryEngineException($"Unexpected character at position '{_current}'.");
                    }
                    break;
            }
        }

        private void ReadString()
        {
            while (!IsAtEnd && (Peek() != '\'' || (Peek() == '\'' && PeekPrev() == '\\')))
            {
                Advance();
            }

            if (IsAtEnd)
            {
                throw new QueryEngineException($"Unterminated string at position '{_current + 1}'.");
            }

            // Consume the closing "'"
            Advance();

            var value = _source.Substring(_start + 1, _current - _start - 2).Replace(@"\'", "''");
            AddToken(TokenType.String, value);
        }

        private void ReadNumber()
        {
            while (char.IsDigit(Peek()))
            {
                Advance();
            }

            if (Peek() == '.' && char.IsDigit(PeekNext()))
            {
                // Consume the "."
                Advance();

                while (char.IsDigit(Peek()))
                {
                    Advance();
                }
            }

            var value = _source.Substring(_start, _current - _start);
            AddToken(TokenType.Number, decimal.Parse(value));
        }

        private void ReadIdentifier()
        {
            while (char.IsLetterOrDigit(Peek()))
            {
                Advance();
            }

            var text = _source.Substring(_start, _current - _start);

            // "not" can only be used with "like"
            if (text.ToLower() == "not")
            {
                while (char.IsWhiteSpace(Peek()))
                {
                    Advance();
                }

                var start = _current;

                while (char.IsLetter(Peek()))
                {
                    Advance();
                }

                text = _source.Substring(start, _current - start).ToLower();
                if (text == "like")
                {
                    AddToken(TokenType.NotLike);
                }
                else
                {
                    throw new QueryEngineException($"Unexpected character at position '{start + 1}': \"not\" keyword may only be used with \"like\" keyword.");
                }
            }
            else
            {
                var type = _keywords.ContainsKey(text.ToLower()) ? _keywords[text.ToLower()] : (TokenType?)null;
                AddToken(type is null ? TokenType.Identifier : type.Value);
            }
        }

        private void AddToken(TokenType type) => AddToken(type, null);

        private void AddToken(TokenType type, object literal)
        {
            var text = _source.Substring(_start, _current - _start);
            _tokens.Add(new Token(type, _start, text, literal));
        }

        private char Advance() => _source[_current++];

        private char PeekPrev() => _current - 1 < 0
            ? '\0'
            : _source[_current - 1];

        private char Peek() => IsAtEnd
            ? '\0'
            : _source[_current];

        private char PeekNext() => _current + 1 >= _source.Length
            ? '\0'
            : _source[_current + 1];

        private bool Match(char expected)
        {
            if (IsAtEnd || _source[_current] != expected)
            {
                return false;
            }

            _current++;
            return true;
        }
    }
}
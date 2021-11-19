// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

namespace Sejil.Data.Query;

public sealed class Token
{
    public TokenType Type { get; }
    public int Position { get; }
    public string Text { get; }
    public object Value { get; }

    public Token(TokenType type, int position, string text, object value)
    {
        Type = type;
        Position = position;
        Text = text;
        Value = value;
    }
}

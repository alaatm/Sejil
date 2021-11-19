// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

namespace Sejil.Data.Query;

internal static class TokenExtensions
{
    public static string Negate(this Token token) => token.Type == TokenType.NotEqual
        ? "="
        : token.Type == TokenType.NotLike
            ? "LIKE"
            : token.Text.ToUpperInvariant();

    public static bool IsExluding(this Token token) => token.Type is TokenType.NotEqual or TokenType.NotLike;
}

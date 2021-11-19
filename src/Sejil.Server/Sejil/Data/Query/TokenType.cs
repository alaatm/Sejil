// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

namespace Sejil.Data.Query;

internal enum TokenType
{
    Identifier,
    BuiltInIdentifier,
    String,
    Number,

    OpenParenthesis,
    CloseParenthesis,

    And,
    Or,
    Like,
    NotLike,
    True,
    False,

    Equal,
    NotEqual,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,

    Eol,
}

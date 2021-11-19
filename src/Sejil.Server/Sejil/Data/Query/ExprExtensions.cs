// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

namespace Sejil.Data.Query;

internal static class ExprExtensions
{
    /// <summary>
    /// Returns whether all parts of the specified expression are property types.
    /// i.e. none of them are a built-in property (one that starts with @)
    /// </summary>
    /// <param name="expr">The expression.</param>
    /// <returns></returns>
    public static bool HasAllProperty(this Expr expr) => expr switch
    {
        Expr.Grouping g => g.Expression.HasAllProperty(),
        Expr.Logical l => l.Left.HasAllProperty() && l.Right.HasAllProperty(),
        Expr.Binary b => b.Left.HasAllProperty() && b.Right.HasAllProperty(),
        Expr.Variable v => v.Token.Type is TokenType.Identifier,
        Expr.Literal => true,
        _ => false,
    };
}

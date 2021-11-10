// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using System.Text;

namespace Sejil.Data.Query.Internal
{
    internal static class TokenExtensions
    {
        public static string NegateIfNonInclusion(this Token token) => token.Type == TokenType.NotEqual
            ? "="
            : token.Type == TokenType.NotLike
                ? "LIKE"
                : token.Text;

        public static bool IsExluding(this Token token) => token.Type is TokenType.NotEqual or TokenType.NotLike;
    }

    internal class CodeGenerator : Expr.IVisitor
    {
        private readonly StringBuilder _sql = new();
        private bool _insidePropBlock;

        public string Generate(Expr expr)
        {
            Resolve(expr);
            if (_insidePropBlock)
            {
                _sql.Append(')');
            }
            return _sql.ToString();
        }

        private void Resolve(Expr expr) => expr.Accept(this);

        public void Visit(Expr.Binary expr)
        {
            CheckPropertyScope(expr);

            Resolve(expr.Left);

            _sql.Append(expr.IsProperty
                ? $"value {expr.Operator.NegateIfNonInclusion().ToUpper()} "
                : $" {expr.Operator.Text.ToUpper()} ");

            Resolve(expr.Right);

            if (expr.Left.IsProperty)
            {
                _sql.Append($") {(expr.Operator.IsExluding() ? "=" : ">")} 0");
            }
        }

        public void Visit(Expr.Grouping expr)
        {
            CheckPropertyScope(expr);

            _sql.Append('(');
            Resolve(expr.Expression);
            _sql.Append(')');
        }

        public void Visit(Expr.Literal expr) => _sql.Append($"'{expr.Value}'");

        public void Visit(Expr.Logical expr)
        {
            Resolve(expr.Left);
            if (_insidePropBlock && !expr.Right.IsProperty)
            {
                _sql.Append(')');
            }
            _sql.Append($" {expr.Operator.Text.ToUpper()} ");
            Resolve(expr.Right);
        }

        public void Visit(Expr.Variable expr) => _sql.Append(expr.IsProperty
            ? $"SUM(name = '{expr.Token.Text}' AND "
            : expr.Token.Text);

        private void CheckPropertyScope(Expr expr)
        {
            if (expr.IsProperty && !_insidePropBlock)
            {
                _sql.Append("id IN (SELECT logId FROM log_property GROUP BY logId HAVING ");
                _insidePropBlock = true;
            }
            else if (!expr.IsProperty)
            {
                _insidePropBlock = false;
            }
        }
    }
}

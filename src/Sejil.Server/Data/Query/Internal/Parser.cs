// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;

namespace Sejil.Data.Query.Internal
{
    internal class Parser
    {
        private readonly List<Token> _tokens;
        private readonly string[] _nonPropertyColumns;
        private int _current;

        private bool IsAtEnd => Peek().Type == TokenType.Eol;

        public Parser(List<Token> tokens, string[] nonPropertyColumns) => (_tokens, _nonPropertyColumns) = (tokens, nonPropertyColumns.Select(p => p.ToLower()).ToArray());

        public Expr Parse()
        {
            var expr = Or();
            if (!IsAtEnd)
            {
                throw new QueryEngineException(Error(Peek(), "Expect end of line."));
            }

            return expr;
        }

        private Expr Or()
        {
            var expr = And();

            while (Match(TokenType.Or))
            {
                var op = Previous();
                var right = And();
                expr = new Expr.Logical(expr, op, right, expr.IsProperty, right.IsProperty);
            }

            return expr;
        }

        private Expr And()
        {
            var expr = Equality();

            while (Match(TokenType.And))
            {
                var op = Previous();
                var right = Equality();
                expr = new Expr.Logical(expr, op, right, expr.IsProperty, right.IsProperty);
            }

            return expr;
        }

        private Expr Equality()
        {
            var expr = Comparison();

            while (Match(TokenType.NotEqual, TokenType.Equal))
            {
                var op = Previous();
                var right = Comparison(expr.IsProperty);
                expr = new Expr.Binary(expr, op, right, expr.IsProperty);
            }

            return expr;
        }

        private Expr Comparison(bool isProperty = false)
        {
            var expr = Primary(isProperty);

            while (Match(TokenType.Like, TokenType.NotLike))
            {
                var op = Previous();
                var right = Primary(expr.IsProperty);
                expr = new Expr.Binary(expr, op, right, expr.IsProperty);
            }

            return expr;
        }

        private Expr Primary(bool isProperty)
        {
            if (Match(TokenType.False))
            {
                return new Expr.Literal(false, isProperty);
            }

            if (Match(TokenType.True))
            {
                return new Expr.Literal(true, isProperty);
            }

            if (Match(TokenType.Number, TokenType.String))
            {
                return new Expr.Literal(Previous().Value, isProperty);
            }

            if (Match(TokenType.Identifier))
            {
                var prev = Previous();
                isProperty = !_nonPropertyColumns.Contains(prev.Text.ToLower());
                return new Expr.Variable(Previous(), isProperty);
            }

            if (Match(TokenType.OpenParenthesis))
            {
                var expr = Or();
                isProperty = expr is Expr.Logical logExpr ? logExpr.IsProperty && logExpr.IsRightProperty : expr.IsProperty;
                Consume(TokenType.CloseParenthesis, "Expect ')' after expression.");
                return new Expr.Grouping(expr, isProperty);
            }

            throw new QueryEngineException(Error(Peek(), "Expect expression."));
        }

        private bool Match(params TokenType[] types)
        {
            foreach (var type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }

            return false;
        }

        private Token Consume(TokenType type, string message)
        {
            if (Check(type))
            {
                return Advance();
            }

            throw new QueryEngineException(Error(Peek(), message));
        }

        private bool Check(TokenType type) => !IsAtEnd && Peek().Type == type;

        private Token Advance()
        {
            if (!IsAtEnd)
            {
                _current++;
            }

            return Previous();
        }

        private Token Peek() => _tokens[_current];

        private Token Previous() => _tokens[_current - 1];

        static string Error(Token token, string message) => token.Type == TokenType.Eol
            ? $"Error at position '{token.Position + 1}': {message}"
            : $"Error at position '{token.Position + 1}' '{token.Text}': {message}";
    }
}
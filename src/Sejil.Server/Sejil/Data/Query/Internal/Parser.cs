// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

namespace Sejil.Data.Query.Internal;

internal sealed class Parser
{
    private readonly List<Token> _tokens;
    private int _current;

    private bool IsAtEnd => Peek().Type == TokenType.Eol;

    public Parser(List<Token> tokens) => _tokens = tokens;

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
            CheckLogicalLeftRight(expr, _tokens[_current - 2]);
            var op = Previous();
            var right = And();
            CheckLogicalLeftRight(right, Previous());
            expr = new Expr.Logical(expr, op, right);
        }

        return expr;
    }

    private Expr And()
    {
        var expr = Binary();

        while (Match(TokenType.And))
        {
            CheckLogicalLeftRight(expr, _tokens[_current - 2]);
            var op = Previous();
            var right = Binary();
            CheckLogicalLeftRight(right, Previous());
            expr = new Expr.Logical(expr, op, right);
        }

        return expr;
    }

    private Expr Binary()
    {
        var expr = Primary();

        while (Match(
            TokenType.NotEqual,
            TokenType.Equal,
            TokenType.Like,
            TokenType.NotLike,
            TokenType.GreaterThan,
            TokenType.GreaterThanOrEqual,
            TokenType.LessThan,
            TokenType.LessThanOrEqual))
        {
            CheckBinaryLeft(expr, _tokens[_current - 2]);
            var op = Previous();
            var right = Primary();
            CheckBinaryRight(right, op, Previous());
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Primary()
    {
        if (Match(TokenType.False))
        {
            return new Expr.Literal("'False'");
        }

        if (Match(TokenType.True))
        {
            return new Expr.Literal("'True'");
        }

        if (Match(TokenType.Number, TokenType.String))
        {
            return new Expr.Literal(Previous().Value);
        }

        if (Match(TokenType.Identifier, TokenType.BuiltInIdentifier))
        {
            return new Expr.Variable(Previous());
        }

        if (Match(TokenType.OpenParenthesis))
        {
            var expr = Or();
            Consume(TokenType.CloseParenthesis, "Expect ')' after expression.");
            return new Expr.Grouping(expr);
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

    private static void CheckLogicalLeftRight(Expr expr, Token token)
    {
        if (expr is not (Expr.Binary or Expr.Grouping or Expr.Logical))
        {
            throw new QueryEngineException(Error(token, "Expect binary, grouping or logical expression."));
        }
    }

    private static void CheckBinaryLeft(Expr expr, Token token)
    {
        if (expr is not Expr.Variable)
        {
            throw new QueryEngineException(Error(token, "Expect identifier."));
        }
    }

    private static void CheckBinaryRight(Expr expr, Token op, Token token)
    {
        if (expr is not Expr.Literal l)
        {
            throw new QueryEngineException(Error(token, "Expect literal."));
        }
        if (op.Type is TokenType.Like or TokenType.NotLike && l.Value is not string)
        {
            throw new QueryEngineException(Error(token, "Expect string literal."));
        }
        if (op.Type is TokenType.GreaterThan or TokenType.GreaterThanOrEqual or TokenType.LessThan or TokenType.LessThanOrEqual && l.Value is not decimal)
        {
            throw new QueryEngineException(Error(token, "Expect numeric literal."));
        }
    }

    private static string Error(Token token, string message) => token.Type == TokenType.Eol
        ? $"Error at position '{token.Position + 1}': {message}"
        : $"Error at position '{token.Position + 1}' -> {token.Text}: {message}";
}

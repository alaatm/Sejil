// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

namespace Sejil.Data.Query.Internal;

internal abstract class Expr
{
    public abstract void Accept(IVisitor visitor);

    public interface IVisitor
    {
        void Visit(Binary expr);
        void Visit(Grouping expr);
        void Visit(Literal expr);
        void Visit(Logical expr);
        void Visit(Variable expr);
    }

    public sealed class Binary : Expr
    {
        public Expr Left { get; }
        public Token Operator { get; }
        public Expr Right { get; }

        public Binary(Expr left, Token @operator, Expr right)
            => (Left, Operator, Right) = (left, @operator, right);

        public override void Accept(IVisitor visitor) => visitor.Visit(this);
    }

    public sealed class Grouping : Expr
    {
        public Expr Expression { get; }

        public Grouping(Expr expression) => Expression = expression;

        public override void Accept(IVisitor visitor) => visitor.Visit(this);
    }

    public sealed class Literal : Expr
    {
        public object Value { get; }

        public Literal(object value) : base() => Value = value;

        public override void Accept(IVisitor visitor) => visitor.Visit(this);
    }

    public sealed class Logical : Expr
    {
        public Expr Left { get; }
        public Token Operator { get; }
        public Expr Right { get; }

        public Logical(Expr left, Token @operator, Expr right)
            => (Left, Operator, Right) = (left, @operator, right);

        public override void Accept(IVisitor visitor) => visitor.Visit(this);
    }

    public sealed class Variable : Expr
    {
        public Token Token { get; }

        public Variable(Token token) => Token = token;

        public override void Accept(IVisitor visitor) => visitor.Visit(this);
    }
}

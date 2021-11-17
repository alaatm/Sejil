// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using System.Text;

namespace Sejil.Data.Query.Internal;

internal sealed class CodeGenerator : Expr.IVisitor
{
    private readonly StringBuilder _sql = new();

    private string _template;
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

    public void Visit(Expr.Grouping expr)
    {
        CheckOpenPropertyScope(expr);

        _sql.Append('(');
        Resolve(expr.Expression);
        _sql.Append(')');
    }

    public void Visit(Expr.Logical expr)
    {
        Resolve(expr.Left);
        CheckClosePropertyScope(expr.Right);
        _sql.Append($" {expr.Operator.Text.ToUpperInvariant()} ");
        Resolve(expr.Right);
    }

    public void Visit(Expr.Binary expr)
    {
        SetSqlConditionTemplate(expr);
        CheckOpenPropertyScope(expr.Left);

        Resolve(expr.Left);
        _template = _template.Replace("|OP|", expr.Operator.Negate());
        Resolve(expr.Right);

        _sql.Append(_template);
    }

    public void Visit(Expr.Variable expr) => _template = _template.Replace("|PNAME|", expr.Token.Text);

    public void Visit(Expr.Literal expr) => _template = _template.Replace("|PVAL|", expr.Value.ToString());

    private void CheckOpenPropertyScope(Expr expr)
    {
        var isProp = expr.HasAllProperty();

        if (isProp && !_insidePropBlock)
        {
            _sql.Append("id IN (SELECT logId FROM log_property GROUP BY logId HAVING ");
            _insidePropBlock = true;
        }
    }

    private void CheckClosePropertyScope(Expr expr)
    {
        if (_insidePropBlock && !expr.HasAllProperty())
        {
            _sql.Append(')');
            _insidePropBlock = false;
        }
    }

    private void SetSqlConditionTemplate(Expr.Binary expr)
    {
        var valueCol = ((Expr.Literal)expr.Right).Value switch
        {
            decimal => "CAST(value AS NUMERIC)",
            _ => "value",
        };

        _template = expr.HasAllProperty()
            ? expr.Operator.IsExluding()
                ? $"SUM(name = '|PNAME|' AND {valueCol} |OP| |PVAL|) = 0"
                : $"SUM(name = '|PNAME|' AND {valueCol} |OP| |PVAL|) > 0"
            : "|PNAME| |OP| |PVAL|";
    }
}

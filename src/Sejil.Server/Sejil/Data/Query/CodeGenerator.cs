// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using System.Globalization;
using System.Text;

namespace Sejil.Data.Query;

public abstract class CodeGenerator : Expr.IVisitor
{
    private readonly StringBuilder _sql = new();

    private string _template = string.Empty;
    private bool _insidePropBlock;

    protected abstract string LogPropertyTableName { get; }
    protected abstract string NumericCastSql { get; }
    protected abstract string PropertyFilterNegateSql { get; }
    protected abstract string PropertyFilterSql { get; }

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
        _sql.AppendFormat(CultureInfo.InvariantCulture, " {0} ", expr.Operator.Text.ToUpperInvariant());
        Resolve(expr.Right);
    }

    public void Visit(Expr.Binary expr)
    {
        SetSqlConditionTemplate(expr);
        CheckOpenPropertyScope(expr.Left);

        Resolve(expr.Left);
        _template = _template.Replace("|OP|", expr.Operator.Negate(), StringComparison.Ordinal);
        Resolve(expr.Right);

        _sql.Append(_template);
    }

    public void Visit(Expr.Variable expr)
        => _template = _template.Replace("|PNAME|", expr.Token.Text, StringComparison.Ordinal);

    public void Visit(Expr.Literal expr)
        => _template = _template.Replace("|PVAL|", expr.Value.ToString(), StringComparison.Ordinal);

    private void CheckOpenPropertyScope(Expr expr)
    {
        var isProp = expr.HasAllProperty();

        if (isProp && !_insidePropBlock)
        {
            _sql.AppendFormat(
                CultureInfo.InvariantCulture,
                "id IN (SELECT logId FROM {0} GROUP BY logId HAVING ",
                LogPropertyTableName);
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
            decimal => NumericCastSql,
            _ => "value",
        };

        if (expr.HasAllProperty())
        {
            _template = expr.Operator.IsExluding()
                ? PropertyFilterNegateSql
                : PropertyFilterSql;
            _template = _template.Replace("|VALCOL|", valueCol, StringComparison.Ordinal);
        }
        else
        {
            _template = "|PNAME| |OP| |PVAL|";
        }
    }
}

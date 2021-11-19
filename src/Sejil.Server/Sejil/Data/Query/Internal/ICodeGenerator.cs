namespace Sejil.Data.Query.Internal;

internal interface ICodeGenerator
{
    string Generate(Expr expr);
}

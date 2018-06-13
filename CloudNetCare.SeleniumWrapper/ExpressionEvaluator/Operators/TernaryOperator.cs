using System;
using System.Linq.Expressions;

namespace CloudNetCare.SeleniumWrapper.ExpressionEvaluator.Operators
{
    internal class TernaryOperator : Operator<Func<Expression, Expression, Expression, Expression>>
    {
        public TernaryOperator(string value, int precedence, bool leftassoc,
            Func<Expression, Expression, Expression, Expression> func)
            : base(value, precedence, leftassoc, func)
        {
            Arguments = 3;
        }
    }
}
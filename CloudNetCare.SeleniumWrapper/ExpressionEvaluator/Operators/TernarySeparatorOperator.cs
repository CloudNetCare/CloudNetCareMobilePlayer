using System;
using System.Linq.Expressions;

namespace CloudNetCare.SeleniumWrapper.ExpressionEvaluator.Operators
{
    internal class TernarySeparatorOperator : Operator<Func<Expression, Expression>>
    {
        public TernarySeparatorOperator(string value, int precedence, bool leftassoc,
            Func<Expression, Expression> func)
            : base(value, precedence, leftassoc, func)
        {
        }
    }
}
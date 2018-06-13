﻿using System.Linq.Expressions;

namespace CloudNetCare.SeleniumWrapper.ExpressionEvaluator.Operators
{
    internal abstract class Operator<T> : IOperator
    {
        public T Func { get; set; }
        public string Value { get; set; }
        public int Precedence { get; set; }
        public int Arguments { get; set; }
        public bool LeftAssoc { get; set; }
        public ExpressionType ExpressionType { get; set; }

        protected Operator(string value, int precedence, bool leftassoc, T func)
        {
            this.Value = value;
            this.Precedence = precedence;
            this.LeftAssoc = leftassoc;
            this.Func = func;
        }

        protected Operator(string value, int precedence, bool leftassoc, T func, ExpressionType expressionType)
        {
            this.Value = value;
            this.Precedence = precedence;
            this.LeftAssoc = leftassoc;
            this.Func = func;
            this.ExpressionType = expressionType;
        }

        public virtual T GetFunc()
        {
            return Func;
        }

    }
}

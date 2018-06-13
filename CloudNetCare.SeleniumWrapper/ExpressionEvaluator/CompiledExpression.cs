using System;
using System.Linq.Expressions;

namespace CloudNetCare.SeleniumWrapper.ExpressionEvaluator
{
    /// <summary>
    /// Creates compiled expressions with return values that are cast to type Object 
    /// </summary>
    public class CompiledExpression : ExpressionCompiler
    {
        private Func<object> _compiledMethod = null;
        private Action _compiledAction = null;

        public CompiledExpression()
        {
            Parser = new Parser();
            Parser.TypeRegistry = TypeRegistry;

        }

        /// <summary>
        /// Compiles the expression to a function that returns an object
        /// </summary>
        /// <returns></returns>
        public Func<object> Compile()
        {
            if (Expression == null) Expression = WrapExpression(BuildTree());
            return Expression.Lambda<Func<object>>(Expression).Compile();
        }

        protected override void ClearCompiledMethod()
        {
            _compiledMethod = null;
            _compiledAction = null;
        }

        public object Eval()
        {
            if (_compiledMethod == null) _compiledMethod = Compile();
            return _compiledMethod();
        }
    }
}

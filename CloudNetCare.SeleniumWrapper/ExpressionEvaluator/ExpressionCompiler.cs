using System.Linq.Expressions;

namespace CloudNetCare.SeleniumWrapper.ExpressionEvaluator
{
    public abstract class ExpressionCompiler
    {
        protected Expression Expression = null;
        protected Parser Parser = null;
        protected TypeRegistry TypeRegistry = new TypeRegistry();
        protected string Pstr = null;

        public string StringToParse
        {
            get { return Parser.StringToParse; }
            set
            {
                Parser.StringToParse = value;
                Expression = null;
                ClearCompiledMethod();
            }
        }

        protected abstract void ClearCompiledMethod();

        protected Expression BuildTree(Expression scopeParam = null, bool isCall = false)
        {
            return Parser.BuildTree(scopeParam, isCall);
        }


        protected Expression WrapExpression(Expression source, bool castToObject = true)
        {
            if (source.Type != typeof(void) && castToObject)
            {
                return Expression.Convert(source, typeof(object));
            }
            return Expression.Block(source, Expression.Constant(null));
        }
    }
}
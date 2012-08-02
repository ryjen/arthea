using System;
using System.Reflection;
using System.Linq.Expressions;

namespace ArtheaEngine
{

    public static class PropertyHelper<T>
    {
        #region Methods

        public static PropertyInfo GetProperty<TValue>(
            Expression<Func<T, TValue>> selector)
        {
            Expression body = selector;
            if (body is LambdaExpression)
            {
                body = ((LambdaExpression)body).Body;
            }
            switch (body.NodeType)
            {
                case ExpressionType.MemberAccess:
                    return (PropertyInfo)((MemberExpression)body).Member;
                default:
                    throw new InvalidOperationException();
            }
        }

        #endregion Methods
    }
}


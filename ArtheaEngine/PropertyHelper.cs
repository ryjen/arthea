using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ArtheaEngine
{

    /// <summary>
    /// Property helper.
    /// </summary>
    public static class PropertyHelper<T>
    {
        #region Methods

        /// <summary>
        /// Gets the property.
        /// </summary>
        /// <returns>
        /// The property.
        /// </returns>
        /// <param name='selector'>
        /// Selector.
        /// </param>
        /// <typeparam name='TValue'>
        /// The 1st type parameter.
        /// </typeparam>
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
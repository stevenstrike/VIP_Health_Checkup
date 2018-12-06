﻿using System;
using System.Linq.Expressions;

namespace VS2013_Extensions
{
    public static class NameOf
    {
        public static String nameof<T, TT>(this Expression<Func<T, TT>> accessor)
        {
            return nameof(accessor.Body);
        }

        public static String nameof<T>(this Expression<Func<T>> accessor)
        {
            return nameof(accessor.Body);
        }

        public static String nameof<T, TT>(this T obj, Expression<Func<T, TT>> propertyAccessor)
        {
            return nameof(propertyAccessor.Body);
        }

        private static String nameof(Expression expression)
        {
            if (expression.NodeType == ExpressionType.MemberAccess)
            {
                var memberExpression = expression as MemberExpression;
                if (memberExpression == null)
                    return null;
                return memberExpression.Member.Name;
            }
            return null;
        }
    }
}

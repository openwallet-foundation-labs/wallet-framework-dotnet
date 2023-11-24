using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Hyperledger.Aries.Tests.Extensions
{
    public static class ObjectExtensions
    {
        public static void PrivateSet<T, TProperty>(this T member, Expression<Func<T, TProperty>> property, TProperty value)
        {
            var name = ((MemberExpression)property.Body).Member.Name;
        
            PropertyInfo propertyInfo = typeof(T).GetProperty(name);
            if (propertyInfo == null) return;
            propertyInfo.SetValue(member, value);
        }
    }
}

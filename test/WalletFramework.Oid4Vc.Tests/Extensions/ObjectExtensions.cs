using System.Linq.Expressions;

namespace WalletFramework.Oid4Vc.Tests.Extensions;

public static class ObjectExtensions
{
    public static void PrivateSet<T, TProperty>(this T member, Expression<Func<T, TProperty>> property, TProperty value)
    {
        var name = ((MemberExpression)property.Body).Member.Name;
        
        var propertyInfo = typeof(T).GetProperty(name);
        if (propertyInfo == null) return;
        propertyInfo.SetValue(member, value);
    }
}

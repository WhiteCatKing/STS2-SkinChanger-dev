using System;
using System.Linq.Expressions;
using System.Reflection;

namespace STS2SkinChanger.Core;

internal static class ScopedMonsterRoutePolicy
{
	public static Func<object, string?> CreateMonsterIdAccessor(Type profileType)
	{
		PropertyInfo property = profileType.GetProperty("Target", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		PropertyInfo propertyInfo = property?.PropertyType.GetProperty("MonsterId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (property == null || propertyInfo?.PropertyType != typeof(string))
		{
			return (object _) => (string?)null;
		}
		ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "profile");
		MemberExpression memberExpression = Expression.Property(Expression.Convert(parameterExpression, profileType), property);
		Expression expression = Expression.Property(memberExpression, propertyInfo);
		if (!property.PropertyType.IsValueType || Nullable.GetUnderlyingType(property.PropertyType) != null)
		{
			expression = Expression.Condition(Expression.Equal(memberExpression, Expression.Constant(null, property.PropertyType)), Expression.Constant(null, typeof(string)), expression);
		}
		return Expression.Lambda<Func<object, string>>(expression, new ParameterExpression[1] { parameterExpression }).Compile();
	}
}

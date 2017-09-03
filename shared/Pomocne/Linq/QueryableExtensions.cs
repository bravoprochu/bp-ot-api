using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace bp.Pomocne.Linq
{
    public static class QueryableExtensions
    {
        public static IQueryable<TElement> WhereIf<TElement, TValue>(this IQueryable<TElement> source, Expression<Func<TElement, TValue>> propertySelector, bool condition)
        {
            return source.Where(GetWhereIfExpression(propertySelector, condition));
        }

        public static IQueryable<TElement> WhereIn<TElement, TValue>(this IQueryable<TElement> source, Expression<Func<TElement, TValue>> propertySelector, params TValue[] values)
        {
            return source.Where(GetWhereInExpression(propertySelector, values));
        }

        public static IQueryable<TElement> WhereIn<TElement, TValue>(this IQueryable<TElement> source, Expression<Func<TElement, TValue>> propertySelector, IEnumerable<TValue> values)
        {
            return source.Where(GetWhereInExpression(propertySelector, values));
        }

        public static IQueryable<TElement> WhereNotIn<TElement, TValue>(this IQueryable<TElement> source, Expression<Func<TElement, TValue>> propertySelector, IEnumerable<TValue> values)
        {
            return source.Where(GetWhereNotInExpression(propertySelector, values));
        }


        private static Expression<Func<TElement, bool>> GetWhereIfExpression<TElement, TValue>(Expression<Func<TElement, TValue>> propertySelector, bool condition)
        {
            ParameterExpression p = propertySelector.Parameters.Single();

            if (!condition)
            {
                return e => true;
            }

            return Expression.Lambda<Func<TElement, bool>>(propertySelector.Body, p);
        }

        private static Expression<Func<TElement, bool>> GetWhereInExpression<TElement, TValue>(Expression<Func<TElement, TValue>> propertySelector, IEnumerable<TValue> values)
        {
            ParameterExpression p = propertySelector.Parameters.Single();

            if (!values.Any())
            {
                return e => false;
            }

            var equals = values.Select(value => (Expression)Expression.Equal(propertySelector.Body, Expression.Constant(value, typeof(TValue))));
            var body = equals.Aggregate<Expression>((accumulate, equal) => Expression.Or(accumulate, equal));

            return Expression.Lambda<Func<TElement, bool>>(body, p);
        }

        private static Expression<Func<TElement, bool>> GetWhereNotInExpression<TElement, TValue>(Expression<Func<TElement, TValue>> propertySelector, IEnumerable<TValue> values)
        {
            ParameterExpression p = propertySelector.Parameters.Single();

            if (!values.Any())
            {
                return e => true;
            }

            var equals = values.Select(value => (Expression)Expression.NotEqual(propertySelector.Body, Expression.Constant(value, typeof(TValue))));
            var body = equals.Aggregate<Expression>((accumulate, notEqual) => Expression.And(accumulate, notEqual));

            return Expression.Lambda<Func<TElement, bool>>(body, p);
        }
    }
}
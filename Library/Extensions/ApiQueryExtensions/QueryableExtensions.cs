using DataModelLibrary.Models;
using DataModelLibrary.QueryParameters;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace LibraryApi.Extensions.ApiQueryExtensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return query;

            var parameter = Expression.Parameter(typeof(T), "x");
            var property = typeof(T).GetProperty(sortBy,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (property == null)
                return query;

            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExpression = Expression.Lambda(propertyAccess, parameter);

            var methodName = descending ? "OrderByDescending" : "OrderBy";
            var resultExpression = Expression.Call(
                typeof(Queryable),
                methodName,
                new[] { typeof(T), property.PropertyType },
                query.Expression,
                Expression.Quote(orderByExpression));

            return query.Provider.CreateQuery<T>(resultExpression);
        }

        public static IQueryable<Book> ApplyFiltering(this IQueryable<Book> query, BookQueryParameters parameters)
        {
            if (parameters.AvailableOnly)
            {
                query = query.Where(b => b.QuantityInStock > 0);
            }

            if (!string.IsNullOrWhiteSpace(parameters.Author))
            {
                query = query.Where(b => b.Author.Contains(parameters.Author));
            }

            if (parameters.YearFrom.HasValue)
            {
                query = query.Where(b => b.PublishedYear >= parameters.YearFrom.Value);
            }

            if (parameters.YearTo.HasValue)
            {
                query = query.Where(b => b.PublishedYear <= parameters.YearTo.Value);
            }

            if (parameters.PriceFrom.HasValue)
            {
                query = query.Where(b => b.Price >= parameters.PriceFrom.Value);
            }

            if (parameters.PriceTo.HasValue)
            {
                query = query.Where(b => b.Price <= parameters.PriceTo.Value);
            }

            if (parameters.GenreIds != null && parameters.GenreIds.Any())
            {
                query = query.Where(book =>
                    parameters.GenreIds.Contains(book.GenreId));
            }

            return query;
        }

    }
}

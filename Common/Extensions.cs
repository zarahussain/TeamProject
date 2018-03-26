using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks
{
  public static class Extensions
  {
    // To Pascal Case
    public static string ToPascal(this string prop)
    {
      if (string.IsNullOrEmpty(prop))
        return prop;
      return prop.Substring(0, 1).ToUpper() + prop.Substring(1);
    }
    public static string ToCamel(this string prop)
    {
      if (string.IsNullOrEmpty(prop))
        return prop;
      return prop.Substring(0, 1).ToLower() + prop.Substring(1);
    }
    // Set the Entity State
    public static void SetState<T>(this AdventureWorksContext context, T entity, string _state) where T : class
    {
      EntityState state;
      switch (_state)
      {
        case "Added":
          state = EntityState.Added;
          break;
        case "Modified":
          state = EntityState.Modified;
          break;
        case "Deleted":
          state = EntityState.Deleted;
          break;
        default:
          state = EntityState.Unchanged;
          break;
      }
      context.Entry(entity).State = state;
    }
    // failed to handle async and sotring
    public static IQueryable<T> Sort<T>(this IQueryable<T> query, string sort)
    {
      // hold the sorting expression
      Expression<Func<T, object>> expression;
      // split comma separated list into string array
      string[] items = sort.Split(',', StringSplitOptions.RemoveEmptyEntries);
      // hold the sorting pair string array
      string[] pair = new string[] { };
      // hold the sorting direction
      string dir = null;
      // hold the sorting property
      string prop = null;

      // lets build the ordered expression from sort array
      for (int i = 0; i < items.Length; i++)
      {
        pair = items[i].Trim().Split(' ');
        if (pair.Length > 2)
          throw new ArgumentException(String.Format("Invalid OrderBy string '{0}'. Order By Format: Property, Property2 asc, Property2 desc", items[i]));

        prop = pair[0].Trim();
        if (String.IsNullOrEmpty(prop))
          throw new ArgumentException("Invalid Property. Order By Format: Property, Property2 asc, Property3 desc");

        if (pair.Length == 2)
          dir = pair[1].Trim();

        if (String.IsNullOrEmpty(dir))
          dir = "asc";

        // Let us sort it
        expression = item => typeof(T)
                        .GetProperty(prop, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
                        .GetValue(item);

        if (i == 0)
          query = (dir == "asc")
          ? query.OrderBy(expression)
          : query.OrderByDescending(expression);
        else
          query = (dir == "asc")
          ? (query as IOrderedQueryable<T>).ThenBy(expression)
          : (query as IOrderedQueryable<T>).ThenByDescending(expression);
      }

      return query;
    }
  }
}
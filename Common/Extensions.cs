using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.Parsing.Structure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace AdventureWorks
{
  public static class Extensions
  {
    // ToSql to get the generated sql from linq query
    #region ToSql Linq Method
    private static readonly TypeInfo QueryCompilerTypeInfo = typeof(QueryCompiler).GetTypeInfo();
    private static readonly FieldInfo QueryCompilerField = typeof(EntityQueryProvider).GetTypeInfo().DeclaredFields.First(x => x.Name == "_queryCompiler");
    private static readonly PropertyInfo NodeTypeProviderField = QueryCompilerTypeInfo.DeclaredProperties.Single(x => x.Name == "NodeTypeProvider");
    private static readonly MethodInfo CreateQueryParserMethod = QueryCompilerTypeInfo.DeclaredMethods.First(x => x.Name == "CreateQueryParser");
    private static readonly FieldInfo DataBaseField = QueryCompilerTypeInfo.DeclaredFields.Single(x => x.Name == "_database");
    private static readonly PropertyInfo DatabaseDependenciesField = typeof(Database).GetTypeInfo().DeclaredProperties.Single(x => x.Name == "Dependencies");
    public static string ToSql<TEntity>(this IQueryable<TEntity> query) where TEntity : class
    {
      if (!(query is EntityQueryable<TEntity>) && !(query is InternalDbSet<TEntity>))
        throw new ArgumentException("Invalid query");
      var queryCompiler = (QueryCompiler)QueryCompilerField.GetValue(query.Provider);
      var nodeTypeProvider = (INodeTypeProvider)NodeTypeProviderField.GetValue(queryCompiler);
      var parser = (IQueryParser)CreateQueryParserMethod.Invoke(queryCompiler, new object[] { nodeTypeProvider });
      var queryModel = parser.GetParsedQuery(query.Expression);
      var database = DataBaseField.GetValue(queryCompiler);
      var databaseDependencies = (DatabaseDependencies)DatabaseDependenciesField.GetValue(database);
      var queryCompilationContext = databaseDependencies.QueryCompilationContextFactory.Create(false);
      var modelVisitor = (RelationalQueryModelVisitor)queryCompilationContext.CreateQueryModelVisitor();
      modelVisitor.CreateQueryExecutor<TEntity>(queryModel);
      return modelVisitor.Queries.First().ToString();
    }
    #endregion
    // takes a string that contains list of values seperated by a separator,
    // removes any empty elements (white spaces or empty strings)
    // return the clean string
    public static string RemoveEmptyValues(this string str, Char separator)
    {
      // convert comma separated list to array so that we can remove empty items
      string[] strArr = str.Split(separator, StringSplitOptions.RemoveEmptyEntries);
      //Remove empty items from array using where() [filter]
      //and trim each element using select() [map]
      strArr = strArr.Where(item => !string.IsNullOrWhiteSpace(item))
                      .Select(item => item.Trim())
                      .ToArray();
      //convert fieldsArr array to a string with separator
      return string.Join(separator, strArr);
    }
    // takes a string that contains list of values seperated by a separator,
    // removes any empty elements (white spaces or empty strings)
    // return the clean string array
    public static string[] SplitAndRemoveEmpty(this string str, Char separator)
    {
      // convert string to array so that we can remove empty items
      string[] strArr = str.Split(separator, StringSplitOptions.RemoveEmptyEntries);
      // Remove empty items from array using where()
      // and trim each element using select()
      // then return it
      return strArr.Where(item => !string.IsNullOrWhiteSpace(item))
                      .Select(item => item.Trim())
                      .ToArray();
    }
    // To Pascal Case
    public static string ToPascal(this string prop)
    {
      if (string.IsNullOrEmpty(prop))
        return prop;
      return prop.Substring(0, 1).ToUpper() + prop.Substring(1);
    }
    // To Camel Case
    public static string ToCamel(this string prop)
    {
      if (string.IsNullOrEmpty(prop))
        return prop;
      return prop.Substring(0, 1).ToLower() + prop.Substring(1);
    }

    // an extension method attached to object [it can be used within any object]
    // takes a property name which we want to get its PropertyInfo useing reflection
    public static PropertyInfo GetProperty(this object obj, string propName)
    {
      return obj.GetType()
              .GetProperty(propName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
    }
    // an extension method attached to object [it can be used within any object]
    // takes a property name which we want to get its value
    // so we use reflection to get the property value based on its name at runtime
    public static object GetValue(this object obj, string propName)
    {
      // get the property info
      var prop = obj.GetProperty(propName.Trim());
      // if there is no such property on that type then return null
      if (prop == null)
        return null;
      // else return the value
      return prop.GetValue(obj);
    }
    // an extension method attached to object [it can be used within any object]
    // takes a [property name] which we want to sets its value and a [value]
    // so we use reflection to [set] the property value based on its [name] and [type] at runtime
    public static void SetValue(this object obj, string propName, object value)
    {
      // get the property info
      var prop = obj.GetProperty(propName);
      // if there is no such property on that type then do nothing
      if (prop == null)
        return;
      // else do the following
      // check if the type is nullable
      // if so return its underlying type
      // if not, return its type
      var propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
      // get the TypeCode enum from the property type
      TypeCode typeCode = System.Type.GetTypeCode(propertyType);
      // switch on typeCode and Set the Value
      switch (typeCode)
      {
        case TypeCode.Boolean:
          prop.SetValue(obj, Convert.ToBoolean(value), null);
          break;
        case TypeCode.String:
          prop.SetValue(obj, Convert.ToString(value), null);
          break;
        case TypeCode.Byte:
          prop.SetValue(obj, Convert.ToByte(value), null);
          break;
        case TypeCode.SByte:
          prop.SetValue(obj, Convert.ToSByte(value), null);
          break;
        case TypeCode.UInt16:
          prop.SetValue(obj, Convert.ToUInt16(value), null);
          break;
        case TypeCode.UInt32:
          prop.SetValue(obj, Convert.ToUInt32(value), null);
          break;
        case TypeCode.UInt64:
          prop.SetValue(obj, Convert.ToUInt64(value), null);
          break;
        case TypeCode.Int16:
          prop.SetValue(obj, Convert.ToInt16(value), null);
          break;
        case TypeCode.Int32:
          prop.SetValue(obj, Convert.ToInt32(value), null);
          break;
        case TypeCode.Int64:
          prop.SetValue(obj, Convert.ToInt64(value), null);
          break;
        case TypeCode.Single:
          prop.SetValue(obj, Convert.ToSingle(value), null);
          break;
        case TypeCode.Double:
          prop.SetValue(obj, Convert.ToDouble(value), null);
          break;
        case TypeCode.Decimal:
          prop.SetValue(obj, Convert.ToDecimal(value), null);
          break;
        case TypeCode.DateTime:
          prop.SetValue(obj, Convert.ToDateTime(value), null);
          break;
        case TypeCode.Object:
          if (prop.PropertyType == typeof(Guid) || prop.PropertyType == typeof(Guid?))
          {
            prop.SetValue(obj, Guid.Parse(value.ToString()), null);
            return;
          }
          prop.SetValue(obj, value, null);
          break;
        default:
          prop.SetValue(obj, value, null);
          break;
      }
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
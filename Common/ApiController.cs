using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Threading.Tasks;

namespace AdventureWorks
{
  public class Query
  {
    public string SqlTableName { get; set; }
    public string KeyPropName { get; set; }
    public PropertyInfo KeyProp { get; set; }
    public List<string> FiltersFields { get; set; }
    public List<string> SearchFields { get; set; }
  }
  public class Response
  {
    public string Title { get; set; }
    public object Message { get; set; }
  }
  public class APIController<Tkey, TEntity, TDbContext> : Controller
          where Tkey : struct
          where TEntity : class
          where TDbContext : DbContext
  {
    private readonly TDbContext _db;
    private readonly Query _query;
    private IEnumerable<dynamic> _filters;
    private TEntity found;
    private PropertyInfo _modifiedDateProp;
    private string _sqlQuery;

    public APIController(TDbContext dbContext, Query query)
    {
      _db = dbContext;
      _query = query;
      _query.KeyProp = typeof(TEntity).GetProperty(_query.KeyPropName);
    }

    private bool _FieldExists(string FieldName) => _query.FiltersFields.Any(q => q == FieldName);

    private async Task<TEntity> _Find(Tkey id) => await _db.Set<TEntity>()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => _query.KeyProp.GetValue(e).Equals(id));
    private void _SetState(TEntity entity, string _state)
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
      _db.Entry(entity).State = state;
    }
    private async Task<IActionResult> _DoAction(TEntity entity, string actionType)
    {
      if (ModelState.IsValid)
      {
        if (actionType == "Modified")
        {
          _modifiedDateProp = typeof(TEntity).GetProperty("ModifiedDate");
          if (_modifiedDateProp != null)
            _modifiedDateProp.SetValue(entity, DateTime.Now);
        }
        _SetState(entity, actionType);
        try
        {
          await _db.SaveChangesAsync();
          switch (actionType)
          {
            case "Added":
              return Created($"api/Products/{_query.KeyProp.GetValue(entity)}", entity);
            case "Modified":
              return Ok(entity);
            case "Deleted":
              return Ok(new Response
              {
                Title = "Success: Deleting",
                Message = $"Item with Id: {_query.KeyProp.GetValue(entity)} Was Deleted From DB"
              });
            default:
              return NoContent();
          }
        }
        catch (Exception ex)
        {
          return BadRequest(new Response
          {
            Title = "Error: SqlException",
            Message = ex.InnerException.Message
          });
        }
      }
      else
      {
        return BadRequest(new Response
        {
          Title = "Error: Invalid Data",
          Message = ModelState
        });
      }
    }

    // takes array of filters (each : field|operator|value)
    // and generates the conditions of the sql where clause
    // build one condition
    private string _GetCondition(string[] filter, string prefix)
    {
      string _field = filter[0].Trim();
      string _operator = filter[1].Trim();
      string _value = filter[2].Trim();

      var prop = typeof(TEntity).GetProperty(_field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
      //if field not found or null , return emtpy string
      if (prop == null)
        return "";
      var propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
      TypeCode typeCode = System.Type.GetTypeCode(propertyType);
      switch (typeCode)
      {
        case TypeCode.Boolean:
          switch (_operator)
          {
            case "=":
              return $"{prefix} {_field} = {_value} ";
            case "!=":
              return $"{prefix} {_field} != {_value} ";
            default:
              return "";
          }
        case TypeCode.String:
          switch (_operator)
          {
            case "=":
              return $"{prefix} {_field} = '{_value}' ";
            case "!=":
              return $"{prefix} {_field} != '{_value}' ";
            case "%":
              return $"{prefix} {_field} like '%{_value}%' ";
            case "@":
              return $"{prefix} CONTAINS({_field} , '{_value}') ";
            default:
              return "";
          }
        case TypeCode.Byte:
        case TypeCode.SByte:
        case TypeCode.UInt16:
        case TypeCode.UInt32:
        case TypeCode.UInt64:
        case TypeCode.Int16:
        case TypeCode.Int32:
        case TypeCode.Int64:
        case TypeCode.Single:
        case TypeCode.Double:
        case TypeCode.Decimal:
          switch (_operator)
          {
            case "=":
              return $"{prefix} {_field} = {_value} ";
            case "!=":
              return $"{prefix} {_field} != {_value} ";
            case ">":
              return $"{prefix} {_field} > {_value} ";
            case "<":
              return $"{prefix} {_field} < {_value} ";
            case ">=":
              return $"{prefix} {_field} >= {_value} ";
            case "<=":
              return $"{prefix} {_field} <= {_value} ";
            default:
              return "";
          }
        case TypeCode.DateTime:
          switch (_operator)
          {
            case "=":
              return $"{prefix} {_field} = '{_value}' ";
            case "!=":
              return $"{prefix} {_field} != '{_value}' ";
            case ">":
              return $"{prefix} {_field} > '{_value}' ";
            case "<":
              return $"{prefix} {_field} < '{_value}' ";
            case ">=":
              return $"{prefix} {_field} >= '{_value}' ";
            case "<=":
              return $"{prefix} {_field} <= '{_value}' ";
            default:
              return "";
          }
        default:
          return "";
      }
    }
    // takes string filters comma separated values (each : field|operator|value)
    // clean it and get string list of filters
    // bass it to _GetCondition one by one
    // so it builds the whole sql where clause
    public IQueryable<TEntity> _ApplySqlWhere(string filters, string tableName)
    {
      //split filters and add every filter as an item in an array
      string[] filtersArr = filters.SplitAndRemoveEmpty(',');
      //list to hold every filter as an arry with 3 item
      List<string[]> filtersList = filtersArr.Select(item => item.SplitAndRemoveEmpty('|'))
                                             .ToList();

      _sqlQuery = $"select * from {tableName} ";
      foreach (var filter in filtersList)
        _sqlQuery += (filtersList.First() == filter)
                        ? _GetCondition(filter, "where")
                        : _GetCondition(filter, "and");
      //use no tracking so that db context won't track changes on this dbset
      //better performance than AsQueriable -- used in read only queries
      return _db.Set<TEntity>()
                .AsNoTracking()
                .FromSql(_sqlQuery);
    }

    // filtering only on a specific [string] field(s) using contains operator
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string query)
    {
      if (String.IsNullOrEmpty(query.Trim()))
        return BadRequest(new Response { Title = "Invalid search term", Message = "must specify query parameter ..." });
      try
      {
        // set the base select statement
        _sqlQuery = $"select * from {_query.SqlTableName}";
        // build the where clause
        foreach (var field in _query.SearchFields)
          _sqlQuery += (field == _query.SearchFields.First())
                        ? $" where {field} like '%{query}%'"
                        : $" or {field} like '%{query}%'";
        // execute the query
        var result = await _db.Set<TEntity>()
                           .AsNoTracking()
                           .FromSql(_sqlQuery)
                           .ToListAsync();
        // return the result
        return Ok(result);
      }
      catch (Exception ex)
      {
        return BadRequest(new Response { Title = ex.GetType().Name, Message = ex });
      }
    }
    // applying dynamic filter using sql where clause
    [HttpGet("where")]
    public IActionResult SqlWhere([FromQuery] string filters)
    {
      try
      {
        var res = _ApplySqlWhere(filters, _query.SqlTableName);
        return Ok(res);
      }
      catch (Exception ex)
      {
        return BadRequest(new Response { Title = ex.GetType().Name, Message = ex });
      }
    }

    // select only desired fields from SQL Server using Dynamic Linq
    [HttpGet("select")]
    public IActionResult Select([FromQuery] string fields)
    {
      try
      {
        var result = _db.Set<TEntity>().Select($"new({fields})");
        return Ok(result);
      }
      catch (Exception ex)
      {
        return BadRequest(new Response { Title = ex.GetType().Name, Message = ex });
      }
    }
    // Appling Sql like orderBy clause using Dynamic Linq
    [HttpGet("sort")]
    public async Task<IActionResult> Sort([FromQuery] string orderBy)
    {
      if (String.IsNullOrEmpty(orderBy.Trim()))
        return Ok(await _db.Set<TEntity>().ToListAsync());
      try
      {
        var result = await _db.Set<TEntity>()
                              .OrderBy(orderBy)
                              .ToListAsync();
        return Ok(result);
      }
      catch (Exception ex)
      {
        return BadRequest(new Response { Title = ex.GetType().Name, Message = ex.Message });
      }

    }

    [HttpGet("list")]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Set<TEntity>().AsNoTracking().ToListAsync());

    [HttpGet("page")]
    public IActionResult GetPage([FromQuery] Page page) =>
                Ok(new PagedList<TEntity>(_db.Set<TEntity>().AsNoTracking(), page.number, page.size));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] Tkey id)
    {
      found = await _Find(id);
      if (found == null)
        return NotFound();
      return Ok(found);
    }
    [HttpGet("find")]
    public async Task<IActionResult> Find([FromQuery] IDictionary<string, string> query)
    {
      // capitalize the first letter of the field
      _filters = query.Select(item => new
      {
        Field = item.Key.Substring(0, 1).ToString().ToUpper() + item.Key.Substring(1),
        Value = item.Value
      });
      // filter out any not querable fields
      _filters = _filters.Where(f => _FieldExists(f.Field));
      // if no remaining fields return badrequest
      if (_filters.Count() == 0)
        return BadRequest(new { Title = "Invalid QueryString Keys", Error = query });
      // set the base select statement
      _sqlQuery = $"select * from {_query.SqlTableName}";
      // build the where clause
      foreach (var item in _filters)
      {
        if (item.Field == _filters.First().Field)
          _sqlQuery += $" where {item.Field} = '{item.Value}'";
        else
          _sqlQuery += $" and {item.Field} = '{item.Value}'";
      }
      // execute the query
      var result = await _db.Set<TEntity>()
                         .AsNoTracking()
                         .FromSql(_sqlQuery)
                         .ToListAsync();
      // return the appropriate result
      if (result.Count == 0)
        return NotFound();
      return Ok(result);
    }
    [HttpPost("add")]
    public async Task<IActionResult> Add([FromBody] TEntity newProduct)
    {
      return await _DoAction(newProduct, "Added");
    }

    [HttpPut("update")]
    public async Task<IActionResult> Update([FromBody] TEntity updProduct)
    {
      return await _DoAction(updProduct, "Modified");
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> Delete([FromRoute] Tkey id)
    {
      found = await _Find(id);
      if (found == null)
        return NotFound();
      return await _DoAction(found, "Deleted");
    }

  } // end class APIController
} // namespace
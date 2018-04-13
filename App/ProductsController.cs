using System;
using System.Linq;
using System.Dynamic;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AdventureWorks
{
  public class Filter
  {
    public PropertyInfo Property { get; set; }
    public string Operator { get; set; }
    public object Value { get; set; }
  }

  [Route("api/[controller]")]
  public class ProductsController : Controller
  {
    private readonly AdventureWorksContext _db;
    public ProductsController(AdventureWorksContext context) => _db = context;
    private bool ProductExists(int id) => _db.Product.Any(e => e.ProductId == id);
    private async Task<Product> _Find(int id) => await _db.Product.FindAsync(id);
    private async Task<IActionResult> _DoAction(Product entity, string actionType)
    {
      if (ModelState.IsValid)
      {
        if (actionType == "Modified")
          entity.ModifiedDate = DateTime.Now;
        _db.SetState<Product>(entity, actionType);
        try
        {
          await _db.SaveChangesAsync();
          switch (actionType)
          {
            case "Added":
              return Created($"api/Products/{entity.ProductId}", entity);
            case "Modified":
              return Ok(entity);
            case "Deleted":
              return Ok(new { Message = $"Item with Id: {entity.ProductId} Was Deleted From DB" });
            default:
              return NoContent();
          }
        }
        catch (Exception ex)
        {
          return BadRequest(new { Title = "SqlException", Error = ex.InnerException.Message });
        }
      }
      else
      {
        return BadRequest(new { Title = "Invalid Data", Error = ModelState });
      }
    }
    private bool GetItem<T>(T item, Filter Filter)
    {
      Type PropType = Nullable.GetUnderlyingType(Filter.Property.PropertyType) ?? Filter.Property.PropertyType;
      Object PropValue = Filter.Property.GetValue(item);
      switch (Filter.Operator)
      {
        case "==":
          if (Type.GetTypeCode(PropType) == TypeCode.Byte)
            return Convert.ToByte(PropValue) == Convert.ToByte(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.SByte)
            return Convert.ToSByte(PropValue) == Convert.ToSByte(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.Int16)
            return Convert.ToInt16(PropValue) == Convert.ToInt16(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.UInt16)
            return Convert.ToUInt16(PropValue) == Convert.ToUInt16(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.Int32)
            return Convert.ToInt32(PropValue) == Convert.ToInt32(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.UInt32)
            return Convert.ToUInt32(PropValue) == Convert.ToUInt32(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.Int64)
            return Convert.ToInt64(PropValue) == Convert.ToInt64(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.UInt64)
            return Convert.ToUInt64(PropValue) == Convert.ToUInt64(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.Single)
            return Convert.ToSingle(PropValue) == Convert.ToSingle(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.Double)
            return Convert.ToDouble(PropValue) == Convert.ToDouble(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.Decimal)
            return Convert.ToDecimal(PropValue) == Convert.ToDecimal(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.DateTime)
            return Convert.ToDateTime(PropValue) == Convert.ToDateTime(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.String)
            return Convert.ToString(PropValue) == Convert.ToString(Filter.Value);
          else
            return false;
        case "!=":
          if (Type.GetTypeCode(PropType) == TypeCode.Byte)
            return Convert.ToByte(PropValue) != Convert.ToByte(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.SByte)
            return Convert.ToSByte(PropValue) != Convert.ToSByte(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.Int16)
            return Convert.ToInt16(PropValue) != Convert.ToInt16(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.UInt16)
            return Convert.ToUInt16(PropValue) != Convert.ToUInt16(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.Int32)
            return Convert.ToInt32(PropValue) != Convert.ToInt32(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.UInt32)
            return Convert.ToUInt32(PropValue) != Convert.ToUInt32(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.Int64)
            return Convert.ToInt64(PropValue) != Convert.ToInt64(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.UInt64)
            return Convert.ToUInt64(PropValue) != Convert.ToUInt64(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.Single)
            return Convert.ToSingle(PropValue) != Convert.ToSingle(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.Double)
            return Convert.ToDouble(PropValue) != Convert.ToDouble(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.Decimal)
            return Convert.ToDecimal(PropValue) != Convert.ToDecimal(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.DateTime)
            return Convert.ToDateTime(PropValue) != Convert.ToDateTime(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.String)
            return Convert.ToString(PropValue) != Convert.ToString(Filter.Value);
          else
            return false;
        case ">":
          if (Type.GetTypeCode(PropType) == TypeCode.Byte)
            return Convert.ToByte(PropValue) > Convert.ToByte(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.SByte)
            return Convert.ToSByte(PropValue) > Convert.ToSByte(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.Int16)
            return Convert.ToInt16(PropValue) > Convert.ToInt16(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.UInt16)
            return Convert.ToUInt16(PropValue) > Convert.ToUInt16(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.Int32)
            return Convert.ToInt32(PropValue) > Convert.ToInt32(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.UInt32)
            return Convert.ToUInt32(PropValue) > Convert.ToUInt32(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.Int64)
            return Convert.ToInt64(PropValue) > Convert.ToInt64(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.UInt64)
            return Convert.ToUInt64(PropValue) > Convert.ToUInt64(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.Single)
            return Convert.ToSingle(PropValue) > Convert.ToSingle(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.Double)
            return Convert.ToDouble(PropValue) > Convert.ToDouble(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.Decimal)
            return Convert.ToDecimal(PropValue) > Convert.ToDecimal(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.DateTime)
            return Convert.ToDateTime(PropValue) > Convert.ToDateTime(Filter.Value);
          else
            return false;
        case "<":
          if (Type.GetTypeCode(PropType) == TypeCode.Byte)
            return Convert.ToByte(PropValue) < Convert.ToByte(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.SByte)
            return Convert.ToSByte(PropValue) < Convert.ToSByte(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.Int16)
            return Convert.ToInt16(PropValue) < Convert.ToInt16(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.UInt16)
            return Convert.ToUInt16(PropValue) < Convert.ToUInt16(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.Int32)
            return Convert.ToInt32(PropValue) < Convert.ToInt32(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.UInt32)
            return Convert.ToUInt32(PropValue) < Convert.ToUInt32(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.Int64)
            return Convert.ToInt64(PropValue) < Convert.ToInt64(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.UInt64)
            return Convert.ToUInt64(PropValue) < Convert.ToUInt64(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.Single)
            return Convert.ToSingle(PropValue) < Convert.ToSingle(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.Double)
            return Convert.ToDouble(PropValue) < Convert.ToDouble(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.Decimal)
            return Convert.ToDecimal(PropValue) < Convert.ToDecimal(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.DateTime)
            return Convert.ToDateTime(PropValue) < Convert.ToDateTime(Filter.Value);
          else
            return false;
        case ">=":
          if (Type.GetTypeCode(PropType) == TypeCode.Byte)
            return Convert.ToByte(PropValue) >= Convert.ToByte(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.SByte)
            return Convert.ToSByte(PropValue) >= Convert.ToSByte(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.Int16)
            return Convert.ToInt16(PropValue) >= Convert.ToInt16(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.UInt16)
            return Convert.ToUInt16(PropValue) >= Convert.ToUInt16(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.Int32)
            return Convert.ToInt32(PropValue) >= Convert.ToInt32(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.UInt32)
            return Convert.ToUInt32(PropValue) >= Convert.ToUInt32(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.Int64)
            return Convert.ToInt64(PropValue) >= Convert.ToInt64(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.UInt64)
            return Convert.ToUInt64(PropValue) >= Convert.ToUInt64(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.Single)
            return Convert.ToSingle(PropValue) >= Convert.ToSingle(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.Double)
            return Convert.ToDouble(PropValue) >= Convert.ToDouble(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.Decimal)
            return Convert.ToDecimal(PropValue) >= Convert.ToDecimal(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.DateTime)
            return Convert.ToDateTime(PropValue) >= Convert.ToDateTime(Filter.Value);
          else
            return false;
        case "<=":
          if (Type.GetTypeCode(PropType) == TypeCode.Byte)
            return Convert.ToByte(PropValue) <= Convert.ToByte(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.SByte)
            return Convert.ToSByte(PropValue) <= Convert.ToSByte(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.Int16)
            return Convert.ToInt16(PropValue) <= Convert.ToInt16(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.UInt16)
            return Convert.ToUInt16(PropValue) <= Convert.ToUInt16(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.Int32)
            return Convert.ToInt32(PropValue) <= Convert.ToInt32(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.UInt32)
            return Convert.ToUInt32(PropValue) <= Convert.ToUInt32(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.Int64)
            return Convert.ToInt64(PropValue) <= Convert.ToInt64(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.UInt64)
            return Convert.ToUInt64(PropValue) <= Convert.ToUInt64(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.Single)
            return Convert.ToSingle(PropValue) <= Convert.ToSingle(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.Double)
            return Convert.ToDouble(PropValue) <= Convert.ToDouble(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.Decimal)
            return Convert.ToDecimal(PropValue) <= Convert.ToDecimal(Filter.Value);
          if (Type.GetTypeCode(PropType) == TypeCode.DateTime)
            return Convert.ToDateTime(PropValue) <= Convert.ToDateTime(Filter.Value);
          else
            return false;
        case "%":
          return Filter.Property.GetValue(item).ToString().Contains(Filter.Value.ToString());
        case "=w":
          return Regex.IsMatch(Filter.Property.GetValue(item).ToString(), string.Format(@"\b{0}\b", Regex.Escape(Filter.Value.ToString())));
        case "!w":
          return !Regex.IsMatch(Filter.Property.GetValue(item).ToString(), string.Format(@"\b{0}\b", Regex.Escape(Filter.Value.ToString())));
        default:
          return false;
      }
    }

    // GET: api/Products/find?
    [HttpGet("find")]
    public async Task<IActionResult> Find([FromQuery] IDictionary<string, string> query)
    {
      var filters = query.Select(item => new
      {
        Field = item.Key.Substring(0, 1).ToString().ToUpper() + item.Key.Substring(1),
        Value = item.Value
      });
      string sql = "select * from Production.Product";
      foreach (var item in filters)
      {
        if (item.Field == filters.First().Field)
          sql += $" where {item.Field} = '{item.Value}'";
        else
          sql += $" and {item.Field} = '{item.Value}'";
      }
      var result = await _db.Product.FromSql(sql).ToListAsync();
      if (result.Count > 0)
        return Ok(result);
      else
        return NotFound();
    }

    // GET: api/Products?
    [HttpGet("filter")]
    public async Task<IActionResult> Filter([FromQuery] IDictionary<string, string> qStr)
    {
      var filters = qStr.Select(item => new
      {
        Field = typeof(Product).GetProperty(item.Key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance),
        Value = item.Value
      });
      var query = _db.Product.AsNoTracking();
      foreach (var item in filters)
        query = query.Where(p => item.Field.GetValue(p).ToString() == item.Value);
      var result = await query.ToListAsync();
      if (result.Count > 0)
        return Ok(new { result = result, sql = query.ToSql() });
      else
        return NotFound();
    }

    [HttpGet("sort")]
    public async Task<IActionResult> Sort([FromQuery] string orderBy)
    {
      if (String.IsNullOrEmpty(orderBy))
        return Ok(await _db.Product.ToListAsync());
      try
      {
        var result = await _db.Product
                                .OrderBy(orderBy)
                                .ToListAsync();
        return Ok(result);
      }
      catch (Exception ex)
      {
        return BadRequest(new { Title = ex.GetType().Name, Message = ex.Message });
      }

    }

    [HttpGet("orderBy")]
    public async Task<IActionResult> OrderBy([FromQuery] string fields)
    {
      try
      {
        string[] _fields = fields.Split(',', StringSplitOptions.RemoveEmptyEntries);
        // hold the Order Dictionary
        List<KeyValuePair<string, PropertyInfo>> OrderList = new List<KeyValuePair<string, PropertyInfo>>();
        // hold the first Order
        KeyValuePair<string, PropertyInfo> OrderItem;
        // hold the sorting pair
        string[] pair;
        // hold the sorting direction
        string dir = null;
        // hold the sorting property
        string prop = null;

        foreach (var field in _fields)
        {
          pair = field.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
          if (pair.Length > 2)
            throw new ArgumentException(String.Format("Invalid OrderBy string: '{0}'. Order By Format: [Property] | [Property asc] | [Property desc]", field.Trim()));
          else
          {
            prop = pair[0].Trim();
            dir = "asc";
            if (pair.Length == 2)
              dir = pair[1].Trim();
          }
          OrderList.Add(new KeyValuePair<string, PropertyInfo>(dir, typeof(Product).GetProperty(prop, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)));
        }

        var query = _db.Product.AsNoTracking();
        OrderItem = OrderList.First();
        query = (OrderItem.Key == "asc")
                ? query.OrderBy(p => OrderItem.Value.GetValue(p))
                : query.OrderByDescending(p => OrderItem.Value.GetValue(p));

        int len = OrderList.Count;
        for (int i = 1; i < len; i++)
        {
          int index = i;
          query = (OrderList[index].Key == "asc")
                  ? (query as IOrderedQueryable<Product>).ThenBy(p => OrderList[index].Value.GetValue(p))
                  : (query as IOrderedQueryable<Product>).ThenByDescending(p => OrderList[index].Value.GetValue(p));
        }

        var Ordering = OrderList.Select(o => new { Prop = o.Value.Name, Order = o.Key });
        var Result = await query.ToListAsync();
        return Ok(new { Ordering, Result });
      }
      catch (Exception ex)
      {
        return BadRequest(new { Title = ex.GetType().Name, Message = ex.Message, Error = ex });
      }
    }
    // select all data from SQL Server and then shapping it dynamically using Reflection
    [HttpGet("shapping")]
    public IActionResult Shapping([FromQuery] string fields)
    {
      try
      {
        // get the list of fields
        string[] _fields = fields.Split(',', StringSplitOptions.RemoveEmptyEntries);
        // get the list of Props [using Reflection] based on list of fields
        PropertyInfo[] Props = _fields.Select(field => typeof(Product).GetProperty(field.Trim(), BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance))
                                      .ToArray();
        // hold one shapped object
        Dictionary<string, object> shappedObject;
        // get the list and map it to requested Props
        var result = _db.Product
                        .ToList()
                        .Select(p =>
                        {
                          shappedObject = new Dictionary<string, object>();
                          foreach (var prop in Props)
                            shappedObject.Add(prop.Name.ToCamel(), prop.GetValue(p));
                          return shappedObject;
                        });
        // returning the result
        return Ok(result);
      }
      catch (Exception ex)
      {
        return BadRequest(new { Title = ex.GetType().Name, Error = ex });
      }
    }
    // select only desired fields from SQL Server using Dynamic Linq
    [HttpGet("select")]
    public IActionResult Select([FromQuery] string fields)
    {
      try
      {
        // get the list and map it to requested Props
        var result = _db.Product.Select($"new({fields})");
        // returning the result
        return Ok(result);
      }
      catch (Exception ex)
      {
        return BadRequest(new { Title = ex.GetType().Name, Error = ex });
      }
    }

    // GET: api/Products?
    [HttpGet("where")]
    public async Task<IActionResult> Where([FromQuery] string filters)
    {
      try
      {
        string[] _filters = filters.Split(',', StringSplitOptions.RemoveEmptyEntries);
        // hold the filter 3 parts [Field - Opertor - Value]
        string[] parts;
        // hold the filter List Mapped to Filter Object
        List<Filter> Filters = _filters.Select(filter =>
          {
            parts = filter.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3)
              throw new ArgumentException(String.Format("Invalid Filters string: '{0}'. Filters Format: [Field Opertor Value]", filter.Trim()));
            return new Filter()
            {
              Property = typeof(Product).GetProperty(parts[0], BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance),
              Operator = parts[1],
              Value = parts[2]
            };
          }).ToList();

        var query = _db.Product.AsNoTracking();
        foreach (Filter filter in Filters)
          query = query.Where(p => GetItem<Product>(p, filter));

        var result = await query.ToListAsync();
        if (result.Count == 0)
          return NotFound();
        return Ok(result);
      }
      catch (Exception ex)
      {
        return BadRequest(new { Title = ex.GetType().Name, Message = ex.Message, Error = ex });
      }
    }

    [HttpGet("sql")]
    public async Task<IActionResult> Sql([FromQuery] string sql)
    {
      try
      {
        var result = await _db.Product
                                .FromSql(sql)
                                .ToListAsync();
        return Ok(result);
      }
      catch (Exception ex)
      {
        return BadRequest(new { Title = ex.GetType().Name, Message = ex.Message });
      }

    }

    // GET: api/Products/list
    [HttpGet("list")]
    public async Task<IActionResult> GetAll() => Ok(await _db.Product.ToListAsync());

    // GET: api/Products/page?number=1&size=10
    [HttpGet("page")]
    public IActionResult GetPaged([FromQuery] Page page) => Ok(new PagedList<Product>(_db.Product.AsQueryable(), page.number, page.size));

    // GET: api/Products/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
      var product = await _Find(id);
      if (product == null)
        return BadRequest("There is no Product with this Id");
      return Ok(product);
    }

    // POST: api/Products/add
    [HttpPost("add")]
    public async Task<IActionResult> Add([FromBody] Product newProduct)
    {
      return await _DoAction(newProduct, "Added");
    }

    // POST: api/Products/update
    [HttpPut("update")]
    public async Task<IActionResult> Update([FromBody] Product updProduct)
    {
      return await _DoAction(updProduct, "Modified");
    }

    // POST: api/Products/delete
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
      var product = await _Find(id);
      if (product == null)
        return BadRequest("There is no Product with this Id");
      return await _DoAction(product, "Deleted");
    }
  }
}

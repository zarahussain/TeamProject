using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Dynamic;

namespace AdventureWorks
{
  [Route("api/[controller]")]
  public class ProductsController : Controller
  {
    private readonly AdventureWorksContext _context;
    public ProductsController(AdventureWorksContext context) =>
        _context = context;
    private bool ProductExists(int id) =>
        _context.Product.Any(e => e.ProductId == id);
    private async Task<IActionResult> DoAction(Product entity, string actionType)
    {
      if (ModelState.IsValid)
      {
        if (actionType == "Modified")
          entity.ModifiedDate = DateTime.Now;
        _context.SetState<Product>(entity, actionType);
        try
        {
          await _context.SaveChangesAsync();
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

    [HttpGet("sort")]
    public async Task<IActionResult> Sort([FromQuery] string orderBy)
    {
      if (String.IsNullOrEmpty(orderBy))
        return Ok(await _context.Product.ToListAsync());
      try {
        var result = await _context.Product
                                .OrderBy(orderBy)
                                .ToListAsync();
        return Ok(result);
      }
      catch (Exception ex) {
        return BadRequest(new { Title = ex.GetType().Name, Message = ex.Message });
      }

    }

    [HttpGet("shapping")]
    public IActionResult shapping([FromQuery] string fields)
    {
      try {
        string[] _fields = fields.Split(',', StringSplitOptions.RemoveEmptyEntries);
        PropertyInfo[] Props = _fields.Select(field => typeof(Product).GetProperty(field.Trim(), BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) )
                                       .ToArray();

        IDictionary<string,object> shapped;
        List<IDictionary<string,object>> result = new List<IDictionary<string, object>>();

        foreach (var entity in _context.Product) {
          shapped = new ExpandoObject();
          foreach (var prop in Props)
            shapped.Add(prop.Name,prop.GetValue(entity));

          result.Add(shapped);
        }

        return Ok( result );
      }
      catch (Exception ex) {
        return BadRequest(new { Title = ex.GetType().Name, Error = ex });
      }
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
      var query = _context.Product.AsNoTracking();
      foreach (var item in filters)
        query = query.Where(p => item.Field.GetValue(p).ToString() == item.Value);
      var result = await query.ToListAsync();
      if (result.Count > 0)
        return Ok(result);
      else
        return NotFound();
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
      foreach (var item in filters) {
        if (item.Field == filters.First().Field)
          sql += $" where {item.Field} = '{item.Value}'";
        else
          sql += $" and {item.Field} = '{item.Value}'";
      }
      var result = await _context.Product.FromSql(sql).ToListAsync();
      if (result.Count > 0)
        return Ok(result);
      else
        return NotFound();
    }

    [HttpGet("orderBy")]
    public async Task<IActionResult> OrderBy([FromQuery] string fields)
    {
      try {
        string[] _fields = fields.Split(',', StringSplitOptions.RemoveEmptyEntries);
        // hold the Order Dictionary
        List<KeyValuePair<string,PropertyInfo>> OrderList = new List<KeyValuePair<string,PropertyInfo>>();
        // hold the first Order
        KeyValuePair<string,PropertyInfo> OrderItem;
        // hold the sorting pair
        string[] pair;
        // hold the sorting direction
        string dir = null;
        // hold the sorting property
        string prop = null;

        foreach (var field in _fields) {
            pair = field.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (pair.Length > 2)
              throw new ArgumentException(String.Format("Invalid OrderBy string: '{0}'. Order By Format: [Property] | [Property asc] | [Property desc]", field.Trim()));
            else {
              prop = pair[0].Trim();
              dir = "asc";
              if (pair.Length == 2)
                dir = pair[1].Trim();
            }
            OrderList.Add(new KeyValuePair<string,PropertyInfo>(dir,typeof(Product).GetProperty(prop, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)));
        }

        var query = _context.Product.AsNoTracking();
        OrderItem = OrderList.First();
        query = (OrderItem.Key == "asc")
                ? query.OrderBy(p => OrderItem.Value.GetValue(p))
                : query.OrderByDescending(p => OrderItem.Value.GetValue(p));

        int len = OrderList.Count;
        for (int i = 1; i < len; i++) {
          int index = i;
          query = (OrderList[index].Key == "asc")
                  ? (query as IOrderedQueryable<Product>).ThenBy(p => OrderList[index].Value.GetValue(p))
                  : (query as IOrderedQueryable<Product>).ThenByDescending(p => OrderList[index].Value.GetValue(p));
        }

        var Ordering = OrderList.Select(o => new { Prop = o.Value.Name, Order = o.Key });
        var Result = await query.ToListAsync();
        return Ok( new { Ordering, Result} );
      }
      catch (Exception ex) {
        return BadRequest(new { Title = ex.GetType().Name, Message = ex.Message, Error = ex });
      }
    }

    [HttpGet("sql")]
    public async Task<IActionResult> Sql([FromQuery] string sql)
    {
      try {
        var result = await _context.Product
                                .FromSql(sql)
                                .ToListAsync();
        return Ok(result);
      }
      catch (Exception ex) {
        return BadRequest(new { Title = ex.GetType().Name, Message = ex.Message });
      }

    }

    // GET: api/Products/list
    [HttpGet("list")]
    public async Task<IActionResult> GetAll() =>
        Ok(await _context.Product.ToListAsync());

    // GET: api/Products/page?number=1&size=10
    [HttpGet("page")]
    public IActionResult GetPaged([FromQuery] Page page) =>
        Ok(new PagedList<Product>(_context.Product.AsQueryable(), page.number, page.size));

    // GET: api/Products/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
      if (!ProductExists(id))
        return NotFound();
      return Ok(await _context.Product.FindAsync(id));
    }

    // POST: api/Products/add
    [HttpPost("add")]
    public async Task<IActionResult> Add([FromBody] Product newProduct)
    {
      return await DoAction(newProduct, "Added");
    }

    // POST: api/Products/update
    [HttpPut("update")]
    public async Task<IActionResult> Update([FromBody] Product updProduct)
    {
      return await DoAction(updProduct, "Modified");
    }

    // POST: api/Products/delete
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
      if (!ProductExists(id))
        return NotFound();
      var toDelProduct = _context.Product.Find(id);
      return await DoAction(toDelProduct, "Deleted");
    }
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

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

    // GET: api/Products?
    [HttpGet("filter")]
    public async Task<IActionResult> Filter([FromQuery] IDictionary<string, string> qStr)
    {
      var filters = qStr.Select(item => new
      {
        Field = typeof(Product).GetProperty(item.Key, BindingFlags.IgnoreCase |  BindingFlags.Public | BindingFlags.Instance),
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
      var result = await _context.Product.FromSql(sql).ToListAsync();
      if (result.Count > 0)
        return Ok(result);
      else
        return NotFound();
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

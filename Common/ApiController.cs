using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Threading.Tasks;

namespace AdventureWorks
{
  public class AdvSearch {
    public string SqlTableName { get; set; }
    public string KeyPropName { get; set; }
    public PropertyInfo KeyProp { get; set; }
    public List<string> QuerableFields { get; set; }
  }
  public class Response {
    public string Title { get; set; }
    public object Message { get; set; }
  }
  public class APIController<Tkey, TEntity, TDbContext> : Controller
          where Tkey : struct
          where TEntity : class
          where TDbContext : DbContext
  {
    private readonly TDbContext _db;
    private readonly AdvSearch _advSearch;
    private IEnumerable<dynamic> _filters;
    private TEntity found;
    private PropertyInfo _modifiedDateProp;
    private string _sqlQuery;

    public APIController(TDbContext dbContext, AdvSearch advSearch)
    {
      _db = dbContext;
      _advSearch = advSearch;
      _advSearch.KeyProp = typeof(TEntity).GetProperty(_advSearch.KeyPropName);
    }

    private bool FieldExists(string FieldName) => _advSearch.QuerableFields.Any(q => q == FieldName);

    private async Task<TEntity> Find(Tkey id) => await _db.Set<TEntity>()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => _advSearch.KeyProp.GetValue(e).Equals(id));
    private void SetState(TEntity entity, string _state)
    {
      EntityState state;
      switch (_state) {
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
    private async Task<IActionResult> DoAction(TEntity entity, string actionType)
    {
      if (ModelState.IsValid) {
        if (actionType == "Modified") {
          _modifiedDateProp = typeof(TEntity).GetProperty("ModifiedDate");
          if (_modifiedDateProp != null)
            _modifiedDateProp.SetValue(entity, DateTime.Now);
        }
        SetState(entity, actionType);
        try {
          await _db.SaveChangesAsync();
          switch (actionType) {
            case "Added":
              return Created($"api/Products/{_advSearch.KeyProp.GetValue(entity)}", entity);
            case "Modified":
              return Ok(entity);
            case "Deleted":
              return Ok(new Response { Title = "Success: Deleting",
                                       Message = $"Item with Id: {_advSearch.KeyProp.GetValue(entity)} Was Deleted From DB" });
            default:
              return NoContent();
          }
        }
        catch (Exception ex) {
          return BadRequest(new Response {  Title = "Error: SqlException",
                                            Message = ex.InnerException.Message });
        }
      }
      else {
        return BadRequest(new Response {  Title = "Error: Invalid Data",
                                          Message = ModelState });
      }
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Set<TEntity>().AsNoTracking().ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] Tkey id)
    {
      found = await Find(id);
      if (found == null)
        return NotFound();
      return Ok(found);
    }

    [HttpGet("page")]
    public IActionResult GetPaged([FromQuery] Page page) =>
                Ok(new PagedList<TEntity>(_db.Set<TEntity>().AsNoTracking(), page.number, page.size));

    [HttpGet("find")]
    public async Task<IActionResult> Find([FromQuery] IDictionary<string, string> query)
    {
      // capitalize the first letter of the field
      _filters = query.Select(item => new {
        Field = item.Key.Substring(0, 1).ToString().ToUpper() + item.Key.Substring(1),
        Value = item.Value
      });
      // filter out any not querable fields
      _filters = _filters.Where(f => FieldExists(f.Field));
      // if no remaining fields return badrequest
      if (_filters.Count() == 0)
        return BadRequest(new { Title = "Invalid QueryString Keys", Error = query });
      // set the base select statement
      _sqlQuery = $"select * from {_advSearch.SqlTableName}";
      // build the where clause
      foreach (var item in _filters) {
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
      if (result.Count > 0)
        return Ok(result);
      else
        return NotFound();
    }

    [HttpPost("add")]
    public async Task<IActionResult> Add([FromBody] TEntity newProduct)
    {
      return await DoAction(newProduct, "Added");
    }

    [HttpPut("update")]
    public async Task<IActionResult> Update([FromBody] TEntity updProduct)
    {
      return await DoAction(updProduct, "Modified");
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> Delete([FromRoute] Tkey id)
    {
      found = await Find(id);
      if (found == null)
        return NotFound();
      return await DoAction(found, "Deleted");
    }

  } // end class APIController
} // namespace
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        // GET: api/Products/list
        [HttpGet("list")]
        public async Task<IActionResult> GetAll() =>
            Ok(await _context.Product.ToListAsync());

        // GET: api/Products/paging?number=1&size=10
        [HttpGet("paging")]
        public IActionResult GetPaged([FromQuery] Paging page) =>
            Ok(new PagedList<Product>(_context.Product.AsQueryable(), page.number, page.size));

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id) {
            if (!ProductExists(id))
                return NotFound();
            var product = await _context.Product
                .Include(p => p.ProductModel)
                .Include(p => p.ProductSubcategory)
                .Include(p => p.SizeUnitMeasureCodeNavigation)
                .Include(p => p.WeightUnitMeasureCodeNavigation)
                .SingleOrDefaultAsync(m => m.ProductId == id);
            return Ok(product);
        }

        // GET: api/Products/find?
        [HttpGet("find")]
        public async Task<IActionResult> Find([FromQuery] IDictionary<string,string> query) {
            string sql = "select * from Production.Product";
            List<string> fields = query.Keys.Select(key => key.Substring(0,1).ToString().ToUpper()+key.Substring(1)).ToList();
            List<string> values = query.Keys.Select(key => query[key]).ToList();
            for (int i = 0; i < fields.Count; i++) {
                if(i==0)
                    sql += $" where {fields[i]} = '{values[i]}'";
                else
                    sql += $" and {fields[i]} = '{values[i]}'";
            }
            var result = await _context.Product.FromSql(sql).ToListAsync();
            if(result.Count > 0)
                return Ok(result);
            else
                return NotFound();
        }
    }
}

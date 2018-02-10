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
        public ProductsController(AdventureWorksContext context)
        {
            _context = context;
        }
        private bool ProductExists(int id)
        {
            return _context.Product.Any(e => e.ProductId == id);
        }
        // GET: api/Products
        [HttpGet()]
        public async Task<IActionResult> Get()
        {
            // var products = _context.Product.Include(p => p.ProductModel)
            //                             .Include(p => p.ProductSubcategory)
            //                             .Include(p => p.SizeUnitMeasureCodeNavigation)
            //                             .Include(p => p.WeightUnitMeasureCodeNavigation);
            return Ok(await _context.Product.ToListAsync());
        }
        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            if (!ProductExists(id))
            {
                return NotFound();
            }

            var product = await _context.Product
                .Include(p => p.ProductModel)
                .Include(p => p.ProductSubcategory)
                .Include(p => p.SizeUnitMeasureCodeNavigation)
                .Include(p => p.WeightUnitMeasureCodeNavigation)
                .SingleOrDefaultAsync(m => m.ProductId == id);

            return Ok(product);
        }
        [HttpGet("paging")]
        public IActionResult Get([FromQuery] Paging page)
        {
            var products = _context.Product.Include(p => p.ProductModel)
                                        .Include(p => p.ProductSubcategory)
                                        .Include(p => p.SizeUnitMeasureCodeNavigation)
                                        .Include(p => p.WeightUnitMeasureCodeNavigation);
            return Ok(new PagedList<Product>(products.AsQueryable(), page.number, page.size));
        }
    }
}

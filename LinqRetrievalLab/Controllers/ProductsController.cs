using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LinqRetrievalLab.Models.Data;
using LinqRetrievalLab.Models.Domain;
using LinqRetrievalLab.Models.Dto;

namespace LinqRetrievalLab.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

   
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await _context.Product.ToListAsync();
        }

     
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetProduct(int id)
        {
            var product = await _context.Product
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return new
            {
                product.Id,
                product.Name,
                product.Price,
                product.Stock,
                Category = new
                {
                    product.Category.Id,
                    product.Category.Name
                }
            };
        }

 
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(
            int id,
            ProductDto productDto)
        {
            var product = await _context.Product.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            var category = await _context.Category
                .FindAsync(productDto.CategoryId);

            if (category == null)
            {
                return BadRequest("Category not found.");
            }

            product.Name = productDto.Name;
            product.Price = productDto.Price;
            product.Stock = productDto.Stock;
            product.CategoryId = productDto.CategoryId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

   
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(
            ProductDto productDto)
        {
            if (productDto == null)
            {
                return BadRequest();
            }

            var category = await _context.Category
                .FindAsync(productDto.CategoryId);

            if (category == null)
            {
                return BadRequest("Category not found.");
            }

            var product = new Product
            {
                Name = productDto.Name,
                Price = productDto.Price,
                Stock = productDto.Stock,
                CategoryId = productDto.CategoryId
            };

            _context.Product.Add(product);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                "GetProduct",
                new { id = product.Id },
                product);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Product.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            _context.Product.Remove(product);

            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetProductsByCategory(
            int categoryId)
        {
            var category = await _context.Category.FindAsync(categoryId);

            if (category == null)
            {
                return NotFound();
            }

            return await _context.Product
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Price,
                    p.Stock,
                    Category = new
                    {
                        p.Category.Id,
                        p.Category.Name
                    }
                })
                .ToListAsync();
        }


        [HttpGet("search/{name}")]
        public async Task<ActionResult<IEnumerable<Product>>> SearchProducts(
            string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest();
            }

            return await _context.Product
                .Where(p => p.Name.Contains(name))
                .ToListAsync();
        }

  
        [HttpGet("price/{lower}/{upper}")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductsByPrice(
            decimal lower,
            decimal upper)
        {
            if (lower > upper)
            {
                return BadRequest();
            }

            return await _context.Product
                .Where(p => p.Price >= lower && p.Price <= upper)
                .ToListAsync();
        }

        [HttpGet("stock-total")]
        public async Task<ActionResult<int>> GetTotalStock()
        {
            if (!await _context.Product.AnyAsync())
            {
                return NotFound();
            }

            return await _context.Product
                .SumAsync(p => p.Stock);
        }

        [HttpGet("average-price")]
        public async Task<ActionResult<decimal>> GetAveragePrice()
        {
            if (!await _context.Product.AnyAsync())
            {
                return NotFound();
            }

            return await _context.Product
                .AverageAsync(p => p.Price);
        }

        [HttpGet("count")]
        public async Task<ActionResult<int>> GetProductCount()
        {
            return await _context.Product.CountAsync();
        }

        private bool ProductExists(int id)
        {
            return _context.Product.Any(e => e.Id == id);
        }
    }
}
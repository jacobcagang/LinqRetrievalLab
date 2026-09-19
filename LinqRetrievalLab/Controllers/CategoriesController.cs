using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LinqRetrievalLab.Models.Data;
using LinqRetrievalLab.Models.Domain;
using LinqRetrievalLab.Models.Dto;

namespace LinqRetrievalLab.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetCategories()
        {
            return await _context.Category
                .Select(c => new
                {
                    c.Id,
                    c.Name
                })
                .ToListAsync();
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetCategory(int id)
        {
            var category = await _context.Category
                .FindAsync(id);

            if (category == null)
            {
                return NotFound("Category not found.");
            }

            return new
            {
                category.Id,
                category.Name
            };
        }

        [HttpPost]
        public async Task<ActionResult<Category>> PostCategory(
            CategoryDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Category data is required.");
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest("Category name is required.");
            }

            var category = new Category
            {
                Name = dto.Name
            };

            _context.Category.Add(category);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Error saving category.");
            }

            return CreatedAtAction(
                nameof(GetCategory),
                new { id = category.Id },
                category
            );
        }

   
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(
            int id,
            CategoryDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Category data is required.");
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest("Category name is required.");
            }

            var category = await _context.Category.FindAsync(id);

            if (category == null)
            {
                return NotFound("Category not found.");
            }

            category.Name = dto.Name;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
                {
                    return NotFound("Category not found.");
                }

                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Category
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound("Category not found.");
            }

            if (category.Products.Any())
            {
                return BadRequest(
                    "Cannot delete category because it has products."
                );
            }

            _context.Category.Remove(category);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Error deleting category.");
            }

            return NoContent();
        }

        private bool CategoryExists(int id)
        {
            return _context.Category.Any(e => e.Id == id);
        }
    }
}
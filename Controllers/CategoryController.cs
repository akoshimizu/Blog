using Blog.Data;
using Blog.Extensions;
using Blog.Models;
using Blog.ViewModels;
using Blog.ViewModels.Categories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Blog.Controllers
{
    [ApiController]
    [Route("")]
    public class CategoryController : ControllerBase
    {
        [HttpGet("v1/categories")]
        public async Task<IActionResult> GetAsync(
            [FromServices]IMemoryCache cache,
            [FromServices]BlogDataContext context)
        {
            try
            {
                var categories = cache.GetOrCreate("CategoriesCache", entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                    return GetCategories(context);
                });
                return Ok(new ResultViewModel<List<Category>>(categories));
            }
            catch (System.Exception)
            {
                return StatusCode(500, new ResultViewModel<List<Category>>("05EX4 - Falha interna do servidor!"));
            }
        }

        private List<Category> GetCategories(BlogDataContext context)
        {
            return context.Categories.ToList();
        }

        [HttpGet("v1/categories/{id:int}")]
        public async Task<IActionResult> GetByIdAsync([FromServices]BlogDataContext context, [FromRoute] int id)
        {
            var category = await context
                .Categories
                .FirstOrDefaultAsync(cat => cat.Id == id);
            
            if(category == null) return NotFound(new ResultViewModel<Category>("Conteúdo não encontrado."));
            return Ok(new ResultViewModel<Category>(category));
        }

        [HttpPost("v1/categories")]
        public async Task<IActionResult> PostAsync([FromServices]BlogDataContext context, [FromBody] EditorCategoryViewModel model)
        {
            if(!ModelState.IsValid) return BadRequest(new ResultViewModel<Category>(ModelState.GetErrors()));

            try
            {
                var category = new Category{
                    Id = 0,
                    Name = model.Name,
                    Slug = model.Slug.ToLower(),
                    Posts = null
                };
            ;

                await context.Categories.AddAsync(category);
                await context.SaveChangesAsync();
                return Created($"v1/categories/{category.Id}", new ResultViewModel<Category>(category));
            }
            catch(DbUpdateException)
            {
                return StatusCode(500, new ResultViewModel<List<Category>>("05EX5 - Não foi possível incluir a categoria"));
            }
            catch (System.Exception)
            {
                return StatusCode(500, new ResultViewModel<List<Category>>("05EX4 - Falha interna do servidor!"));
            }
        }

        [HttpPut("v1/categories/{id:int}")]
        public async Task<IActionResult> PutAsync([FromServices]BlogDataContext context, [FromRoute] int id, [FromBody] EditorCategoryViewModel model)
        {
            if(!ModelState.IsValid) return BadRequest(new ResultViewModel<Category>(ModelState.GetErrors()));
            try
            {
                var category = await context
                .Categories
                .FirstOrDefaultAsync(cat => cat.Id == id);
            
            if(category == null) return NotFound(new ResultViewModel<Category>("Conteúdo não encontrado."));

            category.Name = model.Name;
            category.Slug = model.Slug;

            context.Categories.Update(category);
            await context.SaveChangesAsync();


            return Ok(new ResultViewModel<Category>(category));
            }
            catch(DbUpdateException)
            {
                return StatusCode(500, new ResultViewModel<List<Category>>("05EX7 - Não foi possível atualizar a categoria"));
            }
            catch (System.Exception)
            {
                return StatusCode(500, new ResultViewModel<List<Category>>("05EX4 - Falha interna do servidor!"));
            }
        }

        [HttpDelete("v1/categories/{id:int}")]
        public async Task<IActionResult> DeleteAsync([FromServices]BlogDataContext context, [FromRoute] int id)
        {
            var category = await context
                .Categories
                .FirstOrDefaultAsync(cat => cat.Id == id);
            
            if(category == null) return NotFound(new ResultViewModel<Category>("Conteúdo não encontrado."));
            context.Categories.Remove(category);
            await context.SaveChangesAsync();

            return Ok(new ResultViewModel<Category>(category));
        }
    }
}
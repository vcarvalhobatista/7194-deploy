using Kairos.API.Data;
using Kairos.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kairos.API.Controllers
{
    [ApiController]
    [Route("v1/categories")]    
    public class CategoryController : ControllerBase
    {

        [HttpGet]
        [AllowAnonymous]
        [ResponseCache(VaryByHeader = "User-agent", Location = ResponseCacheLocation.Any, Duration = 30)]
        // [ResponseCache(Duration = 0, Location = ResponseCacheLocation.Any, NoStore = true)]
        public async Task<ActionResult<List<Category>>> Get([FromServices]DataContext context)
        {
            return await context.Categories.AsNoTracking().ToListAsync();
        }

        [HttpGet("{ID:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<Category>> GetById(int ID, [FromServices]DataContext context)
        {
            try
            {               
            var category = await context.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == ID);
            if (category == null)
                return NotFound(new {message = "Categoria não encontrada"});

            return Ok(category);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new {message = "Não foi possível encontrar a categoria soliciada", error = ex.Message});
            }
        }

        [HttpPost]
        [Authorize(Roles = "employee")]        
        public async Task<ActionResult<Category>> Post(Category category, [FromServices]DataContext context)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {

                context.Categories.Add(category);
                await context.SaveChangesAsync();
                return Created("", new { categoria = category });
            }
            catch (System.Exception)
            {

                return BadRequest(new { message = "Não foi possível criar nova categoria" });
            }
        }

        [HttpPut("{ID:int}")]
        [Authorize(Roles = "employee")]
        public async Task<ActionResult<Category>> Put(int ID, Category category, [FromServices]DataContext context)
        {
            if (ID != category.Id)
                return NotFound(new { message = "Categoria não encontrada" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                context.Entry<Category>(category).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return Ok(category);

            }
            catch (DbUpdateConcurrencyException dbEx)
            {
                return BadRequest(new { message = $"Houve concorrência no momento da atualização da categoria {dbEx.Message}" });
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Não foi possível atualizar a categoria solicitada" });
            }

        }

        [HttpDelete("{ID:int}")]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult<Category>> Delete(int ID, [FromServices]DataContext context)
        {
            var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == ID);

            if (category == null)
            {
                return NotFound(new { message = "Categoria não encontrada" });
            }

            try
            {
                context.Categories.Remove(category);
                context.Entry<Category>(category).State = EntityState.Deleted;
                await context.SaveChangesAsync();
                return Ok(new { message = "Categoria removida com sucesso" });
            }
            catch (DbUpdateConcurrencyException dbEx)
            {
                return BadRequest(new { message = "Falha ao remover categoria do banco de dados", error = dbEx.Message });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = "Não foi possível excluir a categoria solicitada" });
            }


        }

    }
}
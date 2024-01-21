using System.Data;
using Kairos.API.Data;
using Kairos.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kairos.API.Controllers
{
    [Route("v1/products")]
    public class ProductController : ControllerBase
    {   
        [HttpGet("")]
        [AllowAnonymous]
        public async Task<ActionResult<List<Product>>> Get([FromServices]DataContext context)
        {
            var products = await context
                                .Products
                                .Include(c => c.Category)
                                .AsNoTracking()
                                .ToListAsync();
            return products;
        }

        [HttpGet("{ID:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<Product>> GetById(int ID, [FromServices]DataContext context)
        {
            try
            {               
            var product = await context
            .Products
            .Include(c => c.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == ID);
            return Ok(product);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new {message = "Não foi possível encontrar a categoria soliciada", error = ex.Message});
            }
        }

        [HttpGet("categories/{ID:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<Product>>> GetByCategoryId(int ID, [FromServices]DataContext context)
        {
            try
            {               
            var products = await context
            .Products
            .Include(c => c.Category)
            .AsNoTracking()
            .Where(x => x.Id == ID)
            .ToListAsync();
            return Ok(products);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new {message = "Não foi possível encontrar a categoria soliciada", error = ex.Message});
            }
        }

        [HttpPost("addproduct")]
        [Authorize(Roles = "employee")]
        public async Task<ActionResult<Product>> PostProduct([FromBody]Product product, [FromServices]DataContext context)
        {            
            try
            {
                if (ModelState.IsValid)
                {
                    context.Products.Add(product);
                    await context.SaveChangesAsync();

                    return Created("", product);
                }
                    return BadRequest(ModelState);                
            }
            catch(DBConcurrencyException e)
            {
                return BadRequest(new {message = $"não foi possível adicionar este produto {e}"});
            }
            catch (System.Exception ex)
            {
                 return BadRequest(new {message = $"não foi possível adicionar este produto {ex.Message}"});
            }
        }

        [HttpPut("{ID:int}")]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult<Product>> PutProduct(int ID, [FromBody]Product product ,[FromServices]DataContext context)
        {   
            if (!ModelState.IsValid)
                return BadRequest(new {message = "Produto não atende as especificações", error = ModelState});

            if(ID != product.Id)
                return NotFound(new {message = "Produto não encontrado"});

            try
            {                
                context.Entry<Product>(product).State = EntityState.Modified;
                await context.SaveChangesAsync();

                return Created("",new {message = "Produto alterado com sucesso"});
            }
            catch(DBConcurrencyException e)
            {
                return BadRequest(new {message = $"não foi possível adicionar este produto {e}"});
            }
            catch (System.Exception ex)
            {
                 return BadRequest(new {message = $"não foi possível adicionar este produto {ex.Message}"});
            }
        }

        [HttpDelete("{ID:int}")]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult> DeleteProduct(int ID,[FromServices]DataContext context)
        {            
            var product = await context.Products.FirstOrDefaultAsync(p => p.Id == ID);

            if (product == null)
                return NotFound(new {message = "Produto não existe"});

            try
            {
                context.Entry<Product>(product).State = EntityState.Deleted;
                context.Products.Remove(product);
                await context.SaveChangesAsync();
                return Ok(new {message = "Produto removido com sucesso"});
            }
            catch(DBConcurrencyException ex)
            {
                return BadRequest(new {message = "Houve falha no momento da remoção do produto" , error = ex.Message});
            }
            catch (System.Exception ex)
            {
                 return BadRequest(new {message = ex.Message});
            }
        }

    }
}
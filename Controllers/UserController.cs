using System.Data;
using Kairos.API.Data;
using Kairos.API.Models;
using Kairos.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Kairos.API.Controllers
{
    [ApiController]
    [Route("v1/user")]
    public class UserController : ControllerBase
    {
        [HttpPost("create")]
        [AllowAnonymous]
        // [Authorize(Roles = "manager")]
        public async Task<ActionResult<User>> CreateUser([FromServices] DataContext context, [FromBody] User user)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    user.Role = "employee";
                    context.Users.Add(user);
                    await context.SaveChangesAsync();
                    user.Password = "*******";

                    return Created("", new { message = "Usuário criado com sucesso", user });
                }
                return BadRequest(ModelState);
            }
            catch (DBConcurrencyException e)
            {
                return BadRequest(new { message = $"não foi possível adicionar este usuário {e}" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = $"não foi possível adicionar este usuário {ex.Message}" });
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<dynamic>> Login([FromServices] DataContext context, [FromBody] User user)
        {
            try
            {
                var userFromDB = await context.Users
                .AsNoTracking()
                .Where(x => x.UserName == user.UserName && x.Password == user.Password)
                .FirstOrDefaultAsync();

                if (user == null)
                    return NotFound(new { message = $"Usuário ou senha inválidos" });

                var _token = TokenService.GenerateToken(user);
                user.Password = "*******";
                return Created("", new
                {
                    user = user,
                    token = _token
                });

            }
            catch (DBConcurrencyException ex)
            {
                return BadRequest(new { message = "Houve falha na comunicação com o banco de dados" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = $"Usuário ou senha inválidos" });
            }
        }

        [HttpGet]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult<List<User>>> GetUsers([FromServices] DataContext context)
        {
            try
            {
                var users = await context.Users.AsNoTracking().ToListAsync();
                if (users == null)
                    return NotFound(new { message = "Não há usuários cadastrados" });

                return users;
            }
            catch (DBConcurrencyException ex)
            {
                return BadRequest(new { message = "Houve falha na comunicação com o banco de dados" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = $"Usuário não encontrados" });
            }
        }

        [HttpPut("{ID:int}")]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult> UpdateUser([FromServices] DataContext context, int ID, [FromBody] User user)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (ID != user.Id)
                    return NotFound(new { message = "Usuário não encontrado" });

                context.Entry<User>(user).State = EntityState.Modified;
                await context.SaveChangesAsync();

                return Ok(new { message = "Usuário alterado com sucesso", user = user });
            }
            catch (DBConcurrencyException ex)
            {
                return BadRequest(new { message = "Houve falha na comunicação com o banco de dados" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = $"Falha ao atualizar usuário", error = ex.Message });
            }
        }

        [HttpDelete("ID:int")]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult> DeleteUser([FromServices] DataContext context, int ID)
        {
            var user = context.Users.AsNoTracking().FirstOrDefault(u => u.Id == ID);

            if (user == null)
                return NotFound(new { message = "Usuário não encontrado" });

            try
            {
                context.Entry<User>(user).State = EntityState.Deleted;
                await context.SaveChangesAsync();

                return Ok(new { message = "Usuário excluído com sucesso", user = user });
            }
            catch (DBConcurrencyException ex)
            {
                return BadRequest(new { message = "Houve falha na comunicação com o banco de dados" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = $"Falha ao remover usuário", error = ex.Message });
            }
        }



        // [HttpGet("anonimo")]
        // [AllowAnonymous]
        // public string Anonimo() => "Anonimo";

        // [HttpGet("autenticado")]
        // [Authorize]
        // public string Autenticado() => "Autenticado";

        // [HttpGet("funcionario")]
        // [Authorize(Roles = "employee")]
        // public string Funcionario() => "Funcionario";

        // [HttpGet("gerente")]
        // [Authorize(Roles = "manager")]
        // public string Gerente() => "Gerente";
    }
}
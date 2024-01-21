using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kairos.API.Data;
using Kairos.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kairos.API.Controllers
{
    [ApiController]
    [Route("v1")]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<dynamic>> Get([FromServices]DataContext context)
        {
            var employee = new User{Id = 1, UserName = "robin", Password = "123456", Role = "employee"};
            var manager = new User{Id = 2, UserName = "batman", Password = "123456", Role = "manager"};
            var category = new Category{Id = 1, Title = "Informática"};
            var product = new Product{Id = 1, Category = category,Description = "Computador 32GB RAM, SSD M2 2TB, RTX 4090TI", Price = 299, Title = "Computador Gamer"};
            context.Users.Add(employee);
            context.Users.Add(manager);
            context.Categories.Add(category);
            context.Products.Add(product);
            await context.SaveChangesAsync();

            return Ok(new {
                message = "Dados configurados com sucesso"
            });
        }   
    }
}
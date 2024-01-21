using System.Text;
using Kairos.API;
using Kairos.API.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
// "connectionStrings" : "SERVER=DESKTOP-CQR5I74\\SQLEXPRESS;database=shop;Trusted_Connection=True;TrustServerCertificate=True",
// Add services to the container.
builder.Services.AddCors();
builder.Services.AddControllers();

var key = Encoding.ASCII.GetBytes(Settings.Secret);
builder.Services.AddAuthentication(a => {
    a.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    a.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(
    x => 
    {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddDbContext<DataContext>(opt => opt.UseInMemoryDatabase("Database"));
// builder.Services.AddDbContext<DataContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("connectionStrings")));

builder.Services.AddResponseCompression(options => 
{
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new [] {"applicarion/json"});
    
});
// builder.Services.AddResponseCaching();


//Every contract, every dependency need to be resolved.


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();    
}

// app.UseResponseCaching();

app.UseCors(x => x
.AllowAnyHeader()
.AllowAnyMethod()
.AllowAnyOrigin());

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

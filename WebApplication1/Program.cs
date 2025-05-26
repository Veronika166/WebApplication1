using Microsoft.EntityFrameworkCore;
using WebApplication1;
using System;
using System.Linq;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebApplication1.Services;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WebApplication1.Model;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<UserContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Currency API", Version = "v1" });

    c.AddSecurityDefinition("basic", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "basic",
        In = ParameterLocation.Header,
        Description = "Enter your username and password"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "basic"
                }
            },
            new string[] {}
        }
    });
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("BasicAuthentication", new AuthorizationPolicyBuilder("Basic")
        .RequireAuthenticatedUser()
        .Build());
});
builder.Services.AddHttpClient();
builder.Services.AddHostedService<CurrencyRateBackgroundService>();

//сервисы аутентификации
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "CurrencyApi",
            ValidateAudience = true,
            ValidAudience = "https://localhost:5001",
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("your_super_secret_key_here")),
            ValidateIssuerSigningKey = true,
        };
    });



Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.Use(async (context, next) =>
{
    if (context.Request.Headers.ContainsKey("Authorization"))
    {
        var authHeader = context.Request.Headers["Authorization"].ToString();
        if (authHeader.StartsWith("Basic "))
        {
            var encodedCredentials = authHeader["Basic ".Length..].Trim();
            var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials)).Split(':');
            var username = credentials[0];
            var password = credentials[1];

            // Проверка логина/пароля
            var user = await context.RequestServices.GetRequiredService<UserContext>()
                .Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user != null)
            {
                var hasher = context.RequestServices.GetRequiredService<IPasswordHasher<User>>();
                if (hasher.VerifyHashedPassword(user, user.PasswordHash, password) == PasswordVerificationResult.Success)
                {
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim("id", user.Id.ToString())
                    };
                    context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Basic"));
                }
            }
        }
    }
    await next();
});
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<UserContext>();

        db.Database.EnsureCreated();

        if (!db.Валюты.Any())
        {
            var currencies = new List<Валюта>
            {
                new Валюта { Название_валюты = "Доллар США" },
                new Валюта { Название_валюты = "Евро" },
                new Валюта { Название_валюты = "Российский рубль" }
            };

            db.Валюты.AddRange(currencies);
            db.SaveChanges();

            //Заполняем знчение для рубля для наглядности
            var today = DateTime.Today;
            db.КурсыВалют.AddRange(
                new Курсы_валют { Дата = today, Значение = 1.00m, ID_валюты = currencies[2].Id_валюты }
            );

            db.SaveChanges();
            Console.WriteLine("Данные успешно инициализированы");

            if (!db.Users.Any())
            {
                var passwordHasher = new PasswordHasher<User>();
                var adminUser = new User
                {
                    Username = "admin"
                };
                adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "admin123");

                db.Users.Add(adminUser);
                db.SaveChanges();
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ошибка инициализации БД");
    }
}



app.Run();

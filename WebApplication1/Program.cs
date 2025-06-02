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

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });

    c.AddSecurityDefinition("basicAuth", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "basic",
        Description = "������� ����� � ������ � ������� username:password"
    });

    c.OperationFilter<AddLoginOperationFilter>();
});

builder.Services.AddHttpClient();
builder.Services.AddHostedService<CurrencyRateBackgroundService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<UserContext>();

        db.Database.EnsureCreated();

        if (!db.������.Any())
        {
            var currencies = new List<������>
            {
                new ������ { ��������_������ = "������ ���" },
                new ������ { ��������_������ = "����" },
                new ������ { ��������_������ = "���������� �����" }
            };

            db.������.AddRange(currencies);
            db.SaveChanges();

            //��������� ������� ��� ����� ��� �����������
            var today = DateTime.Today;
            db.����������.AddRange(
                new �����_����� { ���� = today, �������� = 1.00m, ID_������ = currencies[2].Id_������ }
            );

            db.SaveChanges();
            Console.WriteLine("������ ������� ����������������");

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
        logger.LogError(ex, "������ ������������� ��");
    }
}



app.Run();

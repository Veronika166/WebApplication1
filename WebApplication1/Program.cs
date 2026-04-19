
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
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1", Description = "Currency Exchange API" });

    c.AddSecurityDefinition("basicAuth", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "basic",
        Description = "Введите логин и пароль в формате username:password"
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

        if (!db.Валюты.Any())
        {
            var currencies = new List<Currency>
            {
                new Currency { Название_валюты = "Доллар США" },
                new Currency { Название_валюты = "Евро" },
                new Currency { Название_валюты = "Российский рубль" }
            };

            db.Валюты.AddRange(currencies);
            db.SaveChanges();

            //Заполняем знчение для рубля для наглядности
            var today = DateTime.Today;
            db.КурсыВалют.AddRange(
                new Exchange_rates { Дата = today, Значение = 1.00m, ID_валюты = currencies[2].Id_валюты }
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

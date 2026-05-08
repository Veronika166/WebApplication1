using WebApplication1.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1", Description = "Currency Exchange API" });
  
});

builder.Services.AddHttpClient();
builder.Services.AddScoped<CurrencyRateBackgroundService>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();


app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<DBContext>();

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
           
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ошибка инициализации БД");
    }
}



app.Run();

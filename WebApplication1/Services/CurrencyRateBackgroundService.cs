using Microsoft.EntityFrameworkCore;
using System.Xml.Serialization;
using Microsoft.Extensions.DependencyInjection;
using WebApplication1.Model;
using System.Globalization;
using System.Xml;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;

namespace WebApplication1.Services
{
    public class CurrencyRateBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<CurrencyRateBackgroundService> _logger;
        private readonly HttpClient _httpClient;

        public CurrencyRateBackgroundService(
            IServiceProvider services,
            ILogger<CurrencyRateBackgroundService> logger,
            HttpClient httpClient)
        {
            _services = services;
            _logger = logger;
            _httpClient = httpClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Служба обновления курсов запущена");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _services.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider
                            .GetRequiredService<UserContext>();

                        await FetchAndSaveRates(dbContext);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обновлении курсов");
                }

                // Ожидание 1 минуту перед следующим обновлением
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task FetchAndSaveRates(UserContext dbContext)
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                var today = DateTime.Today.ToString("dd/MM/yyyy");
                var url = $"https://www.cbr.ru/scripts/XML_daily.asp?date_req={today}";
                var responseBytes = await _httpClient.GetByteArrayAsync(url);

                // Конвертируем из windows-1251
                var encoding = Encoding.GetEncoding(1251);
                var xmlString = encoding.GetString(responseBytes);

                var serializer = new XmlSerializer(typeof(CbrCurrencyRate));
                using var reader = new StringReader(xmlString);
                var result = (CbrCurrencyRate)serializer.Deserialize(reader);

                // Получаем текущие курсы
                var usdRate = result.Currencies.FirstOrDefault(c => c.Code == "USD");
                var eurRate = result.Currencies.FirstOrDefault(c => c.Code == "EUR");
                var todayDate = DateTime.Today;

                if (usdRate != null)
                {
                    await SaveRate(dbContext, "Доллар США", todayDate, usdRate.Value);
                }

                if (eurRate != null)
                {
                    await SaveRate(dbContext, "Евро", todayDate, eurRate.Value);
                }

              

                _logger.LogInformation($"Курсы обновлены в {DateTime.Now}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении курсов");
                throw;
            }
        }

        private async Task SaveRate(UserContext dbContext, string currencyName, DateTime date, decimal value)
        {
            try
            {
                var currency = await dbContext.Валюты
                    .FirstOrDefaultAsync(c => c.Название_валюты == currencyName);

                if (currency == null)
                {
                    _logger.LogWarning($"Валюта {currencyName} не найдена");
                    return;
                }

                var hour = DateTime.Now.Hour;
                var minuteSlot = DateTime.Now.Minute / 1; 

                var exists = await dbContext.КурсыВалют
                    .AnyAsync(r => r.ID_валюты == currency.Id_валюты &&
                                 r.Дата.Date == date.Date &&
                                 r.Дата.Hour == hour &&
                                 r.Дата.Minute / 1 == minuteSlot);

                if (!exists)
                {
                    var newRate = new Курсы_валют
                    {
                        Дата = DateTime.Now, 
                        Значение = value,
                        ID_валюты = currency.Id_валюты
                    };

                    dbContext.КурсыВалют.Add(newRate);
                    var count = await dbContext.SaveChangesAsync();

                    _logger.LogInformation($"Добавлен курс: {currencyName} {value} (ID: {newRate.ID_курса})");
                }
                else
                {
                    _logger.LogInformation($"Курс {currencyName} уже существует для этого временного слота");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при сохранении курса {currencyName}");
            }
        }
    }
}

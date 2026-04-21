using System.Net.Http;

namespace WebApplication1.Services;

public class CurrencyService
{
    private ILogger<CurrencyService> _logger;
    private HttpClient _httpClient;

    public CurrencyService(
        IServiceProvider services,
        ILogger<CurrencyService> logger,
        HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }
    public async Task FetchAndSaveRates(DBContext dbContext)
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

    public async Task SaveRate(DBContext dbContext, string currencyName, DateTime date, decimal value)
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
                var newRate = new Exchange_rates
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


namespace WebApplication1.Services;

public class CurrencyRateBackgroundService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<CurrencyRateBackgroundService> _logger;
    private readonly CurrencyService _currencyService;
   public CurrencyRateBackgroundService(
        IServiceProvider services,
        ILogger<CurrencyRateBackgroundService> logger,
        CurrencyService currencyService)
    {
        _currencyService = currencyService;
        _services = services;
        _logger = logger;
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
                        .GetRequiredService<DBContext>();

                    await _currencyService.FetchAndSaveRates(dbContext);
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
}
   

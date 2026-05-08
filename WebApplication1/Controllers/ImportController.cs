namespace WebApplication1.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ImportController : ControllerBase
{
    private readonly DBContext _context;
    private readonly ILogger<ImportController> _logger;

    public ImportController(DBContext context, ILogger<ImportController> logger)
    {
        _context = context;
        _logger = logger;
    }
    [HttpGet("test")]
    public IActionResult Test()
    {
        throw new Exception();
    }
    [HttpPost("upload-rates")]
    public async Task<IActionResult> UploadRates([FromBody] List<ExchangeRateDto> rates)
    {

            foreach (var rate in rates)
            {
                // Проверяем существование валюты
                var currencyExists = await _context.Валюты
                    .AnyAsync(c => c.Id_валюты == rate.CurrencyId);

                if (!currencyExists)
                {
                    _logger.LogWarning($"Валюта с ID {rate.CurrencyId} не найдена");
                    continue;
                }

                // Проверяем, не существует ли уже курс на эту дату
                var rateExists = await _context.КурсыВалют
                    .AnyAsync(r => r.Дата == rate.Date && r.ID_валюты == rate.CurrencyId);

                if (rateExists)
                {
                    _logger.LogInformation($"Курс для валюты {rate.CurrencyId} на {rate.Date} уже существует");
                    continue;
                }

                // Добавляем новый курс
                var newRate = new Exchange_rates
                {
                    Дата = rate.Date,
                    Значение = rate.Value,
                    ID_валюты = rate.CurrencyId
                };

                _context.КурсыВалют.Add(newRate);
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Данные успешно импортированы" });
   
    }
}



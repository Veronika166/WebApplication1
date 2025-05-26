using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImportController : ControllerBase
    {
        private readonly UserContext _context;
        private readonly ILogger<ImportController> _logger;

        public ImportController(UserContext context, ILogger<ImportController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("upload-rates")]
        public async Task<IActionResult> UploadRates([FromBody] List<ExchangeRateDto> rates)
        {
            try
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
                    var newRate = new Курсы_валют
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при импорте данных");
                return StatusCode(500, new { Error = "Ошибка при импорте данных" });
            }
        }
    }

    public class ExchangeRateDto
    {
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
        public int CurrencyId { get; set; }
    }
}

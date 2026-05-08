namespace WebApplication1.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CurrencyController : ControllerBase
{
    private readonly ILogger<CurrencyController> _logger;
    private readonly DBContext _database;


    public CurrencyController(
        ILogger<CurrencyController> logger, DBContext database)
    {
        _logger = logger;
        _database = database;
    }

    [HttpGet]
    public async Task<IActionResult> GetCurrencyList()
        {
        var currencyList = await _database.Валюты.ToListAsync();
        return Ok(currencyList);
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCurrencyOne(int id)
    {
        var currencyOne = await _database.Валюты.FirstOrDefaultAsync(c => c.Id_валюты == id);
        if (id == 0)
        {
            _logger.LogError("Неверный Id");
            return NotFound($"Нет такого Id: {id}");
        }
        else if (currencyOne is null)
        {
            _logger.LogError("Валюта не найдена");
            return NotFound("Нет такой валюты");
        }
        return Ok(currencyOne);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCurrency(int id, [FromBody] CurrencyDto currencyDto)
    {
        var currency = await _database.Валюты.FindAsync(id);
        if (id == 0)
        {
            _logger.LogError("Неверный Id");
            return NotFound($"Нет такого Id: {id}");
        }
        currency.Название_валюты = currencyDto.Name;
        await _database.SaveChangesAsync();
        _logger.LogInformation($"Запись обновлена Id - {id} название - {currencyDto.Name}");
        return Ok(currency);
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCurrency(int id)
    {
        var currencyDel = await _database.Валюты.FindAsync(id);
        if (id == 0)
        {
            _logger.LogError("Неверный Id");
            return NotFound($"Невозможно удалить по данному Id: {id}");
        }
        else if (currencyDel is null)
        {
            _logger.LogError("Запись не найдена");
            return NotFound("Нет такой записи по данному Id");
        }
        _database.Валюты.Remove(currencyDel);
        await _database.SaveChangesAsync();      
        return NoContent(); 
    }
}


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

    [HttpGet("all-currencies")]
    public async Task<IActionResult> GetCurrencyList()
        {
        var currencyList = await _database.Валюты.ToListAsync();
        return Ok(currencyList);
    }
    [HttpGet("one-currencies")]
    public async Task<IActionResult> GetCurrencyOne(int id)
    {
        var currencyOne = await _database.Валюты.FirstOrDefaultAsync(c => c.Id_валюты == id);
        return Ok(currencyOne);
    }

}

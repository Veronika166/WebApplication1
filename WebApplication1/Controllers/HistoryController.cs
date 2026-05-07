namespace WebApplication1.Controllers;

public class HistoryController : Controller
{
    private readonly ILogger<HistoryController> _logger;
    private readonly ICurrencyService _currencyService;

    public HistoryController(
     ILogger<HistoryController> logger, ICurrencyService currencyService)
    {
        _logger = logger;
        _currencyService = currencyService;
    }
    [HttpGet("{CurrencyName}")]
    public async Task<ActionResult> GetHistory(string CurrencyName)
    {
        var rate = await _currencyService.GetCurrencyHistory(CurrencyName);
        return Ok(rate);
    }
}

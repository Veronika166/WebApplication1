namespace WebApplication1.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RateController : ControllerBase
{
    private readonly ILogger<RateController> _logger;
    private readonly ICurrencyService _currencyService;

    public RateController(
     ILogger<RateController> logger, ICurrencyService currencyService)
    {
        _logger = logger;
        _currencyService = currencyService;
    }
    [HttpGet("{date}")]
    public async Task<ActionResult> GetCbList(DateOnly date)
    {
        var rate = await _currencyService.GetCbCurrencyRate(date);
        return Ok(rate);
    }

  
}

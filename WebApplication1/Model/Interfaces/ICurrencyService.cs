namespace WebApplication1.Model.Interfaces;

public interface ICurrencyService
{
    Task FetchAndSaveRates(DBContext dbContext);
    Task SaveRate(DBContext dbContext, string currencyName, DateTime date, decimal value);

}

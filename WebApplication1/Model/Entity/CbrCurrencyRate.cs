
namespace WebApplication1.Model.Entity;

[XmlRoot("ValCurs")]
public class CbrCurrencyRate
{
    [XmlElement("Valute")]
    public List<CbrCurrency> Currencies { get; set; }
}
public class CbrCurrency
{
    [XmlElement("CharCode")]
    public string Code { get; set; }

    [XmlElement("Value")]
    public string StringValue { get; set; }

    [XmlIgnore]
    public decimal Value => decimal.Parse(StringValue.Replace(",", "."), CultureInfo.InvariantCulture);
}

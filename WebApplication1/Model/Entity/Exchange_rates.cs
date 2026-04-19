namespace WebApplication1.Model.Entity;

//2 таблица
public class Exchange_rates
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID_курса { get; set; }

    [Required]
    public DateTime Дата { get; set; }

    [Required]
    [Column(TypeName = "decimal(15, 6)")]
    public decimal Значение { get; set; }

    [Required]
    [ForeignKey("Валюта")] 
    public int ID_валюты { get; set; }

    public Currency Валюта { get; set; }
}

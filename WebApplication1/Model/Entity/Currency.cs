namespace WebApplication1.Model.Entity;

//1 таблица
public class Currency
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id_валюты { get; set; }

    [Required]
    [MaxLength(50)]
    public string Название_валюты { get; set; }
}

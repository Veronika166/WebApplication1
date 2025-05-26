using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1
{
    //2 таблица
    public class Курсы_валют
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

        public Валюта Валюта { get; set; }
    }
}

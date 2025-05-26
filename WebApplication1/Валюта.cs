using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1
{
    //1 таблица
    public class Валюта
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id_валюты { get; set; }

        [Required]
        [MaxLength(50)]
        public string Название_валюты { get; set; }
    }
}

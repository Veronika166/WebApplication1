using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Model
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Имя пользователя обязательно")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Длина логина от 3 до 20 символов")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Пароль обязателен")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}

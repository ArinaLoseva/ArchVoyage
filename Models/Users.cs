using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ArchVoyage.Models
{
    public class Users
    {
        [Key]
        public int? UserID { get; set; }

        [Required(ErrorMessage = "Email обязателен для заполнения")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            ErrorMessage = "Пожалуйста, введите корректный email адрес")]
        public string Email { get; set; }

        [NotMapped]
        [Required(ErrorMessage = "Пароль обязателен для заполнения")]
        [RegularExpression(@"^[A-Za-z\d]{8,}$",
            ErrorMessage = "Пароль должен содержать минимум 8 символов, включая только латинские буквы и цифры.")]
        public string Password { get; set; }

        public string? PasswordHash { get; set; }

        [NotMapped]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        public string? ConfirmPassword { get; set; }

        [NotMapped]
        public string? RecaptchaToken { get; set; }

    }
}
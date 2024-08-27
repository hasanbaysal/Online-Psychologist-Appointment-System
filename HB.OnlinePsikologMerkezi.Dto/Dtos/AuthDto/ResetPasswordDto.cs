using System.ComponentModel.DataAnnotations;

namespace HB.OnlinePsikologMerkezi.Dto.Dtos
{
    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "Şifre girmelisin")]
        [Display(Name = "Şifre")]
        [RegularExpression(pattern: "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).+$"
            , ErrorMessage = "şifre bir büyük harf, bir küçük harf ve bir sayı içermeli")]
        [MinLength(8, ErrorMessage = "şifre minimum 8 karakter olmalı")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Display(Name = "Şifre Tekrar")]
        [Required(ErrorMessage = "Şifreyi tekrar girmelisin")]
        [Compare(nameof(Password), ErrorMessage = "Şifreler Eşleşmiyor")]
        [DataType(DataType.Password)]
        public string RepeatPassword { get; set; } = null!;
    }
}

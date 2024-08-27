using System.ComponentModel.DataAnnotations;

namespace HB.OnlinePsikologMerkezi.Dto.Dtos
{
    public class UserSignUpDto
    {
        [Required(ErrorMessage = "Bir kullanıcı adı girmelisin")]
        [MaxLength(15, ErrorMessage = "Kullanıcı adı en fazla 15 karakter olabilir")]
        [Display(Name = "Kullanıcı Adı")]
        public string UserName { get; set; } = null!;


        [Display(Name = "E-Posta Adresin")]
        [Required(ErrorMessage = "Mutlaka bir email adresi girmelisin")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi girmelisin")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Bir şifre belirlemelisiniz")]
        [RegularExpression(pattern: "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).+$"
            , ErrorMessage = "şifre bir büyük harf, bir küçük harf ve bir sayı içermeli")]
        [MinLength(8, ErrorMessage = "şifre minimum 8 karakter olmalı")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Şifrenizi Tekrar Giriniz")]
        [Compare(nameof(UserSigInDto.Password), ErrorMessage = "Şifreler Uyuşmuyor")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre Tekrar")]
        public string PasswordConfirm { get; set; } = null!;

        public bool TermOfService { get; set; }
    }


}

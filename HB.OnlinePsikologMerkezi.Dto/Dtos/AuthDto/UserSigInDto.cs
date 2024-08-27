using System.ComponentModel.DataAnnotations;

namespace HB.OnlinePsikologMerkezi.Dto.Dtos
{
    public class UserSigInDto
    {
        [Required(ErrorMessage = "e-posta boş bırakılamaz")]
        [EmailAddress(ErrorMessage = "geçersiz e-posta")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "şifre boş bırakılmaz")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        public bool RememberMe { get; set; }
    }
}

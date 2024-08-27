using System.ComponentModel.DataAnnotations;

namespace HB.OnlinePsikologMerkezi.Dto.Dtos
{
    public class ForgetPasswordDto
    {

        [Required(ErrorMessage = "Mutkala bir e-posta addresi girmelisin")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi girmelisin")]
        [Display(Name = "E-Posta Adresin")]
        public string Email { get; set; } = null!;
    }
}

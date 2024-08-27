using System.ComponentModel.DataAnnotations;

namespace HB.OnlinePsikologMerkezi.Web.Models
{
    public class MessageViewModel
    {

        [Required(ErrorMessage = "bu alan boş bırakılamaz")]
        [MaxLength(25, ErrorMessage = "En fazla 25 karakter olabilir")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "bu alan boş bırakılamaz")]
        [MaxLength(25, ErrorMessage = "En fazla 25 karakter olabilir")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "bu alan boş bırakılamaz")]
        
        [RegularExpression(pattern: @"^(05(\d{9}))$", ErrorMessage = "geçerli bir telefon numarası giriniz")]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "bu alan boş bırakılamaz")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi girmelisin")]
        [MaxLength(45, ErrorMessage = "En fazla 45 karakter olabilir")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "bu alan boş bırakılamaz")]
        [MaxLength(350, ErrorMessage = "En fazla 350 karakter olabilir")]
        public string UserMessage { get; set; } = null!;
    }
}

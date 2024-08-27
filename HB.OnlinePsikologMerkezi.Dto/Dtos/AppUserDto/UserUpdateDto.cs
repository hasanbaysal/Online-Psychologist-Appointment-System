using System.ComponentModel.DataAnnotations;

namespace HB.OnlinePsikologMerkezi.Dto.Dtos
{
    public class UserUpdateDto
    {
        public string Id { get; set; }

        [Required( ErrorMessage ="kullanıcı adı girilmesi zorunludur")]
        [MaxLength(15, ErrorMessage = "Kullanıcı adı en fazla 15 karakter olabilir")]
        [MinLength(5, ErrorMessage = "Kullanıcı adı en 5 karakter olmalı")]
        public string UserName { get; set; }

        public string? Email { get; set; }
        [MaxLength(30, ErrorMessage = "Ad en fazla 30 karakter olabilir")]
        public string? Name { get; set; }

        [MaxLength(30, ErrorMessage = "Soyaden fazla 30 karakter olabilir")]
        public string? LastName { get; set; }

        [RegularExpression(pattern: @"^(05(\d{9}))$", ErrorMessage = "geçerli bir telefon numarası giriniz")]
        public string? PhoneNumber { get; set; }

    }
}

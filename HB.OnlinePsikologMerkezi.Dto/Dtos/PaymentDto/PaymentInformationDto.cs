using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace HB.OnlinePsikologMerkezi.Dto.Dtos
{
    public class PaymentInformationDto
    {

        [Required(ErrorMessage ="Zorunlu Alan")]
        public string NameOnTheCard { get; set; } = null!;

        [CreditCard(ErrorMessage ="Eksik ")]
        [Required(ErrorMessage = "Zorunlu Alan")]
        [MinLength(16, ErrorMessage ="Hatalı Kart Numarası")]
        [RegularExpression(@"^[0-9]*$", ErrorMessage ="Geçersiz Karakter")]
        public string CardNumbet { get; set; } = null!;


        [Required(ErrorMessage = "Zorunlu Alan")]
        [RegularExpression(pattern: @"^\d{2}\/\d{2}$", ErrorMessage ="Geçersiz Tarih")]
        public string TotalDate { get; set; } = null!;

        [ValidateNever]
        public string ExpireMonth { get; set; } = null!;
        [ValidateNever]
        public string ExpireYear { get; set; } = null!;

        [Required(ErrorMessage = "Zorunlu Alan")]
        public string Cvc { get; set; } = null!;

        [ValidateNever]
        public int seccondkey { get; set; } = 0;
        [ValidateNever]
        public int appointmentID { get; set; } = 0;

    }
}

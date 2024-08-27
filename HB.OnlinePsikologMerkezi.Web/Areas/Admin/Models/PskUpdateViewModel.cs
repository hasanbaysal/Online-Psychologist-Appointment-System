using HB.OnlinePsikologMerkezi.Dto.Dtos;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace HB.OnlinePsikologMerkezi.Web.Areas.Admin.Models
{
    public class PskUpdateViewModel
    {
        public string Psychologist_ID { get; set; }

        //public int SecondKey { get; set; }

        [Required(ErrorMessage = "cv boş olamaz")]
        public string? Cv { get; set; }

        [Required(ErrorMessage = "danışma ücreti boş olamaz")]
        public int ConsulationPrice { get; set; }


        [Required(ErrorMessage = "site sıralama boş olamaz")]

        public int Rank { get; set; }

        public string? Title { get; set; }
        public bool IsWorking { get; set; }

        [Required(ErrorMessage = "Kısa açıklama belirtiniz")]
        public string ShortDescription { get; set; }


        [ValidateNever]
        public IFormFile? Photo { get; set; }


        public List<CategoryPskAddDto> PsychologistCategories { get; set; }

        public PskUpdateViewModel()
        {
            PsychologistCategories = new();
        }
    }
}

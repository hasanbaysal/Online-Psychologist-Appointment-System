using System.ComponentModel.DataAnnotations;

namespace HB.OnlinePsikologMerkezi.Web.Areas.Admin.Models
{
    public class PskAddViewModel
    {

        public string Psychologist_ID { get; set; }
        [Required(ErrorMessage = "cv gerekli")]
        public string? Cv { get; set; }

        [Required(ErrorMessage = "resim gerekli")]
        public IFormFile Photo { get; set; }

        [Required(ErrorMessage = "görüşme ücreti belirtilmeli")]
        public int ConsulationPrice { get; set; }

        [Required(ErrorMessage = "sayfa sıralaması belirtiniz")]
        public int Rank { get; set; }

        public bool IsWorking { get; set; }

        [Required(ErrorMessage = "Kısa  Açıklama belirtiniz")]
        public string ShortDescription { get; set; }

        public string? Title { get; set; }

        //[Required(ErrorMessage = "Kategory belirtin")]
        //public List<CategoryPskAddDto> PsychologistCategories { get; set; }

        //public PskAddViewModel()
        //{
        //    PsychologistCategories = new();
        //}


    }
}

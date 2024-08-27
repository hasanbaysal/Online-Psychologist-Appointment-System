using System.ComponentModel.DataAnnotations;

namespace HB.OnlinePsikologMerkezi.Dto.Dtos
{
    public class PsychologistAddDto
    {

        public string Psychologist_ID { get; set; }
        public string? Cv { get; set; }
        public string? ProfilePhotoPath { get; set; }

        [Required(ErrorMessage = "görüşme ücreti belirtilmeli")]
        public int ConsulationPrice { get; set; }

        [Required(ErrorMessage = "sayfa sıralaması belirtiniz")]
        public int Rank { get; set; }

        //psk sayfasında bunu düzenle
        public string? Title { get; set; }

        [Required(ErrorMessage = "Kısa açıklama belirtiniz")]
        public string ShortDescription { get; set; }



        public bool IsWorking { get; set; }

        public List<CategoryPskAddDto> PsychologistCategories { get; set; }

        public PsychologistAddDto()
        {
            PsychologistCategories = new();
        }

    }



}

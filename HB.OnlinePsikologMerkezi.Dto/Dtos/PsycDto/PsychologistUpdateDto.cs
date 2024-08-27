using System.ComponentModel.DataAnnotations;

namespace HB.OnlinePsikologMerkezi.Dto.Dtos
{
    public class PsychologistUpdateDto
    {

        public string Psychologist_ID { get; set; }

        //public int SecondKey { get; set; }
        public string? Cv { get; set; }
        public string? ProfilePhotoPath { get; set; }
        public int ConsulationPrice { get; set; }
        public int Rank { get; set; }
        public bool IsWorking { get; set; }

        [Required(ErrorMessage = "Kısa açıklama belirtiniz")]
        public string ShortDescription { get; set; }

        //psk update sayfasında bunu düzenle
        public string? Title { get; set; }
        public List<CategoryPskAddDto> PsychologistCategories { get; set; }

        public PsychologistUpdateDto()
        {
            PsychologistCategories = new();
        }

    }



}

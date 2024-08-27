namespace HB.OnlinePsikologMerkezi.Dto.Dtos
{
    public class PsychologistListDto
    {
        public PsychologistListDto()
        {
            Appointments = new();
            PsychologistCategories = new();
        }
        public string Psychologist_ID { get; set; }
        public AppUserListDto AppUser { get; set; }
        public int Rank { get; set; }
        public string? Cv { get; set; }
        public string? ProfilePhotoPath { get; set; }
        public int ConsulationPrice { get; set; }

        public string? Title { get; set; }
        public int SecondKey { get; set; }

        public string ShortDescription { get; set; }

        public List<AppointmentListDto> Appointments { get; set; }

        public List<CategoryPskListDto> PsychologistCategories { get; set; }

    }
}

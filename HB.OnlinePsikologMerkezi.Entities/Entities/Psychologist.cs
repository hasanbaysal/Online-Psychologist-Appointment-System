using HB.OnlinePsikologMerkezi.Entities.Interface;
using System.ComponentModel.DataAnnotations.Schema;

namespace HB.OnlinePsikologMerkezi.Entities.Entities
{
    public class Psychologist : IBaseEntity
    {

        public string Psychologist_ID { get; set; }
        public AppUser AppUser { get; set; }



        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SecondKey { get; set; }

        public string? Title { get; set; }
        public string? Cv { get; set; }
        public string? ProfilePhotoPath { get; set; }
        public int ConsulationPrice { get; set; }
        public List<Appointment> Appointments { get; set; }
        public int Rank { get; set; } = 0;

        public string ShortDescription { get; set; }
        public bool IsWorking { get; set; }

        public List<PsychologistCategory> PsychologistCategories { get; set; }

        public Psychologist()
        {
            Appointments = new List<Appointment>();
            PsychologistCategories = new();

        }
    }

}

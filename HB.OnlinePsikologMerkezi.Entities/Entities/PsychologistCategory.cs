using HB.OnlinePsikologMerkezi.Entities.Interface;

namespace HB.OnlinePsikologMerkezi.Entities.Entities
{
    /// <summary>
    /// cross table
    /// </summary>
    public class PsychologistCategory : IBaseEntity
    {
        public int CategorId { get; set; }
        public string PsycholoistId { get; set; }

        public Category Category { get; set; }
        public Psychologist Psychologist { get; set; }

    }
}

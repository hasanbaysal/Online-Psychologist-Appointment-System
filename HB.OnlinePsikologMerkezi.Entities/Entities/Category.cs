using HB.OnlinePsikologMerkezi.Entities.Interface;

namespace HB.OnlinePsikologMerkezi.Entities.Entities
{
    public class Category : IBaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public List<PsychologistCategory> PsychologistCategories { get; set; }

        public Category()
        {
            PsychologistCategories = new();

        }
    }
}

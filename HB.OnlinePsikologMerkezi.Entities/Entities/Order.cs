using HB.OnlinePsikologMerkezi.Entities.Interface;

namespace HB.OnlinePsikologMerkezi.Entities.Entities
{
    public class Order : IBaseEntity
    {
        public int Id { get; set; }
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }

        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; }

        public DateTime DurchaseDate { get; set; }
        public int Price { get; set; }

        public bool IsItPaid { get; set; } = false;

    }



}

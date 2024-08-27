namespace HB.OnlinePsikologMerkezi.Dto.Dtos
{
    public class OrderListDto
    {

        public int Id { get; set; }
        public string AppUserId { get; set; }
        public AppUserListDto AppUser { get; set; }

        public int AppointmentId { get; set; }
        public AppointmentListDto Appointment { get; set; }

        public DateTime DurchaseDate { get; set; }
        public int Price { get; set; }

        public bool IsItPaid { get; set; } = false;


    }
}

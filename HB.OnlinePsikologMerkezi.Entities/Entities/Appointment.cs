using HB.OnlinePsikologMerkezi.Entities.Interface;

namespace HB.OnlinePsikologMerkezi.Entities.Entities
{
    public class Appointment : IBaseEntity
    {
        public int Id { get; set; }
        public DateTime AppointmentDate { get; set; }
        public Psychologist Psychologist { get; set; }
        public string PsychologistId { get; set; }
        public int Status { get; set; }
        public int Price { get; set; }

        public Order Order { get; set; }

        public string? AppointmentDetails { get; set; }
        public string? MeetingLink { get; set; }

        public string? CustomerId { get; set; }
        public string? ConversationId { get; set; }

        //list dto içine yerleştir
        public string? ThirdPartyPaymentId { get; set; }

        public string? UserAppointmentComment { get; set; }

        public DateTime? Start3DTime { get; set; }



    }

}

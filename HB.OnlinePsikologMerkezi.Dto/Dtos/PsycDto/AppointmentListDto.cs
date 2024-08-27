namespace HB.OnlinePsikologMerkezi.Dto.Dtos
{
    public class AppointmentListDto
    {

        public int Id { get; set; }
        public DateTime AppointmentDate { get; set; }
        public PsychologistListDto Psychologist { get; set; }
        public string PsychologistId { get; set; }

        public int Status { get; set; }
        public int Price { get; set; }

        public OrderListDto Order { get; set; }

        public string? AppointmentDetails { get; set; }
        public string? MeetingLink { get; set; }


        public string? ThirdPartyPaymentId { get; set; }

        public string? UserAppointmentComment { get; set; }
    }
}

namespace HB.OnlinePsikologMerkezi.Dto.Dtos
{
    public class AppointmentAddDto
    {
        public AppointmentAddDto()
        {
            Dates = new();
        }
        public string PskId { get; set; } = null!;
        public List<DateTime> Dates { get; set; }

    }
}

namespace HB.OnlinePsikologMerkezi.Dto.Dtos
{
    public class AppUserListDto
    {
        public AppUserListDto()
        {
            Orders = new();
        }
        public string Id { get; set; }
        public string? Name { get; set; } = "belirtilmedi";
        public string? LastName { get; set; } = "belirtilmedi";
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; } = "belirtilmedi";
        public List<OrderListDto> Orders { get; set; }
        public string? LastLoginIpAddress { get; set; }

        public bool isBlock { get; set; } = false;

    }
}

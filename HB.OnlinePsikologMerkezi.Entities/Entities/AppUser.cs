using HB.OnlinePsikologMerkezi.Entities.Interface;
using Microsoft.AspNetCore.Identity;

namespace HB.OnlinePsikologMerkezi.Entities.Entities
{
    public class AppUser : IdentityUser, IBaseEntity
    {
        public AppUser()
        {
            Orders = new List<Order>();


        }

        public string? Name { get; set; }
        public string? LastName { get; set; }
        public string? LastLoginIpAddress { get; set; }
        public Psychologist Psychologist { get; set; }
        public List<Order> Orders { get; set; }

        public bool isBlock { get; set; } = false;


    }

}

using HB.OnlinePsikologMerkezi.Entities.Interface;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.OnlinePsikologMerkezi.Entities.Entities
{
    public class Message: IBaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; } 
        public string Email { get; set; }
        public string UserMessage { get; set; }
    }
}

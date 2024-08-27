using AutoMapper;
using HB.OnlinePsikologMerkezi.Entities.Entities;
using HB.OnlinePsikologMerkezi.Web.Models;

namespace HB.OnlinePsikologMerkezi.Web.Mapping
{
    public class MessageProfile:Profile
    {
        public MessageProfile()
        {
            CreateMap<Message, MessageViewModel>().ReverseMap();
        }
    }
}

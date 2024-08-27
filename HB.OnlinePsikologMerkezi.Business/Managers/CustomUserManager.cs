using AutoMapper;
using HB.OnlinePsikologMerkezi.Business.Services;
using HB.OnlinePsikologMerkezi.Common.CustomResponse;
using HB.OnlinePsikologMerkezi.Data.Interface;
using HB.OnlinePsikologMerkezi.Dto.Dtos;
using HB.OnlinePsikologMerkezi.Entities.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HB.OnlinePsikologMerkezi.Business.Managers
{
    public class CustomUserManager : IUserService
    {
        private readonly UserManager<AppUser> userManager;
        private readonly IUow uow;
        private readonly IMapper mapper;

        public CustomUserManager(UserManager<AppUser> userManager, IUow uow, IMapper mapper)
        {
            this.userManager = userManager;
            this.uow = uow;
            this.mapper = mapper;
        }

        public async Task<Response<NoDataResponse>> UserUpdate(UserUpdateDto dto)
        {

            var user = await userManager.FindByIdAsync(dto.Id);

            user.UserName = dto.UserName;
            user.Name = dto.Name;
            user.LastName = dto.LastName;
            user.PhoneNumber = dto.PhoneNumber;

            await userManager.UpdateAsync(user);

            return new Response<NoDataResponse>(ResponseType.Success);
        }
        public async Task<Response<List<OrderListDto>>> GetUserOrders(string userid)
        {
            var data = await uow.GetRepository<Order>().GetQueryable()
                .Include(x => x.Appointment)
                .ThenInclude(x => x.Psychologist)
                .OrderByDescending(x => x.DurchaseDate)
                .ToListAsync();

            var mappeddata = mapper.Map<List<OrderListDto>>(data);

            return new Response<List<OrderListDto>>(ResponseType.Success, mappeddata);

        }

    }
}

using AutoMapper;
using FluentValidation;
using HB.OnlinePsikologMerkezi.Business.Extentions;
using HB.OnlinePsikologMerkezi.Business.Services;
using HB.OnlinePsikologMerkezi.Common.CustomEnums;
using HB.OnlinePsikologMerkezi.Common.CustomResponse;
using HB.OnlinePsikologMerkezi.Common.Defaults;
using HB.OnlinePsikologMerkezi.Data.Context;
using HB.OnlinePsikologMerkezi.Data.Interface;
using HB.OnlinePsikologMerkezi.Dto.Dtos;
using HB.OnlinePsikologMerkezi.Entities.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
//08.06.1997 Gül <3 26.05.2023

namespace HB.OnlinePsikologMerkezi.Business.Managers
{
    public class AdminManager : IAdminService
    {
        private readonly UserManager<AppUser> userManager;
        private readonly IUow uow;
        private readonly IMapper mapper;
        private readonly IValidator<PsychologistAddDto> pskAddValidation;
        private readonly IValidator<PsychologistUpdateDto> pskUpdateValidation;
        private readonly AppDbContext context;
        private readonly SignInManager<AppUser> signInManager;

        public AdminManager(UserManager<AppUser> userManager,
            IUow uow,
            IMapper mapper,
            IValidator<PsychologistAddDto> pskAddValidation,
            IValidator<PsychologistUpdateDto> pskUpdateValidation,
            AppDbContext context,
            SignInManager<AppUser> signInManager)
        {
            this.userManager = userManager;
            this.uow = uow;
            this.mapper = mapper;
            this.pskAddValidation = pskAddValidation;
            this.pskUpdateValidation = pskUpdateValidation;
            this.context = context;
            this.signInManager = signInManager;
        }

        public async Task<Response<PsychologistAddDto>> PsychologistRoleAddAsync(PsychologistAddDto dto)
        {

            var validationResult = pskAddValidation.Validate(dto);

            if (!validationResult.IsValid)
            {
                return new Response<PsychologistAddDto>(dto, validationResult.CustomErrorList());
            }


            var user = await userManager.FindByIdAsync(dto.Psychologist_ID);



            var roleExist = await userManager.IsInRoleAsync(user, RoleDefaults.psk);
            if (roleExist)
            {
                return new Response<PsychologistAddDto>(ResponseType.Fail, "bu kullanıcı zaten psikolog");
            }

            //role kontrol

            //maping işlemini



            var mappeddata = mapper.Map<Psychologist>(dto);


            uow.GetRepository<Psychologist>().Add(mappeddata);
            await uow.CommitAsync();

            await userManager.AddToRoleAsync(user, RoleDefaults.psk);

            return new Response<PsychologistAddDto>(ResponseType.Success);

        }

        public async Task<Response<PsychologistUpdateDto>> GetPskForUpdate(string pskİd)
        {

            var data = await
                uow.GetRepository<Psychologist>()
                .GetQueryable()
                .AsNoTrackingWithIdentityResolution()
                .Where(x => x.Psychologist_ID == pskİd)
                .Include(x => x.PsychologistCategories)
                .FirstOrDefaultAsync();


            //.GetByFilterAsync(x => x.Psychologist_ID == pskİd);
            var mappedData = mapper.Map<PsychologistUpdateDto>(data);
            return new Response<PsychologistUpdateDto>(ResponseType.Success, mappedData);
        }

        public async Task<Response<PsychologistUpdateDto>> PskUpdate(PsychologistUpdateDto dto)
        {

            var validationREsult = pskUpdateValidation.Validate(dto);

            if (!validationREsult.IsValid)
            {

                return new Response<PsychologistUpdateDto>(dto, validationREsult.CustomErrorList());
            }

            var data = mapper.Map<Psychologist>(dto);


            var orginalData = await uow.GetRepository<Psychologist>().GetQueryable().Include(x => x.PsychologistCategories).Where(x => x.Psychologist_ID == dto.Psychologist_ID).FirstOrDefaultAsync();

            orginalData!.ConsulationPrice = dto.ConsulationPrice;
            orginalData!.Rank = dto.Rank;
            orginalData!.Cv = dto.Cv;
            orginalData!.ProfilePhotoPath = dto.ProfilePhotoPath;
            orginalData!.IsWorking = dto.IsWorking;
            orginalData!.ShortDescription = dto.ShortDescription;
            orginalData!.Title = dto.Title;
            var new_list = new List<PsychologistCategory>();
            dto.PsychologistCategories.ForEach(x => new_list.Add(new()
            {
                CategorId = x.CategorId,
                PsycholoistId = dto.Psychologist_ID
            }));
            orginalData.PsychologistCategories = new_list;
            //data.PsychologistCategories.ForEach(x => x.PsycholoistId = dto.Psychologist_ID);
            context.Set<Psychologist>().Update(orginalData);

            context.Entry(orginalData).Property(x => x.SecondKey).IsModified = false;


            //  var  list = new List<PsychologistCategory>();
            //  dto.PsychologistCategories.ForEach(x => list.Add(new()
            //  {
            //      CategorId= x.CategorId,
            //      PsycholoistId=dto.Psychologist_ID
            //  }));

            //  data.PsychologistCategories = list;


            //var  orjinaldata = await uow.GetRepository<Psychologist>().GetQueryable().Include(x => x.PsychologistCategories).Where(x => x.Psychologist_ID == dto.Psychologist_ID).FirstOrDefaultAsync();

            //  orjinaldata = data;

            //  context.Set<Psychologist>().Update(data);
            //  context.Entry(data).Property(x => x.SecondKey).IsModified = false;

            await context.SaveChangesAsync();

            //uow.GetRepository<Psychologist>().Update(data,orginalData!);



            //await uow.CommitAsync();

            return new Response<PsychologistUpdateDto>(ResponseType.Success, dto);

        }

        //set isWoking false bu düzenlemeye gitmese de olur
        public async Task<Response<NoDataResponse>> PsychologistRoleRemoveAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return new Response<NoDataResponse>(ResponseType.Fail, "geçerli bir kullanıcı adı girin");
            }

            var user = await userManager.FindByNameAsync(name);
            if (user == null)
            {
                return new Response<NoDataResponse>(ResponseType.NotFound, "Böyle bir kullanıcı yok");
            }



            var result = await userManager.RemoveFromRoleAsync(user, RoleDefaults.psk);

            if (!result.Succeeded)
            {
                return new Response<NoDataResponse>(ResponseType.Fail, "işlem başarısız bir hata meydana geldi");
            }


            var repo = uow.GetRepository<Psychologist>();

            var psk = await repo.GetByFilterAsync(x => x.Psychologist_ID == user.Id);

            if (psk == null)
            {
                return new Response<NoDataResponse>(ResponseType.Fail, "bu kişi psikolog değil");
            }
            psk.IsWorking = false;

            await uow.CommitAsync();

            //sistemden kullanıcıyı düşür

            await userManager.UpdateSecurityStampAsync(user);

            return new Response<NoDataResponse>(ResponseType.Success, "işlem başarılı psikolog pasife alındı psikolog yetkisi kaldırıldı");
        }




        public async Task<Response<NoDataResponse>> UserBlockAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return new Response<NoDataResponse>(ResponseType.Fail, "kullanıcı adı boş bırakılamaz");
            }
            var user = await userManager.FindByNameAsync(name);

            if (user == null)
            {

                return new Response<NoDataResponse>(ResponseType.NotFound, "böyle bir kullanıcı yok");

            }


            user.isBlock = true;

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return new Response<NoDataResponse>(ResponseType.Fail, "bir hat meydana geldi");

            }

            await userManager.UpdateSecurityStampAsync(user);

            return new Response<NoDataResponse>(ResponseType.Success, "kullanıcı engellendi");

        }

        public async Task<Response<NoDataResponse>> UserUnBlockAsync(string name)
        {

            if (string.IsNullOrEmpty(name))
            {
                return new Response<NoDataResponse>(ResponseType.Fail, "kullanıcı adı boş bırakılamaz");
            }
            var user = await userManager.FindByNameAsync(name);

            if (user == null)
            {

                return new Response<NoDataResponse>(ResponseType.NotFound, "böyle bir kullanıcı yok");

            }


            user.isBlock = false;

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return new Response<NoDataResponse>(ResponseType.Fail, "bir hata meydana geldi");
            }

            return new Response<NoDataResponse>(ResponseType.Success, "engel kaldırıldı");

        }

        public async Task<List<AppUserListDto>> GetUserAsync(int pagesize, int pagenumber)
        {

            var data = await context.Users
                 .Where(x => !context.Set<Psychologist>().Any(b => b.Psychologist_ID == x.Id))
                 .Skip((pagenumber - 1) * pagesize)
                 .Take(pagesize)
                 .AsNoTracking()
                 .ToListAsync();

            var mappeddata = mapper.Map<List<AppUserListDto>>(data);
            return mappeddata;

        }
        public async Task<Response<AppUserListDto>> GetUserdetails(string userId)
        {
            var data = await context.Users
                .Where(x => x.Id == userId)
                .Include(x => x.Orders)
                .ThenInclude(x => x.Appointment)
                .ThenInclude(x => x.Psychologist)
                .ThenInclude(x => x.AppUser)
                .FirstOrDefaultAsync();

            var mappedData = mapper.Map<AppUserListDto>(data);

            return new Response<AppUserListDto>(ResponseType.Success, mappedData);

        }
        public async Task<Response<AppUserListDto>> GetUserByEmail(string email)
        {
            var data = await context.Users
               .Where(x => x.Email == email)
               .Include(x => x.Orders)
               .ThenInclude(x => x.Appointment)
               .FirstOrDefaultAsync();

            var mappedData = mapper.Map<AppUserListDto>(data);

            return new Response<AppUserListDto>(ResponseType.Success, mappedData);
        }
        public async Task<Response<List<OrderListDto>>> GetAppointmentsByDate(OrderDate orderDate)
        {
            List<Order> data = new();
            if (orderDate == OrderDate.today)
            {
                data = await uow.GetRepository<Order>()
                .GetQueryable()
                .Include(x => x.AppUser)
                .Include(x => x.Appointment)
                .ThenInclude(x => x.Psychologist)
                .Where(x => x.DurchaseDate.Date == DateTime.Now.Date)
                .ToListAsync();

            }
            if (orderDate == OrderDate.this_week)
            {
                data = await uow.GetRepository<Order>()
               .GetQueryable()
                .Include(x => x.AppUser)
                .Include(x => x.Appointment)
                .ThenInclude(x => x.Psychologist)
               .Where(x => x.DurchaseDate.Date >= DateTime.Now.Date.AddDays(-7))
               .ToListAsync();
            }
            if (orderDate == OrderDate.this_month)
            {
                data = await uow.GetRepository<Order>()
               .GetQueryable()
                .Include(x => x.AppUser)
                .Include(x => x.Appointment)
                 .ThenInclude(x => x.Psychologist)
                .Where(x => x.DurchaseDate.Date >= DateTime.Now.Date.AddDays(-30))
               .ToListAsync();
            }

            if (orderDate == OrderDate.all)
            {
                data = await uow.GetRepository<Order>()
               .GetQueryable()
                .Include(x => x.AppUser)
                .Include(x => x.Appointment)
                .ThenInclude(x => x.Psychologist)
               .ToListAsync();
            }

            var mappedata = mapper.Map<List<OrderListDto>>(data);

            return new Response<List<OrderListDto>>(ResponseType.Success, mappedata);
        }
    }
}

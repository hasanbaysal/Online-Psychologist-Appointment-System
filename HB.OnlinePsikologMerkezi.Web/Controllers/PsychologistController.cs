using AspNetCoreHero.ToastNotification.Abstractions;
using AutoMapper;
using HB.OnlinePsikologMerkezi.Business.Services;
using HB.OnlinePsikologMerkezi.Common.CustomEnums;
using HB.OnlinePsikologMerkezi.Data.Context;
using HB.OnlinePsikologMerkezi.Data.Interface;
using HB.OnlinePsikologMerkezi.Dto.Dtos;
using HB.OnlinePsikologMerkezi.Entities.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel;
using System.Formats.Asn1;
using System.Security.Claims;

namespace HB.OnlinePsikologMerkezi.Web.Controllers
{
    public class PsychologistController : Controller
    {
        private readonly IUow uow;
        private readonly IMapper mapper;
        private readonly IPsychologistService psychologistService; 
        private readonly INotyfService _notyf;
        private readonly IMemoryCache _memoryCache;
        private readonly ICategoryService categoryService;
        private readonly AppDbContext context;

        string psk_cache_key = "psk_liste_data";
        string cate_cache_key = "cate_liste_data";


        public PsychologistController(IUow uow, IPsychologistService psychologistService, IMapper mapper, INotyfService notyf, IMemoryCache memoryCache, ICategoryService categoryService, AppDbContext context)
        {
            this.uow = uow;
            this.psychologistService = psychologistService;
            this.mapper = mapper;
            _notyf = notyf;
            _memoryCache = memoryCache;
            this.categoryService = categoryService;
            this.context = context;
        }

        [Authorize(Roles =("psk"))]
        public IActionResult AddAppointment()
        {

            ViewBag.userid = User!.Claims!.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault()!.Value;


            return View();
        }

        [Authorize(Roles = ("psk"))]
        [HttpPost]
    
        public async Task<IActionResult> SaveAppointment(AppointmentAddDto dto)
        {



            //appointmendate < datetime.now && status ==6 (3dcheck) => sil
            //appointmendate < datetime.now && status ==0 (newapp) => sil

            try
            {

                var oldData = await uow.GetRepository<Appointment>()
                            .GetQueryable()
                            .Where(x => x.PsychologistId == dto.PskId && (x.AppointmentDate < DateTime.Now && x.Status == (int)AppointmentEnum.s3dCheck)).ToListAsync();


                var oldData2 = await uow.GetRepository<Appointment>()
                          .GetQueryable()
                          .Where(x => x.PsychologistId == dto.PskId && (x.AppointmentDate < DateTime.Now && x.Status == (int)AppointmentEnum.new_appointment)).ToListAsync();



                if (oldData != null)
                {
                    if (oldData.Count > 0)
                    {
                        context.Set<Appointment>().RemoveRange(oldData);
                        await context.SaveChangesAsync();
                    }
                }

                if (oldData2 != null)
                {
                    if (oldData2.Count > 0)
                    {
                        context.Set<Appointment>().RemoveRange(oldData2);
                        await context.SaveChangesAsync();
                    }
                }




                if (dto.Dates.Count > 1)
                {


                    var tempList = dto.Dates.OrderByDescending(x => x).ToList();
                    bool control = false;
                    for (int i = 0; i < tempList.Count - 1; i++)
                    {
                        var fark = tempList[i] - tempList[i + 1];


                        if ((int)fark.TotalMinutes >= 60)
                        {

                        }
                        else
                        {
                            control = true;
                        }
                    }

                    if (control)
                    {
                        return NotFound();
                    }

                }







                var result = await psychologistService.AddAppointment(dto);


                if (result.ResponseType == Common.CustomResponse.ResponseType.Success)
                {
                    return RedirectToAction("AddAppointment");
                }
                else
                {

                    return NotFound();
                }

            }
            catch (Exception)
            {

                var result = await psychologistService.AddAppointment(dto);


                if (result.ResponseType == Common.CustomResponse.ResponseType.Success)
                {
                    return RedirectToAction("AddAppointment");
                }
                else
                {

                    return NotFound();
                }

          
            }
           

          
           
        }
        [Authorize(Roles = ("psk"))]
        public async Task<IActionResult> GetAppointmentsTable(string id)
        {

            var data = await uow.GetRepository<Psychologist>()
               .GetQueryable()
               .Where(x => x.Psychologist_ID == id)
               .Include(x => x.Appointments.Where(x=>(x.AppointmentDate>DateTime.Now && x.Status == (int)AppointmentEnum.new_appointment)||(x.Status == (int)AppointmentEnum.sold && x.AppointmentDate > DateTime.Now)))
               .FirstOrDefaultAsync();
            var mappedData = mapper.Map<List<AppointmentListDto>>(data?.Appointments);
            return PartialView("_MyAppointmentsListPartialView", mappedData);




        }
        [Authorize(Roles = ("psk"))]
        public async Task<IActionResult> PsychologistDashBoard()
        {

            if (TempData["hata"] != null)
            {
                _notyf.Error("toplantı bağlantısı hatayı yöneti ile iletişime geçiniz");
            }


            

            //appointmentlardan hepsinin order'ı dolu gelmeyecek ona göre işlem yap

            var userid = User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).First().Value;

            var data = await uow.GetRepository<Psychologist>()
                .GetQueryable()
                .Where(x => x.Psychologist_ID == userid)
                .Include(x => x.AppUser)
                    .Include(x => x.PsychologistCategories)
                         .ThenInclude(x => x.Category)
                                  .Include(x => x.Appointments.Where(y=>y.Status == (int) AppointmentEnum.sold || y.Status == (int)AppointmentEnum.Completed || y.Status == (int)AppointmentEnum.cancel)  )
                                        .ThenInclude(x => x.Order)
                                        .ThenInclude(x=>x.AppUser)
                                  .FirstOrDefaultAsync();

            var mappeddata = mapper.Map<PsychologistListDto>(data);

            return View(mappeddata);
        }
        [AllowAnonymous]
        public async Task<IActionResult> PsychologistProfile(int id)
        {
          
            if (TempData["appointment_conccureny"] != null)
            {
                _notyf.Error("satın alma başarısız");
                _notyf.Warning("Bu randevu bir başkası tarafından satın alındı");
            }

            var data =  await uow.GetRepository<Psychologist>()
                .GetQueryable()
                .Where(x => x.SecondKey == id)
                .Include(x => x.Appointments.Where(y=>y.AppointmentDate > DateTime.Now.AddMinutes(5) && y.Status ==(int)AppointmentEnum.new_appointment))
                .Include(x => x.AppUser)
                .AsNoTrackingWithIdentityResolution()
                .FirstOrDefaultAsync();

            if (data == null)
            {
                Redirect("/");
            }
            
            
            var mappedData=  mapper.Map<PsychologistListDto>(data);


            
            return View(mappedData);
        }

        [AllowAnonymous]
        [HttpGet("Psychologist/PsychologistInfo/{name}/{id}")]
        public async Task<IActionResult> PsychologistInfo(string name,int id)
        {


            var data = await uow.GetRepository<Psychologist>()
            .GetQueryable()
            .Where(x => x.SecondKey == id)
            .Include(x => x.Appointments.Where(y => y.AppointmentDate > DateTime.Now.AddMinutes(5) && y.Status == (int)AppointmentEnum.new_appointment))
            .Include(x => x.AppUser)
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync();

            if (data == null)
            {
                Redirect("/");
            }


            var mappedData = mapper.Map<PsychologistListDto>(data);



            return View(mappedData);

        }


        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {



            List<CategoryListDto> categoryLists;

            if (!_memoryCache.TryGetValue(cate_cache_key, out categoryLists))
            {
                var categorydata = await categoryService.GetCategories();

                categoryLists = categorydata!.Data;
                MemoryCacheEntryOptions options = new MemoryCacheEntryOptions();
                options.AbsoluteExpiration = DateTime.Now.AddSeconds(600);
                _memoryCache.Set<List<CategoryListDto>>(cate_cache_key, categoryLists);
            }

            ViewBag.categories = categoryLists;

            List<PsychologistListDto> psyList;
            if (!_memoryCache.TryGetValue(psk_cache_key, out psyList))
            {
                var result = await psychologistService.GetPsycholistsWithCategort();
                psyList = result!.Data;
                MemoryCacheEntryOptions options = new MemoryCacheEntryOptions();
                options.AbsoluteExpiration = DateTime.Now.AddSeconds(600);
                _memoryCache.Set<List<PsychologistListDto>>(psk_cache_key, psyList);
            }






            return View(psyList);
        }

        [Authorize(Roles = ("psk"))]
        public async Task<IActionResult> AppointmenComplated(int id)
        {
            //gelen id  üzerinden appointmenta git statusu complated yap 

            return View();
        }


        [Authorize(Roles = ("psk"))]
        public async Task<IActionResult> DeleteAppointment(int id)
        {



            var data = await uow.GetRepository<Appointment>().GetQueryable().Where(x => x.Id == id).FirstOrDefaultAsync();
            if (data != null)
            {
                //silmek için appointment'ın statusunun ancak new appointment olması lazım
                if (data.Status == (int)AppointmentEnum.new_appointment)
                {

                uow.GetRepository<Appointment>().Delete(data);
                await uow.CommitAsync();

                }
            }

            return Redirect("/Psychologist/AddAppointment");
        }
    }
}

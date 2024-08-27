using AutoMapper;
using HB.OnlinePsikologMerkezi.Business.Services;
using HB.OnlinePsikologMerkezi.Common.CustomEnums;
using HB.OnlinePsikologMerkezi.Common.CustomResponse;
using HB.OnlinePsikologMerkezi.Data.Interface;
using HB.OnlinePsikologMerkezi.Dto.Dtos;
using HB.OnlinePsikologMerkezi.Entities.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HB.OnlinePsikologMerkezi.Business.Managers
{
    public class PsychologistManager : IPsychologistService
    {

        private readonly UserManager<AppUser> userManager;
        private readonly IUow uow;
        private readonly IMapper mapper;

        public PsychologistManager(UserManager<AppUser> userManager, IUow uow, IMapper mapper)
        {
            this.userManager = userManager;
            this.uow = uow;
            this.mapper = mapper;
        }

        public async Task<Response<NoDataResponse>> AddAppointment(AppointmentAddDto dto)
        {

            if (string.IsNullOrEmpty(dto.PskId))
            {
                return new Response<NoDataResponse>(ResponseType.Fail, "hata");
            }

            var psk = await uow.GetRepository<Psychologist>().GetByFilterAsync(x => x.Psychologist_ID == dto.PskId);

            if (psk == null)
            {
                return new Response<NoDataResponse>(ResponseType.Fail, "hata");
            }

            bool validationError = false;

            foreach (var item in dto.Dates)
            {
                if (DateTime.Now > item)
                {
                    validationError = true;
                }
            }
            if (validationError)
            {
                return new Response<NoDataResponse>(ResponseType.Fail, "hata");
            }



            var myCurrentAppointmnet = await uow.GetRepository<Appointment>()
                .GetQueryable()
                .Where( x=>x.PsychologistId == dto.PskId &&
                (x.AppointmentDate > DateTime.Now) 
                &&
                (
                x.Status == (int)AppointmentEnum.sold ||
                x.Status == (int)AppointmentEnum.new_appointment ||
                x.Status == (int)AppointmentEnum.being_bought ||
                x.Status == (int)AppointmentEnum.s3dCheck)
                )
                .AsNoTracking()
                .Select(x => x.AppointmentDate)
                .ToListAsync();

            if (myCurrentAppointmnet != null)
            {
                if(myCurrentAppointmnet.Count>0)
                {

                    myCurrentAppointmnet.AddRange(dto.Dates);

                    var controldateList = myCurrentAppointmnet.OrderByDescending(x => x).ToList();
                    bool control = false;
                    for (int i = 0; i < controldateList.Count-1; i++)
                    {
                        var fark = controldateList[i] - controldateList[i + 1];

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
                        return new Response<NoDataResponse>(ResponseType.Fail, "hata");
                    }

                }
            }
            else { 
                //db boş ama kendi aralarında kontrol
                var controldates = dto.Dates.OrderByDescending(x=>x).ToList();
                bool control = false;
                for (int i = 0; i < controldates.Count-1; i++)
                {
                    var fark = controldates[i] - controldates[i + 1];

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
                    return new Response<NoDataResponse>(ResponseType.Fail, "hata");
                }

            }



            var appointments = new List<Appointment>();

            dto.Dates.ForEach(x =>
            {
                appointments.Add(new Appointment()
                {
                    PsychologistId = psk.Psychologist_ID,
                    Price = psk.ConsulationPrice,
                    AppointmentDate = x,
                    Status = (int)AppointmentEnum.new_appointment
                });
            });

            uow.GetRepository<Appointment>().AddRange(appointments);
            await uow.CommitAsync();



            return new Response<NoDataResponse>(ResponseType.Success);
        }

        public async Task<Response<NoDataResponse>> RemoveAppointment(string pskid, int id)
        {



            if (string.IsNullOrEmpty(pskid))
            {
                return new Response<NoDataResponse>(ResponseType.Fail, "hata");
            }

            var psk = await uow.GetRepository<Psychologist>().GetQueryable().Where(x => x.Psychologist_ID == pskid).Include(x => x.Appointments).SingleOrDefaultAsync();

            if (psk == null)
            {
                return new Response<NoDataResponse>(ResponseType.Fail, "hata");

            }

            if (psk.Appointments.Where(x => x.Id == id).FirstOrDefault() == null)
            {
                return new Response<NoDataResponse>(ResponseType.Fail, "hata");
            }

            var randevu = psk.Appointments.Where(x => x.Id == id).FirstOrDefault();

            if (randevu!.Status != (int)AppointmentEnum.new_appointment)
            {
                return new Response<NoDataResponse>(ResponseType.Fail, "bu randevu üzerinde şu an bir başkası işlem yapıyor");
            }

            uow.GetRepository<Appointment>().Delete(randevu);
            await uow.CommitAsync();

            return new Response<NoDataResponse>(ResponseType.Success);

        }

        //satılan randevuları getir

        public async Task<Response<List<PsychologistListDto>>> GetPsycholistsWithCategort()
        {

            var data = await uow.GetRepository<Psychologist>()
                .GetQueryable()
                .Where(x=>x.IsWorking)
                .Include(x => x.AppUser)
                .Include(x => x.PsychologistCategories)
                .ThenInclude(x => x.Category)
                .OrderBy(x => x.Rank)
                .ToListAsync();


            var mappedataa = mapper.Map<List<PsychologistListDto>>(data);





            return new Response<List<PsychologistListDto>>(ResponseType.Success, mappedataa);
        }

        public async Task<Response<List<AppointmentListDto>>> GetAppointmensASync(AppointmentEnum type, int secondkey)
        {
            var data = await uow.GetRepository<Appointment>().GetQueryable().Include(x => x.Psychologist).Where(x => x.Psychologist.SecondKey == secondkey).Include(x => x.Order).Include(x => x.Psychologist.AppUser).ToListAsync();

            //bu 3  tablonun joini ama duruma göre order ve user null olabilir
            // bunu kullanacağın yere göre null olma durumu değişir
            //randevu alan user'da order alanı boş olmaz user alanı da boş olmaz
            //ama listede sadece duran ve satılmayı bekleyen bir şeyse order ve user boş olur yani null

            var mappedData = mapper.Map<List<AppointmentListDto>>(data);

            var filterdata = mappedData.Where(x => x.Status == (int)type).ToList();

            return new Response<List<AppointmentListDto>>(ResponseType.Success, filterdata);

        }
        //satılan randevulara bilgi gir ve sahibine mesaj yolla

        //satışta olan randevularım getir

    }
}

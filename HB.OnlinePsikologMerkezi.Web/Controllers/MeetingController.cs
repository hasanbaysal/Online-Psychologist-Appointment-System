using HB.OnlinePsikologMerkezi.Common.CustomEnums;
using HB.OnlinePsikologMerkezi.Data.Interface;
using HB.OnlinePsikologMerkezi.Entities.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.ObjectModelRemoting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace HB.OnlinePsikologMerkezi.Web.Controllers
{
    [EnableCors("AllowAnyOrigin")]
    [Authorize]
    public class MeetingController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IUow uow;
        private readonly IConfiguration configuration;
        public MeetingController(IHttpClientFactory httpClientFactory, IUow uow, IConfiguration configuration)
        {
            this.httpClientFactory = httpClientFactory;
            this.uow = uow;
            this.configuration = configuration;
        }


        [Authorize(Roles ="psk")]
        public async Task<IActionResult> CrateMeeting(int id)
        {
            var apikey = configuration.GetValue<string>("DailyApiKey");
          
            var appointment = await uow.GetRepository<Appointment>()
                .GetQueryable()
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            if (appointment == null)
            {
                return StatusCode(404);
            }

            var client = httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apikey);
            Random random = new Random();
            var guidname = random.Next(1, 90000001).ToString();

            appointment.MeetingLink = guidname;

            var postparametres = new
            {
                name = guidname,
              
                properties = new
                {
                    max_participants = 2,
                    nbf = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    exp = DateTimeOffset.UtcNow.AddMinutes(60).ToUnixTimeSeconds(),
                    enable_people_ui = true,
                    eject_at_room_exp = true,
                    enable_screenshare = false,
                    lang = "tr",
                    enable_chat = true
                }
            };
            var requestdata = JsonSerializer.Serialize(postparametres);
            var content = new StringContent(requestdata, Encoding.UTF8, "application/json");
            var roomdetatil = await client.PostAsync("https://api.daily.co/v1/rooms", content);

            if (roomdetatil.IsSuccessStatusCode)
            {

                uow.GetRepository<Appointment>().Update(appointment);
                await uow.CommitAsync();

                return StatusCode(200);

            }


            return StatusCode(404);


        }

        public async Task<IActionResult> JoingMeeting(int id)
        {

            var appointment = await uow.GetRepository<Appointment>()
                .GetQueryable().Where(x => x.Id == id)
                .Include(x => x.Order)
                .ThenInclude(x => x.AppUser)
                .AsNoTrackingWithIdentityResolution()
                .FirstOrDefaultAsync(); 

            if (appointment == null)
            {
                TempData["join_error"] = "randevu bulunamadı";
                return Redirect("/");
            }
            if (DateTime.Now > appointment.AppointmentDate.AddMinutes(65)  )
            {
                TempData["join_timeout"] = "asd";
                return Redirect("/");

            }
           

            var userid = User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault()!.Value;

            if (userid == appointment.Order.AppUser.Id || userid == appointment.PsychologistId)
            {
                ViewBag.meetingUrl = appointment.MeetingLink;
                return View();
            }

            TempData["join_error"] = "randevu bulunamadı";
            return Redirect("/");

            //appointment id için
            //appointmen bul
            //user üzerinden username al
            //appointment'ın psk id'si bu id'ye eşitse girişe izin var
            //giren kişi user olabilir  user üzerinden username al
            //appointment üzerinden order'a git ve bu orderin appuser'ı ile 
            //user'ın username veya id'lerini kontrol et

            //viewbag olarak meetinkodunu gönder
            //iframe içinde kullanılacak link'e ekle ....daily.co/roomt/{meetinkodu}




        }

        public async Task<IActionResult> ControlerMeetingStatus(int id)
        {

            var appointment = await uow.GetRepository<Appointment>()
               .GetQueryable().Where(x => x.Id == id)
               .Include(x => x.Order)
               .ThenInclude(x => x.AppUser)
               .FirstOrDefaultAsync();

            if (appointment == null)
            {

                return StatusCode(404);
            }


            var userid = User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault()!.Value;

            if (userid == appointment.Order.AppUser.Id || userid == appointment.PsychologistId)
            {
                if (appointment.MeetingLink != null)
                {
                    return StatusCode(200);
                }
                else
                {
                    return StatusCode(404);
                }
            }


            //gelen appointmentın order table karşılığını bul
            //order table appuser üzerinden üzer id al
            //istek üzerinden USER üzerinden userid ile bir üst satırdaki useridleri karşılaştır

            //daha sonra meeting id boşmu dolumu kontrol et
            //dolu ise ok
            //değil ise notfound gönder

            //kullanıcı taraındada da /meeting/JoingMeeting/id => appointment şeklidne generate et doluysa

            return StatusCode(404);
        }


        [Authorize(Roles = "psk")]
        public async Task<IActionResult> CompletedMeeting(int id)
        {
            var appointment = await uow.GetRepository<Appointment>()
              .GetQueryable().Where(x => x.Id == id)
              .Include(x => x.Order)
              .ThenInclude(x => x.AppUser)
              .FirstOrDefaultAsync();

            if (appointment == null)
            {
                TempData["join_error"] = "randevu bulunamadı";
                return Redirect("/");
            }
             appointment.Status = (int)AppointmentEnum.Completed;

            uow.GetRepository<Appointment>().Update(appointment);
            await uow.CommitAsync();

            return Redirect("/Psychologist/PsychologistDashBoard");
        }
        [Authorize(Roles = "psk")]
        public async Task<IActionResult> RoomDelete(string id)
        {

            var apikey = configuration.GetValue<string>("DailyApiKey");
            var client = httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apikey);

            var result = await client.DeleteAsync($"https://api.daily.co/v1/rooms/{id}");
            if (result.IsSuccessStatusCode)
            {
                return StatusCode(200);

            }
            return StatusCode(404);
        }
    }
}

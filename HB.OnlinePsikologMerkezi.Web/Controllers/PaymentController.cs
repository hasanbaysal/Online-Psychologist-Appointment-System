using AspNetCoreHero.ToastNotification.Abstractions;
using AutoMapper;
using HB.OnlinePsikologMerkezi.Business.Services;
using HB.OnlinePsikologMerkezi.Common.CustomEnums;
using HB.OnlinePsikologMerkezi.Common.Mail;
using HB.OnlinePsikologMerkezi.Data.Interface;
using HB.OnlinePsikologMerkezi.Dto.Dtos;
using HB.OnlinePsikologMerkezi.Entities.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HB.OnlinePsikologMerkezi.Web.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IUow uow;
        private readonly IMapper mapper;
        private readonly IPaymentService paymentService;
        private readonly INotyfService _notyf;
        private readonly UserManager<AppUser> userManager;
        private readonly IMailService mailService;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly HttpClient httpClient;
        private readonly IConfiguration configuration;

        public PaymentController(IUow uow, IMapper mapper, IPaymentService paymentService, INotyfService notyf, UserManager<AppUser> userManager, IMailService mailService, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            this.uow = uow;
            this.mapper = mapper;
            this.paymentService = paymentService;
            _notyf = notyf;
            this.userManager = userManager;
            this.mailService = mailService;
            this.httpClient = httpClientFactory.CreateClient();
            this.configuration = configuration;
        }

        public async Task<IActionResult> BuyAppointment(int id)
        {


            var customerName = User.Identity.Name;

            var customerInfo = await userManager.FindByNameAsync(customerName);


            ViewBag.Name = customerInfo.Name ?? " ";
            ViewBag.LasName = customerInfo.LastName ?? " ";
            ViewBag.Email = customerInfo.Email ?? " ";
            ViewBag.PhoneNumber = customerInfo.PhoneNumber ?? " ";


            var msg = string.Empty;

            if (customerInfo.Name == null)
            {
                msg += " Ad,";
            }

            if (customerInfo.LastName == null)
            {
                msg += " SoyAd,";
            }


            if (customerInfo.PhoneNumber == null)
            {
                msg += " Telefon";
            }

            if (!string.IsNullOrEmpty(msg))
            {
                msg += " Bilgileriniz eksik satın alma öncesinde bilgilerim alanından güncelleyiniz.";
                _notyf.Warning(msg);
            }
            

            var data = await uow.GetRepository<Appointment>()
                .GetQueryable()
                .Where(x => x.Id == id)
                .Include(x => x.Psychologist)
                .ThenInclude(x => x.AppUser)
                .AsNoTrackingWithIdentityResolution()
                .FirstOrDefaultAsync();


            var mappeddata = mapper.Map<AppointmentListDto>(data);

            if (mappeddata == null)
            {

                return Redirect("/");
            }
            if (mappeddata.Status != (int)AppointmentEnum.new_appointment)
            {
                return RedirectToAction("PsychologistProfile", "Psychologist", new { id = mappeddata.Psychologist.SecondKey });
            }

            ViewBag.productDetails = mappeddata;
            ViewBag.pskid = mappeddata.Psychologist.SecondKey;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> BuyAppointment(PaymentInformationDto dto)
        {

            if (!ModelState.IsValid)
            {
                _notyf.Warning("ödeme bilgilerinizi gözden geçirin");

                var data = await uow.GetRepository<Appointment>()
                    .GetQueryable()
                    .Where(x => x.Id == dto.appointmentID)
                    .Include(x => x.Psychologist)
                    .ThenInclude(x => x.AppUser)
                    .AsNoTrackingWithIdentityResolution()
                    .FirstOrDefaultAsync();


                var mappeddata = mapper.Map<AppointmentListDto>(data);



                ViewBag.productDetails = mappeddata;
                ViewBag.pskid = mappeddata.Psychologist.SecondKey;

                return View(dto);
            }

            var status = await paymentService.CheckAppointmentStatus(dto.appointmentID);

            if (!status)
            {
                TempData["appointment_conccureny"] = "x";
                return RedirectToAction("PsychologistProfile", "Psychologist", new { id = dto.seccondkey });
            }


            var username = User!.Identity!.Name;

            var parts = dto.TotalDate.Split('/');
            dto.ExpireMonth = parts[0];
            dto.ExpireYear = parts[1];

            var result = await paymentService.BeginPayment3D(dto, username, dto.appointmentID);

            if (result.ResponseType == Common.CustomResponse.ResponseType.Fail)
            {
                TempData["appointment_conccureny"] = "x";
                return RedirectToAction("PsychologistProfile", "Psychologist", new { id = dto.seccondkey });
            }


            if (result.ResponseType == Common.CustomResponse.ResponseType.paymenterror)
            {

                _notyf.Warning("ödeme bilgilerinizi gözden geçirin");
                _notyf.Error("ödeme başarısız");




                var data = await uow.GetRepository<Appointment>()
                    .GetQueryable()
                    .Where(x => x.Id == dto.appointmentID)
                    .Include(x => x.Psychologist)
                    .ThenInclude(x => x.AppUser)
                    .AsNoTrackingWithIdentityResolution()
                    .FirstOrDefaultAsync();


                var mappeddata = mapper.Map<AppointmentListDto>(data);



                ViewBag.productDetails = mappeddata;
                ViewBag.pskid = mappeddata.Psychologist.SecondKey;

                return View(dto);
            }




            TempData["htmlcontent"] = result.Message;

            return RedirectToAction("Code3d");




        }

        public async Task<IActionResult> Code3d()
        {

            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Control3d()
        {


            var conversationid = Request.Form["conversationId"].ToString();
            var paymentID = Request.Form["paymentId"].ToString();
            if (conversationid == null || paymentID == null)
            {
                TempData["3d"] = "hata";
                return Redirect("/");
            }

            var control3doperation = paymentService.Control3dSecureResult(paymentID);


            if (control3doperation)
            {
                var result = await paymentService.Complate3dSecureOperation(conversationid, paymentID);
                if (!result)
                {
                    if (conversationid == null || paymentID == null)
                    {

                        TempData["3d"] = "hata";


                        return Redirect("/");
                    }
                }
                else
                {
                    TempData["3dok"] = "asd";


                    //sms mail işlemleri bu alanda yapılacak

                    var pskdetail = await uow.GetRepository<Appointment>()
                        .GetQueryable()
                        .Where(x => x.ThirdPartyPaymentId == paymentID)
                        .Include(x => x.Psychologist)
                        .ThenInclude(x => x.AppUser)
                        .FirstOrDefaultAsync();



                    var userDetatils = await userManager.FindByIdAsync(pskdetail.CustomerId);



                    //#region sms ve mail gönderimi


                    await mailService.SendMail(MailType.Notification, userDetatils.Email, UserOrderMail(pskdetail, userDetatils, pskdetail.Psychologist), "Randevu Bilgileri");

                    await mailService.SendMail(MailType.Notification, pskdetail.Psychologist.AppUser.Email, PskOrderMail(pskdetail, userDetatils, pskdetail.Psychologist), "Randevu Bilgileri");


                    //"NetGsmUserCode": "8508400106",
                    //  "NetGsmPassword": "x7.34f77",


                    var NetGsmUserCode = configuration.GetValue<string>("NetGsmUserCode");
                    var NetGsmPassword = configuration.GetValue<string>("NetGsmPassword");
                    var NetGsmMessageHeader = configuration.GetValue<string>("NetGsmMessageHeader");




                    if (userDetatils.PhoneNumber != null)
                    {


                        //user sms gönder 


                        var userphoneNumer = userDetatils.PhoneNumber.Substring(1);




                        var formData = new Dictionary<string, string>
                            {
                                { "usercode", NetGsmUserCode },
                                { "password", NetGsmPassword },
                                { "gsmno", userphoneNumer },
                                { "message", $"Randevu tarihiniz : {pskdetail.AppointmentDate.ToString("dddd, dd MMMM yyyy HH:mm:ss")} Randevularım alanından diğer randevu bilgilerine ulaşabilirsiniz." },
                                { "msgheader", NetGsmMessageHeader }
                            };



                        var content = new FormUrlEncodedContent(formData);

                        var response = await httpClient.PostAsync("https://api.netgsm.com.tr/sms/send/get/", content);
                        var responseData = await response.Content.ReadAsStringAsync();

                        Console.WriteLine("SMS gönderme cevabı: " + responseData);
                    }



                    if (pskdetail.Psychologist.AppUser.PhoneNumber != null)
                    {
                        var pskphone = pskdetail.Psychologist.AppUser.PhoneNumber.Substring(1);

                        var formData = new Dictionary<string, string>
                            {
                                { "usercode", NetGsmUserCode },
                                { "password", NetGsmPassword },
                                { "gsmno", pskphone},
                                { "message", $"{pskdetail.AppointmentDate.ToString("dddd, dd MMMM yyyy HH:mm:ss")} Tarihli randevunuz satın alınmıştır" },
                                { "msgheader", NetGsmMessageHeader }
                            };



                        var content = new FormUrlEncodedContent(formData);

                        var response = await httpClient.PostAsync("https://api.netgsm.com.tr/sms/send/get/", content);
                        var responseData = await response.Content.ReadAsStringAsync();

                        Console.WriteLine("SMS gönderme cevabı: " + responseData);
                    }




                    //#endregion




                    //başarılı işlem!!! profile yönlendir
                    return Redirect("/");

                }
            }


            TempData["3d"] = "hata";
            return Redirect("/");
        }


        private string UserOrderMail(Appointment apo, AppUser user, Psychologist psk)
        {


            return @$"




                    <html>
                    <head>
                        <meta charset=""UTF-8"">
                        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                        <title>Document</title>
                    </head>
                    <body style=""background-color:#f6f6f6; padding: 20px;"">
    
                    <div style=""padding: 15px; height: 30px; text-align:center; color:white; background:rgb(2,0,36);
                    background: linear-gradient(90deg,rgba(2,0,36,1) 0%,rgba(9,9,121,1) 35%,rgba(0,212,255,1) 100%);"">
    
                     <h2 style=""margin: 0px; padding: 0px;"">Online Psikolog Merkezi</p>

                    </div>

                    <div style=""background-color:#ffffff; padding: 30px; height: 430px; text-align:center;"">

                     <img style=""width: 150px;  object-fit: cover;"" src=""https://onlinepsikologmerkezi.com/assets/img/singup-image.jpg"">


                        <p>Merhaba Sayın <span style=""font-weight:bold;"" >  {user.Name + " " + user.LastName} </span></p>
                        <hr>
                        <p>{DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss")} tarihinde satın almış olduğunuz randevu bilgisi bize ulaştı</p>
                        <p>{psk.Title + " " + psk.AppUser.Name + " " + psk.AppUser.LastName} ile görüşmenizi sitemiz üzerinden gerçekleştirebilirsiniz
                        </p>      
                        <p><span style=""font-weight:bold;"" >  Randevu tarihiniz </span> : {apo.AppointmentDate.ToString("dddd, dd MMMM yyyy HH:mm:ss")}</p>
                        <p><span style=""font-weight:bold;"" >  Randevu ücret </span> :  {apo.Price}</p>
                        <p><span style=""font-weight:bold;"" >  Psikolog  </span> : {psk.AppUser.Name + " " + psk.AppUser.LastName}</p>
  
                        <p>
                            Randevularım alanında satın aldığınız görüşmeyi görüntülüyebilirsiniz. Görüşme başlamadan 5 dakika önce toplantıya katılın butonu aktif olacaktır.

                        </p>
                        <br>



                    </div>

                    </body>
                    </html>


              
                    ";
        }
        private string PskOrderMail(Appointment apo, AppUser user, Psychologist psk)
        {
            return @$"
                      
                        <html>
                        <head>
                            <meta charset=""UTF-8"">
                            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                            <title>Document</title>
                        </head>
                        <body style=""background-color:#f6f6f6; padding: 20px;"">
    
                        <div style=""padding: 15px; height: 30px; text-align:center; color:white; background:rgb(2,0,36);
                        background: linear-gradient(90deg,rgba(2,0,36,1) 0%,rgba(9,9,121,1) 35%,rgba(0,212,255,1) 100%);"">
    
                         <h2 style=""margin: 0px; padding: 0px;"">Online Psikolog Merkezi</p>

                        </div>

                        <div style=""background-color:#ffffff; padding: 30px; height: 400px; text-align:center;"">

                        <img style=""width: 150px;  object-fit: cover;"" src=""https://onlinepsikologmerkezi.com/assets/img/singup-image.jpg"">

                            <p>Merhaba Sayın {psk.AppUser.Name + " " + psk.AppUser.LastName}</p>
                      
                            <p>{apo.AppointmentDate.ToString("dddd, dd MMMM yyyy HH:mm:ss")} Tarihli Randevunuz Satın Alındı</p>  
    
                         <p>
                             Psikolog panelinizden Görüşmeyi başlatabilirsiniz. Görüşmeyi Başlat butonu randevu zamanından 5 dakika önce aktif olacaktır.

                         </p>
   

                        </div>

                        </body>
                        </html>


                    ";
        }



    }
}

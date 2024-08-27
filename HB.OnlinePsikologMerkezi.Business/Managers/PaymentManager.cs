using HB.OnlinePsikologMerkezi.Business.Services;
using HB.OnlinePsikologMerkezi.Common.CustomEnums;
using HB.OnlinePsikologMerkezi.Common.CustomResponse;
using HB.OnlinePsikologMerkezi.Common.Defaults;
using HB.OnlinePsikologMerkezi.Common.Mail;
using HB.OnlinePsikologMerkezi.Data.Interface;
using HB.OnlinePsikologMerkezi.Dto.Dtos;
using HB.OnlinePsikologMerkezi.Entities.Entities;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace HB.OnlinePsikologMerkezi.Business.Managers
{
    public class PaymentManager : IPaymentService
    {
        private readonly UserManager<AppUser> userManager;
        private readonly IUow uow;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMailService mailService;
        private readonly IConfiguration configuration;
        public PaymentManager(UserManager<AppUser> userManager, IUow uow, IHttpContextAccessor httpContextAccessor, IMailService mailService, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.uow = uow;
            _httpContextAccessor = httpContextAccessor;
            this.mailService = mailService;
            this.configuration = configuration;

          
        }

 

        //dönenpayment ıd' değerini sakla
        public async Task<Response<NoDataResponse>> BeginPayment(PaymentInformationDto paymentDto, string userName, int appointmentId)
        {

            //payment information validation -----> bu kısım atlandı!!! 

            //satın alacak kişi bilgisi lazım ++
            //satın alınacak randevu id ++

            //satın alma status kontrol +++++

            //satın alma statusu yap => concurrency exception ile ++++

            // satın alma başlat +++++ 
            // işlem başarılı ise  => psikolog ve user'a mesaj at -----> bu kısım atlandı!!! 
            // işlem başarılı değilse statusu düzelt ++++

            var user = await userManager.FindByNameAsync(userName);

            if (user == null)
            {
                return new Response<NoDataResponse>(ResponseType.Fail, "kullanıcı bulunamadı hata");
            }

            var appointment = await uow.GetRepository<Appointment>().GetByFilterAsync(x => x.Id == appointmentId);

            if (appointment == null)
            {
                return new Response<NoDataResponse>(ResponseType.Fail, "randevu bulunamadı hata");
            }


            if (appointment.Status != (int)AppointmentEnum.new_appointment)
            {
                return new Response<NoDataResponse>(ResponseType.Fail, "bu randevu başka biri tarafından satın alınıyor");
            }


            appointment.Status = (int)AppointmentEnum.being_bought;


            var repository = uow.GetRepository<Appointment>();

            repository.Update(appointment);

            try
            {
                await uow.CommitAsync();
            }
            catch (Exception)
            {
                return new Response<NoDataResponse>(ResponseType.Fail, "bu randevu başka biri tarafından satın alınıyor");

            }



            #region ödeme bilgileri
            CreatePaymentRequest request = new CreatePaymentRequest();
            request.Locale = Locale.TR.ToString();
            request.Price = appointment.Price.ToString();
            request.PaidPrice = appointment.Price.ToString();
            request.Currency = Currency.TRY.ToString();
            request.Installment = 1;
            request.BasketId = appointment.Id.ToString();


            PaymentCard paymentCard = new PaymentCard();
            paymentCard.CardNumber = paymentDto.CardNumbet.ToString();
            paymentCard.ExpireMonth = paymentDto.ExpireMonth.ToString();
            paymentCard.ExpireYear = paymentDto.ExpireYear.ToString();
            paymentCard.Cvc = paymentDto.Cvc.ToString();
            paymentCard.CardHolderName = paymentDto.NameOnTheCard.ToString();

            request.PaymentCard = paymentCard;


            Buyer buyer = new Buyer();
            buyer.Id = user.Id;
            buyer.Name = user.Name ?? "belirtilmedi";
            buyer.Surname = user.Name ?? "belirtilmedi";
            buyer.IdentityNumber = "belirtilmedi"; //alıcı tc gereksiz
            buyer.City = "belirtilmedi"; //alıcı şehir gereksiz
            buyer.Country = "belirtilmedi"; //alıcı ülke gereksiz
            buyer.Email = user.Email; //alıcı email
            buyer.GsmNumber = user.PhoneNumber ?? "belirtilmedi";
            buyer.Ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? " ";
            buyer.RegistrationAddress = "belirtilmedi";
            request.Buyer = buyer;


            Address billingAddress = new Address();

            billingAddress.ContactName = (user.UserName ?? "belirtilmedi") + " " + (user.LastName ?? "belirtilmedi");

            billingAddress.City = "belirtilmedi";
            billingAddress.Country = "belirtilmedi";
            billingAddress.Description = "belirtilmedi";
            billingAddress.ZipCode = "belirtilmedi";
            request.BillingAddress = billingAddress;


            Address shippingAddress = new Address();
            shippingAddress.ContactName = (user.UserName ?? "belirtilmedi") + " " + (user.LastName ?? "belirtilmedi");
            shippingAddress.City = "belirtilmedi";
            shippingAddress.Country = "belirtilmedi";
            shippingAddress.Description = "belirtilmedi";
            shippingAddress.ZipCode = "belirtilmedi";
            request.ShippingAddress = shippingAddress;


            List<BasketItem> basketItems = new List<BasketItem>();
            BasketItem firstBasketItem = new BasketItem();
            firstBasketItem.Id = appointment.Id.ToString();
            firstBasketItem.Name = "online randevu";
            firstBasketItem.Category1 = "online randevu hizmeti";
            firstBasketItem.ItemType = BasketItemType.VIRTUAL.ToString();
            firstBasketItem.Price = appointment.Price.ToString();
            basketItems.Add(firstBasketItem);
            request.BasketItems = basketItems;

            #endregion


            Payment payment = Iyzipay.Model.Payment.Create(request, new Iyzipay.Options() { ApiKey = "sandbox-bT7hQkkfKkbuaGhE4D6bFyzYVTF6PyI2", SecretKey = "sandbox-uiA8qTSYm49hzTfKsIUMInVZQNJc0H7Z", BaseUrl = "https://sandbox-api.iyzipay.com" });



            RetrievePaymentRequest paymentCheckProperties = new RetrievePaymentRequest();
            paymentCheckProperties.Locale = Locale.TR.ToString();

            paymentCheckProperties.PaymentId = payment.PaymentId;

            Payment paymentResult = Iyzipay.Model.Payment.Retrieve(paymentCheckProperties, new Iyzipay.Options() { ApiKey = "sandbox-bT7hQkkfKkbuaGhE4D6bFyzYVTF6PyI2", SecretKey = "sandbox-uiA8qTSYm49hzTfKsIUMInVZQNJc0H7Z", BaseUrl = "https://sandbox-api.iyzipay.com" });

            if (paymentResult.Status == "success")
            {

                //statusu satıldıya çek
                appointment.Status = (int)AppointmentEnum.sold;

                repository.Update(appointment);

                //order table'a ekle

                uow.GetRepository<Order>().Add(new Order() { AppointmentId = appointment.Id, DurchaseDate = DateTime.Now, AppUserId = user.Id, Price = appointment.Price });


                await uow.CommitAsync();

                //psikolog ve user'a bildirim gönder eksik



                var psk = await uow.GetRepository<Psychologist>().GetQueryable().Where(x => x.Psychologist_ID == appointment.PsychologistId).Include(x => x.AppUser).FirstOrDefaultAsync();

                var pskEmail = psk!.AppUser.Email;
                var useremail = user.Email;

                await mailService.SendMail(MailType.Notification, useremail, UserOrderMail(appointment, user, psk), "Randevu Bilgileri");
                await mailService.SendMail(MailType.Notification, pskEmail, PskOrderMail(appointment, user, psk), "Randevu Bilgileri");





                return new Response<NoDataResponse>(ResponseType.Success);


            }

            //ödeme başarızsa statusu geri eski haline al
            appointment.Status = (int)AppointmentEnum.new_appointment;
            await uow.CommitAsync();

            return new Response<NoDataResponse>(ResponseType.paymenterror, paymentResult.ErrorMessage);
        }

        public async Task<Response<NoDataResponse>> BeginPayment3D(PaymentInformationDto paymentDto, string userName, int appointmentId)
        {


            var uniqueueconversationId = Guid.NewGuid().ToString();
            var crequest = _httpContextAccessor!.HttpContext!.Request;
            var baseUrl = $"{crequest.Scheme}://{crequest.Host.Value}";
            var uriBuilder = new UriBuilder(baseUrl);

            uriBuilder.Path = "/Payment/Control3d";

            var myCallBackUrl = uriBuilder.ToString(); 

      

            var user = await userManager.FindByNameAsync(userName);

            if (user == null)
            {
                return new Response<NoDataResponse>(ResponseType.Fail, "kullanıcı bulunamadı hata");
            }

            var appointment = await uow.GetRepository<Appointment>().GetByFilterAsync(x => x.Id == appointmentId);

            if (appointment == null)
            {
                return new Response<NoDataResponse>(ResponseType.Fail, "randevu bulunamadı hata");
            }


            if (appointment.Status != (int)AppointmentEnum.new_appointment)
            {
                return new Response<NoDataResponse>(ResponseType.Fail, "bu randevu başka biri tarafından satın alınıyor");
            }


            appointment.Status = (int)AppointmentEnum.being_bought;


            var repository = uow.GetRepository<Appointment>();

            repository.Update(appointment);

            try
            {
                await uow.CommitAsync();
            }
            catch (Exception)
            {
                return new Response<NoDataResponse>(ResponseType.Fail, "bu randevu başka biri tarafından satın alınıyor");

            }



            #region ödeme bilgileri
            CreatePaymentRequest request = new CreatePaymentRequest();
            request.Locale = Locale.TR.ToString();
            request.Price = appointment.Price.ToString();
            request.PaidPrice = appointment.Price.ToString();
            request.Currency = Currency.TRY.ToString();
            request.Installment = 1;
            request.BasketId = appointment.Id.ToString();

            request.CallbackUrl = myCallBackUrl;
            request.ConversationId = uniqueueconversationId;

            PaymentCard paymentCard = new PaymentCard();
            paymentCard.CardNumber = paymentDto.CardNumbet.ToString();
            paymentCard.ExpireMonth = paymentDto.ExpireMonth.ToString();
            paymentCard.ExpireYear = paymentDto.ExpireYear.ToString();
            paymentCard.Cvc = paymentDto.Cvc.ToString();
            paymentCard.CardHolderName = paymentDto.NameOnTheCard.ToString();

            request.PaymentCard = paymentCard;


            Buyer buyer = new Buyer();
            buyer.Id = user.Id;
            buyer.Name = user.Name ?? "belirtilmedi";
            buyer.Surname = user.LastName ?? "belirtilmedi";
            buyer.IdentityNumber = "belirtilmedi"; //alıcı tc gereksiz
            buyer.City = "belirtilmedi"; //alıcı şehir gereksiz
            buyer.Country = "belirtilmedi"; //alıcı ülke gereksiz
            buyer.Email = user.Email; //alıcı email
            buyer.GsmNumber = user.PhoneNumber ?? "belirtilmedi";
            buyer.Ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? " ";
            buyer.RegistrationAddress = "belirtilmedi";
            request.Buyer = buyer;



            Address billingAddress = new Address();

            billingAddress.ContactName = (user.UserName ?? "belirtilmedi") + " " + (user.LastName ?? "belirtilmedi");

            billingAddress.City = "belirtilmedi";
            billingAddress.Country = "belirtilmedi";
            billingAddress.Description = "belirtilmedi";
            billingAddress.ZipCode = "belirtilmedi";
            request.BillingAddress = billingAddress;


            Address shippingAddress = new Address();
            shippingAddress.ContactName = (user.UserName ?? "belirtilmedi") + " " + (user.LastName ?? "belirtilmedi");
            shippingAddress.City = "belirtilmedi";
            shippingAddress.Country = "belirtilmedi";
            shippingAddress.Description = "belirtilmedi";
            shippingAddress.ZipCode = "belirtilmedi";
            request.ShippingAddress = shippingAddress;


            List<BasketItem> basketItems = new List<BasketItem>();
            BasketItem firstBasketItem = new BasketItem();
            firstBasketItem.Id = appointment.Id.ToString();
            firstBasketItem.Name = "online randevu";
            firstBasketItem.Category1 = "online randevu hizmeti";
            firstBasketItem.ItemType = BasketItemType.VIRTUAL.ToString();
            firstBasketItem.Price = appointment.Price.ToString();
            basketItems.Add(firstBasketItem);
            request.BasketItems = basketItems;

            #endregion





            ThreedsInitialize threedsInitialize = ThreedsInitialize.Create(request, new Iyzipay.Options() { ApiKey = PaymentStringDefault.IyzicoApiKey, SecretKey = PaymentStringDefault.IyzicoApiSecret, BaseUrl = PaymentStringDefault.IyzicoApiBaseUrl });
            
            //ThreedsInitialize threedsInitialize = ThreedsInitialize.Create(request, new Iyzipay.Options() { ApiKey = "sandbox-bT7hQkkfKkbuaGhE4D6bFyzYVTF6PyI2", SecretKey = "sandbox-uiA8qTSYm49hzTfKsIUMInVZQNJc0H7Z", BaseUrl = "https://sandbox-api.iyzipay.com" });


            if (threedsInitialize.Status == Status.SUCCESS.ToString())
            {
                //html contenti gönder +
                //appointment içine converstion id basss +
                //appointment içine userid bass +
                //appointmen statusu s3dCheck yap +

                appointment.Status = (int)AppointmentEnum.s3dCheck;
                appointment.CustomerId = user.Id;
                appointment.ConversationId = uniqueueconversationId;
                appointment.Start3DTime = DateTime.Now;
                
                repository.Update(appointment);
                await uow.CommitAsync();

                return new Response<NoDataResponse>(ResponseType.Success,threedsInitialize.HtmlContent);

            }

            appointment.Status = (int)AppointmentEnum.new_appointment;
            await uow.CommitAsync();

            return new Response<NoDataResponse>(ResponseType.paymenterror);
        }

        public bool Control3dSecureResult(string paymentid)
        {
            CreateThreedsPaymentRequest request = new CreateThreedsPaymentRequest();
            request.PaymentId = paymentid;
            ThreedsPayment threedsPayment = ThreedsPayment.Create(request, new Iyzipay.Options() { ApiKey = PaymentStringDefault.IyzicoApiKey, SecretKey = PaymentStringDefault.IyzicoApiSecret, BaseUrl = PaymentStringDefault.IyzicoApiBaseUrl });

            if (threedsPayment.Status == Status.SUCCESS.ToString())
            {

                //uow.GetRepository<Appointment>().GetQueryable().Where(x=>x.)

                return true;

            }

            return false;
        }
        public async Task<bool> Complate3dSecureOperation(string conversationId,string paymentid)
        {
            //conversion id
            //appointmen içindeki conversionid ara
            //bulunca
            //appointmenti order olarak user'a ekle
            

            


            var repo = uow.GetRepository<Appointment>();


            var appointment = await repo.GetQueryable()
                .Where(x => x.ConversationId == conversationId)
                .FirstOrDefaultAsync();
            if (appointment == null)
            {
                return false;
            }

            appointment.Status = (int)AppointmentEnum.sold;
            appointment.ThirdPartyPaymentId = paymentid;
            repo.Update(appointment);

            uow.GetRepository<Order>().Add(new Order() { AppointmentId = appointment.Id, DurchaseDate = DateTime.Now, AppUserId = appointment!.CustomerId!, Price = appointment.Price });

            await uow.CommitAsync();

            //try catch içinde
            //psikologa mesaj mail + sms
            //müşteriye mesaj at mail + sms 


            var user = await userManager.FindByIdAsync(appointment.CustomerId);

            var psk = await uow.GetRepository<Psychologist>().GetQueryable().Where(x => x.Psychologist_ID == appointment.PsychologistId).Include(x => x.AppUser).FirstOrDefaultAsync();

            var pskEmail = psk!.AppUser.Email;
            var useremail = user.Email;

            // bu kısım iptal

            //await mailService.SendMail(MailType.Notification, useremail, UserOrderMail(appointment, user, psk), "Randevu Bilgileri");
            //await mailService.SendMail(MailType.Notification, pskEmail, PskOrderMail(appointment, user, psk), "Randevu Bilgileri");



            return true;

        }

        public async Task<bool> CheckAppointmentStatus(int id)
        {
            var data = await uow.GetRepository<Appointment>().GetByFilterAsync(x => x.Id == id);

            if (data == null)
            {
                return false;
            }
            if (data.AppointmentDate < DateTime.Now)
            {
                return false;
            }

            return data.Status == (int)AppointmentEnum.new_appointment;
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

                    <div style=""background-color:#ffffff; padding: 30px; height: 270px; text-align:center;"">



                        <p>Merhaba Sayın <span style=""font-weight:bold;"" >  {user.Name +  " " + user.LastName} </span></p>
                        <hr>
                        <p>{DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss")} tarihinde satın almış olduğunuz randevu bilgisi bize ulaştı</p>
                        <p>{ psk.Title+" "+psk.AppUser.Name + " "  + psk.AppUser.LastName} ile görüşmenizi sitemiz üzerinden gerçekleştirebilirsiniz
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

                        <div style=""background-color:#ffffff; padding: 30px; height: 170px; text-align:center;"">


                            <p>Merhaba Sayın {psk.AppUser.Name + " " + psk.AppUser.LastName}</p>
                      
                            <p>{apo.AppointmentDate.ToString("dddd, dd MMMM yyyy HH:mm:ss")} Tarihli Randevunuz Satın Aldındı</p>  
    
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

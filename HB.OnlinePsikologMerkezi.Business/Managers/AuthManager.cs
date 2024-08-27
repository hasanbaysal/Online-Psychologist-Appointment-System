using FluentValidation;
using HB.OnlinePsikologMerkezi.Business.Extentions;
using HB.OnlinePsikologMerkezi.Business.Helpers;
using HB.OnlinePsikologMerkezi.Business.Services;
using HB.OnlinePsikologMerkezi.Common.CustomResponse;
using HB.OnlinePsikologMerkezi.Common.Mail;
using HB.OnlinePsikologMerkezi.Dto.Dtos;
using HB.OnlinePsikologMerkezi.Entities.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Security.Claims;
using System.Web;

namespace HB.OnlinePsikologMerkezi.Business.Managers
{
    public class AuthManager : IAuthService
    {


        private readonly IValidator<UserSignUpDto> _userSignUpDtoValidator;
        private readonly UserManager<AppUser> userManager;
        private readonly IMailService mailService;
        private readonly ILinkHelper linkHelper;
        private readonly SignInManager<AppUser> signInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IValidator<UserSigInDto> _userSingInDtoValidator;
        private readonly IValidator<ForgetPasswordDto> _forgetPasswordDtoValidator;
        private readonly IValidator<ResetPasswordDto> _resetPasswordDtoValidator;

        public AuthManager(
            IValidator<UserSignUpDto> userSignUpDtoValidator,
            UserManager<AppUser> userManager,
            IMailService mailService,
            ILinkHelper linkHelper,
            SignInManager<AppUser> signInManager,
            IHttpContextAccessor httpContextAccessor,
            IValidator<UserSigInDto> userSingInDtoValidator,
            IValidator<ForgetPasswordDto> forgetPasswordDtoValidator,
            IValidator<ResetPasswordDto> resetPasswordDtoValidator)

        {
            _userSignUpDtoValidator = userSignUpDtoValidator;
            this.userManager = userManager;
            this.mailService = mailService;
            this.linkHelper = linkHelper;
            this.signInManager = signInManager;
            _httpContextAccessor = httpContextAccessor;
            _userSingInDtoValidator = userSingInDtoValidator;
            _forgetPasswordDtoValidator = forgetPasswordDtoValidator;
            _resetPasswordDtoValidator = resetPasswordDtoValidator;
        }

        public async Task<Response<UserSignUpDto>> SingUpAsync(UserSignUpDto upDto)
        {

            var result = await _userSignUpDtoValidator.ValidateAsync(upDto);

            if (!result.IsValid)
            {
                return new Response<UserSignUpDto>(upDto, result.CustomErrorList());
            }

            var identityResult = await userManager.CreateAsync(new()
            { Email = upDto.Email, UserName = upDto.UserName }, upDto.Password);

            if (!identityResult.Succeeded)
            {

                return new Response<UserSignUpDto>(upDto, identityResult.IdentityErrorList());
            }

            var user = await userManager.FindByNameAsync(upDto.UserName);

            var emailToken = await userManager.GenerateEmailConfirmationTokenAsync(user);

            var mailLink = linkHelper.CrateLinkForActivationorResetPassword("Activation", "Account", user.Id, emailToken);


            try
            {
                await mailService.SendMail(MailType.Service, user.Email, ActivationMailBody(mailLink), "Hesap Aktivasyonu");
            }
            catch (SmtpFailedRecipientException e)
            {

                await userManager.DeleteAsync(user);
                return new Response<UserSignUpDto>(upDto, new() { new() { PropertyName = "Email", ErrorMessage = "Geçerli bir mail adresi giriniz" } });
            }
            catch (Exception e)
            {

                await userManager.DeleteAsync(user);
                return new Response<UserSignUpDto>(upDto, new() { new() { PropertyName = "Email", ErrorMessage = "Geçerli bir mail adresi giriniz" } });
            }


            return new Response<UserSignUpDto>(ResponseType.Success, "Kayıt başarılı aktivasyon bağlantısı e-posta adresinize iletildi");
        }
        public async Task<Response> AccountActivationAsync(string userid, string token)
        {

            if (string.IsNullOrEmpty(userid) || string.IsNullOrEmpty(token))
            {
                return new Response(ResponseType.NotFound, "Bağlantınız bozuk yada geçersiz");
            }

            var user = await userManager.FindByIdAsync(userid);

            if (user == null)
            {
                return new Response(ResponseType.NotFound, "Bağlantınız bozuk yada geçersiz");
            }
            var confirmResut = await userManager.ConfirmEmailAsync(user, token);
            if (!confirmResut.Succeeded)
            {
                if (user.EmailConfirmed)
                {
                    return new Response(ResponseType.NotFound, "Bağlantınız bozuk yada geçersiz");
                }


                var emailToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
                var mailLink = linkHelper.CrateLinkForActivationorResetPassword("Activation", "Account", user.Id, emailToken);
                await mailService.SendMail(MailType.Service, user.Email, ActivationMailBody(mailLink), "Hesap Aktivasyonu");
                return new Response(ResponseType.NotFound, "Bağlantınız bozuk yada geçersiz aktivasyon maili tekrar gönderilecektir");
            }

            await userManager.UpdateSecurityStampAsync(user);
            return new Response(ResponseType.Success, "Aktivasyon Başarılı Giriş Yapabilirisniz :) ");

        }
        public Task<ChallengeResult> LoginWithGoole(string redirectUrl)
        {
            var prop = signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
            return Task.FromResult<ChallengeResult>(new ChallengeResult("Google", prop));

        }
        public async Task<Response<NoDataResponse>> ExternalLogin(string redirectUrl = "/")
        {
            ExternalLoginInfo info = await signInManager.GetExternalLoginInfoAsync();

            if (info == null)
            {
                return new Response<NoDataResponse>(ResponseType.Fail, "Bir hata meydana geldi");
            }
            else
            {



                var result = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, true);

                if (result.Succeeded)
                {



                    var userBlockKontrol = await userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                    //block control

                    if (userBlockKontrol.isBlock)
                    {

                        await signInManager.SignOutAsync();

                        return new Response<NoDataResponse>(ResponseType.isBlock, "Bu hesap kalıcı olarak engellenmiştir. Bir yanlışlık olduğunu düşünüyorsanız yönetici ile iletişime geçiniz");
                    }



                    var user = await userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

                    user.LastLoginIpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? " ";
                    // Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? " ";

                    await userManager.UpdateAsync(user);

                    return new Response<NoDataResponse>(ResponseType.Success, redirectUrl);
                }
                else
                {
                    Random rnd = new();
                    AppUser user = new AppUser();
                    user.EmailConfirmed = true;

                    user.Email = info.Principal.FindFirst(ClaimTypes.Email)!.Value;
                    string externalUserID = info.Principal.FindFirst(ClaimTypes.NameIdentifier)!.Value;
                    user.UserName = "user" + rnd.NextInt64(100, 10000000);

                    //default şifre ekle ve mail gönder
                    //string password = ***
                    var password = CreateNewPassword();
                    IdentityResult registerReuslt = await userManager.CreateAsync(user, password);

                    if (registerReuslt.Succeeded)
                    {
                        await mailService.SendMail(MailType.Notification, user.Email, SignUpWithGoogleHelpMessage(password), "Hoş Geldin");

                        var loginResult = await userManager.AddLoginAsync(user, info);
                        if (loginResult.Succeeded)
                        {
                            user.LastLoginIpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? " ";
                            await userManager.UpdateAsync(user);
                            await signInManager.SignInAsync(user, true);

                            return new Response<NoDataResponse>(ResponseType.Success, redirectUrl);
                        }
                        else
                        {


                            return new Response<NoDataResponse>(new NoDataResponse(), registerReuslt.IdentityErrorList());
                        }
                    }
                    else
                    {
                        return new Response<NoDataResponse>(new NoDataResponse(), registerReuslt.IdentityErrorList());
                    }

                }

            }


        }
        public async Task<Response<UserSigInDto>> SingInWithPassword(UserSigInDto dto)
        {
            var validationResult = await _userSingInDtoValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                return new Response<UserSigInDto>(dto, validationResult.CustomErrorList());
            }
            //kullanıcı varlık kontrolü
            var user = await userManager.FindByEmailAsync(dto.Email);

            if (user == null)
            {
                return new Response<UserSigInDto>(ResponseType.NotFound, "Şifre ve E-Posta adresi hatalı");
            }


            //user block kontrol
            if (user.isBlock)
            {

                return new Response<UserSigInDto>(ResponseType.isBlock, "Bu hesap kalıcı olarak engellenmiştir. Bir yanlışlık olduğunu düşünüyorsanız yönetici ile iletişime geçiniz");
            }

            var signResult = await signInManager.PasswordSignInAsync(user, dto.Password, dto.RememberMe, true);
            if (signResult.IsNotAllowed)
            {
                return new Response<UserSigInDto>(ResponseType.IsnNotAllowed, "E-Posta adresinizi doğrulamanız. Aktivasyon bağlantısı E-Posta adresinize iletildi. Gelen kutunuzu veya spam kutunuzu kontrol ediniz");
            }
            if (signResult.IsLockedOut)
            {
                return new Response<UserSigInDto>(ResponseType.IsLockOut, "Çok fazla hatalı giriş denemesi yapıldı 3 dakika boyunca giriş yapılamaz");
            }
            if (!signResult.Succeeded)
            {
                return new Response<UserSigInDto>(ResponseType.Fail, "Şifre ve E-Posta adresi hatalı");
            }

            //son giriş ıp adresi alındır
            user.LastLoginIpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? " ";


            await userManager.UpdateAsync(user);


            //validasyon
            //kullanıcı bulunama
            //email activasyon durumu
            //kitlenmiş hesap
            //şifre yada email hatası

            //başarılı giriş


            return new Response<UserSigInDto>(ResponseType.Success);

        }

        //forget password=> email gönderme
        public async Task<Response<ForgetPasswordDto>> ForgetPasswordSendMail(ForgetPasswordDto dto)
        {
            var validationResult = _forgetPasswordDtoValidator.Validate(dto);
            if (!validationResult.IsValid)
            {
                return new Response<ForgetPasswordDto>(dto, validationResult.CustomErrorList());
            }

            var user = await userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                return new Response<ForgetPasswordDto>(ResponseType.NotFound, "Böyle bir e-posta bulunamadı");
            }

            string token = await userManager.GeneratePasswordResetTokenAsync(user);

            var resetLink = linkHelper.CrateLinkForActivationorResetPassword("ResetPassword", "Account", user.Id, token);

            await mailService.SendMail(MailType.Service, user.Email, ResetPasswordMailBody(resetLink), "Online Psikolog Merkezi Şifre Yenileme");

            return new Response<ForgetPasswordDto>(ResponseType.Success, "E-Posta adresinize şifre yenileme bağlantısı gönderildi");
        }


        //reset password => gelen şifreyi  yeni şifre yapmak
        //token ve userid'i tempdata içinde taşıyarak reset action'a gönder
        //orada tekrar her ihtimale karşı tekrar tempdata içinde al
        public async Task<bool> ConfrimUserIdAndResetToken(string userid, string token)
        {

            if (string.IsNullOrEmpty(userid) || string.IsNullOrEmpty(token))
            {
                return false;
            }
            userid = HttpUtility.UrlDecode(userid);
            var user = await userManager.FindByIdAsync(userid);
            if (user == null)
            {
                return false;
            }

            token = HttpUtility.UrlDecode(token);
            IdentityOptions options = new IdentityOptions();
            var result = await userManager.VerifyUserTokenAsync(user, options.Tokens.PasswordResetTokenProvider, UserManager<AppUser>.ResetPasswordTokenPurpose, token).ConfigureAwait(false);
            if (!result)
            {
                return false;
            }

            return true;
        }
        public async Task<Response<ResetPasswordDto>> ResetPassord(ResetPasswordDto dto, string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return new Response<ResetPasswordDto>(ResponseType.Fail, "Bağlantı bozuk yada geçersiz");
            }
            userId = HttpUtility.UrlDecode(userId);
            token = HttpUtility.UrlDecode(token);

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new Response<ResetPasswordDto>(ResponseType.Fail, "Bağlantı bozuk yada geçersiz");
            }
            IdentityOptions options = new IdentityOptions();
            var result = await userManager.VerifyUserTokenAsync(user, options.Tokens.PasswordResetTokenProvider, UserManager<AppUser>.ResetPasswordTokenPurpose, token).ConfigureAwait(false);

            if (!result)
            {
                return new Response<ResetPasswordDto>(ResponseType.Fail, "Bağlantı bozuk yada geçersiz");
            }

            var validationResult = _resetPasswordDtoValidator.Validate(dto);
            if (!validationResult.IsValid)
            {
                return new Response<ResetPasswordDto>(dto, validationResult.CustomErrorList());

            }
            var resetResult = await userManager.ResetPasswordAsync(user, token, dto.RepeatPassword);

            if (!resetResult.Succeeded)
            {
                return new Response<ResetPasswordDto>(ResponseType.Fail, "Bağlantı bozuk yada geçersiz");
            }

            await userManager.UpdateSecurityStampAsync(user);
            return new Response<ResetPasswordDto>(ResponseType.Success, "şifre başarıyla değiştirildi");

        }

        private string CreateNewPassword()
        {
            Random rnd = new Random();
            char[] Lower = "abcçdefgğhiıjklmnoöprsştuüvyz".ToCharArray();
            char[] Upper = "ABCÇDEFGĞHİIJKLMNOÖPRSŞTUÜVYZ".ToCharArray();
            char[] numbers = "1234567890".ToCharArray();

            // a xxxxxx A
            var pass = Upper[rnd.Next(Upper.Length)].ToString()
                       + rnd.NextInt64(0, 10).ToString()
                       + Lower[rnd.Next(Lower.Length)].ToString();

            var sumArr = "abcçdefgğhvyzABCÇDEFGĞHİIJKLVYZ1234iıjklmnoöprsştuü567890".ToCharArray();

            for (int i = 0; i < 8; i++)
            {
                pass += sumArr[rnd.Next(sumArr.Length - 1)].ToString();
            }

            return pass;
        }
        private string SignUpWithGoogleHelpMessage(string pass)
        {
           

            var msg2 = $@"


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

                <div style=""background-color:#ffffff; padding: 30px; height: 370px; text-align:center;"">

                <img style=""width: 150px;  object-fit: cover;"" src=""https://onlinepsikologmerkezi.com/assets/img/singup-image.jpg"">

    
                    <h3>Online Psikolog Merkezine Hoş Geldin </p>

                     <p>Bizimle beraber olduğun için çok mutluyuz</p>

                     <p>Google hesabı kullanmadan giriş yapmak isterseniz, google mailiniz ve bu şifreyi kullanabilirsiniz => {pass}</p>

                </div>

                </body>
                </html>



            ";

            return msg2;
        }
        private string ActivationMailBody(string data)
        {

        

            var msg2 = $@"


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

                <div style=""background-color:#ffffff; padding: 30px; height: 370px; text-align:center;"">
                <img style=""width: 150px;  object-fit: cover;"" src=""https://onlinepsikologmerkezi.com/assets/img/singup-image.jpg"">

    

    
                    <h3>Online Psikolog Merkezine Hoş Geldin </p>

                    <p>Hesabını aktif etmen için sana yolladığımız bağlantıya tıklaman yeterli.</p>

                    <a href=""{data}"" style=""background-color: #0275d8; color: white; display: inline-block; text-align: center; text-decoration: none; border: 1px solid black; width: 50%;"">Hesap Aktivasyon Bağlantısı Buraya Tıklayınız!</a>

                </div>

                </body>
                </html>



        ";


            return msg2;
        }
        private string ResetPasswordMailBody(string data)
        {

            var msg = $@"
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

                <div style=""background-color:#ffffff; padding: 30px; height: 370px; text-align:center;"">

                <img style=""width: 150px;  object-fit: cover;"" src=""https://onlinepsikologmerkezi.com/assets/img/singup-image.jpg"">

    
                    <h3>Online Psikolog Merkezine Hoş Geldin </p>

                    <p>Bugün şifresini unutan ilk kişi değilsin üzülme :) </p>

                    <a href=""{data}"" style=""background-color: #0275d8; color: white; display: inline-block; text-align: center; text-decoration: none; border: 1px solid black; width: 50%;"">Şifreni sıfırlamak için buraya tıkla!</a>

                </div>

                </body>
                </html>



            ";
            return msg;
        }

    }
}

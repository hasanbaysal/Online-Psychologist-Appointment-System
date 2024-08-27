using AspNetCoreHero.ToastNotification.Abstractions;
using HB.OnlinePsikologMerkezi.Business.Services;
using HB.OnlinePsikologMerkezi.Common.CustomResponse;
using HB.OnlinePsikologMerkezi.Dto.Dtos;
using HB.OnlinePsikologMerkezi.Entities.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Web;

namespace HB.OnlinePsikologMerkezi.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService authService;
        private readonly SignInManager<AppUser> signInManager;
        private readonly UserManager<AppUser> userManager;
        private readonly IAdminService adminService;
        private readonly INotyfService _notyf;


        public AccountController(IAuthService authService, SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, IAdminService adminService, INotyfService notyf)
        {
            this.authService = authService;
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.adminService = adminService;
            _notyf = notyf;
        }

        public async Task<IActionResult> Activation(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(token))
            {
                TempData["aktivasyon_hata"] = "dolu";
                //_notyf.Error("aktivasyon başarızı");


                return Redirect("/home/index");
            }
            userId = HttpUtility.UrlDecode(userId);
            token = HttpUtility.UrlDecode(token);

            var result = await authService.AccountActivationAsync(userId, token);

            if (result.ResponseType == ResponseType.NotFound)
            {
                TempData["aktivasyon_hata"] = "dolu";
                return Redirect("/home/index");
            }

            //_notyf.Success("aktivasyon başarılı");

            TempData["aktivasyon_hata"] = "yok";
            return Redirect("/home/index");
        }
        public async Task<IActionResult> LoginWithGoogle(string returnUrl = "/")
        {

            string redirectUrl = Url.Action("ExternalLogin", "Account", new { ReturnUrl = returnUrl })!;


            var result = await authService.LoginWithGoole(redirectUrl);

            return result;


        }
        public async Task<IActionResult> ExternalLogin(string returnUrl = "/")
        {

            var result = await authService.ExternalLogin(returnUrl);


            if (result.ResponseType == ResponseType.Success)
            {
                return Redirect(result.Message);
            }
            else if (result.ResponseType == ResponseType.isBlock)
            {
                TempData["isblock"] = "siktirgit";
                return Redirect("/home/index");
            }
            else
            {
                return Redirect("/Home/Error");
            }

        }

        public IActionResult SignUp()
        {
            if (User!.Identity!.IsAuthenticated)
            {
                return Redirect("/home/index");
            }

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SignUp(UserSignUpDto dto)
        {

            var result = await authService.SingUpAsync(dto);

            if (result.ResponseType == ResponseType.ValidationError)
            {
                result!.Errors!.ForEach(x => ModelState.AddModelError(x.PropertyName ?? "", x.ErrorMessage));
                _notyf.Error("işlem başarısız", 5);
                return View();
            }

            TempData["hesap_olustu"] = "ok";

            return Redirect("/home/index");


        }

        public async Task<IActionResult> Logout()
        {

            await signInManager.SignOutAsync();

            return Redirect("/");
        }

        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return Redirect("/home/index");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(UserSigInDto dto, string returnUrl = "/")
        {

            var result = await authService.SingInWithPassword(dto);

            if (result.ResponseType == ResponseType.ValidationError)
            {
                result!.Errors!.ForEach(x => ModelState.AddModelError(x.PropertyName, x.ErrorMessage));
                return View();
            }
            if (result.ResponseType == ResponseType.isBlock ||
                result.ResponseType == ResponseType.NotFound ||
                result.ResponseType == ResponseType.IsnNotAllowed ||
                result.ResponseType == ResponseType.IsLockOut ||
                result.ResponseType == ResponseType.Fail)
            {
                _notyf.Error("giriş başarızı", 5);
                ModelState.AddModelError("", result.Message);
                return View();
            }

            return Redirect(returnUrl);
        }

        //reset sayfasında token ve userid'in saklanması gerek kullanıcı hatalı işlem yaparsa eski identity örnekte mevcut

        //bunu post action gibi düşün bunun birde get'i var

        public IActionResult ForgotPassword()
        {
            if (User!.Identity!.IsAuthenticated)
            {
                return Redirect("/home/index");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgetPasswordDto dto)
        {
            var result = await authService.ForgetPasswordSendMail(dto);

            if (result.ResponseType == ResponseType.ValidationError)
            {
                result!.Errors!.ForEach(x => ModelState.AddModelError(x.PropertyName ?? "", x.ErrorMessage ?? ""));
                _notyf.Error("başarısız işlem");
                return View();
            }
            if (result.ResponseType == ResponseType.NotFound)
            {
                ModelState.AddModelError("", result.Message);
                _notyf.Error("başarısız işlem");
                return View();
            }

            TempData["e_mail_gonderildi"] = "dolu";

            return Redirect("/home/index");
        }


        //reset'sayfasının get'i
        public async Task<IActionResult> ResetPassword(string userId, string token)
        {


            if (User!.Identity!.IsAuthenticated)
            {
                return Redirect("/home/index");
            }

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {

                if (TempData["userId"] != null && TempData["token"] != null)
                {

                    TempData["userId"] = TempData["userId"]?.ToString();
                    TempData["token"] = TempData["token"]?.ToString();

                    return View();

                }

                TempData["hatalı_link"] = "dolu";
                return Redirect("/home/index");

            }

            var result = await authService.ConfrimUserIdAndResetToken(userId, token);


            //tempdata ıd pass sakla


            if (result)
            {
                TempData["userId"] = userId;
                TempData["token"] = token;

                return View();
            }

            TempData["hatalı_link"] = "dolu";
            return Redirect("/home/index");
        }

        //burası postu olacak

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {

            string? token = TempData["token"]?.ToString();
            string? userId = TempData["userId"]?.ToString();

            var result = await authService.ResetPassord(dto, userId, token);
            if (result.ResponseType == ResponseType.Fail)
            {

                TempData["hatalı_link"] = "dolu";
                return Redirect("/home/index");
            }

            if (result.ResponseType == ResponseType.ValidationError)
            {
                //geri döneceğiz


                TempData["userId"] = TempData["userId"]?.ToString();
                TempData["token"] = TempData["token"]?.ToString();

                result.Errors?.ForEach(x => ModelState.AddModelError(x.PropertyName, x.ErrorMessage));

                return View();
            }

            TempData["sifre_degisim"] = "dolu";
            return Redirect("/home/index");

        }


        ////şifre yenileme actionlar
        //public IActionResult ForgetPassword() => View();
        //[HttpPost]
        //public IActionResult ForgetPassword(ForgetPasswordDto dto) => View();


        //şifre resetleme
        //id ve şifre doğru ise sayfayı aç
        //public IActionResult ResetPassword(string userId, string token) => View();
        //public IActionResult ResetPassword(object resetpassDto) => View();





    }
}

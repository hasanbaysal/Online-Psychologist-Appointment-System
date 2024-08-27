using AspNetCoreHero.ToastNotification.Abstractions;
using AutoMapper;
using HB.OnlinePsikologMerkezi.Business.Mapping.CategoryPskMappings;
using HB.OnlinePsikologMerkezi.Business.Services;
using HB.OnlinePsikologMerkezi.Data.Interface;
using HB.OnlinePsikologMerkezi.Dto.Dtos;
using HB.OnlinePsikologMerkezi.Entities.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HB.OnlinePsikologMerkezi.Web.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<AppUser> userManager;
        private readonly IMapper mapper;
        private readonly SignInManager<AppUser> signInManager;
        private readonly INotyfService _notyf;
        private readonly IAdminService adminService;
        private readonly IUow uow;
        public ProfileController(UserManager<AppUser> userManager, IMapper mapper, SignInManager<AppUser> signInManager, INotyfService notyf, IAdminService adminService, IUow uow)
        {
            this.userManager = userManager;
            this.mapper = mapper;
            this.signInManager = signInManager;
            _notyf = notyf;
            this.adminService = adminService;
            this.uow = uow;
        }

        public async Task<IActionResult> MyProfile()
        {

            var user =  await userManager.FindByNameAsync(User.Identity.Name);

         
            //ViewBag.email = user.Email;

           var mappeddata=   mapper.Map<UserUpdateDto>(user);


            return View(mappeddata);
        }

        [HttpPost]
        public async Task<IActionResult> MyProfile(UserUpdateDto dto)
        {
            var user = await userManager.FindByIdAsync(dto.Id);

            user.UserName = dto.UserName;
            user.Name = dto.Name;
            user.LastName = dto.LastName;
            user.PhoneNumber = dto.PhoneNumber;


            user.LastLoginIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? " ";

            if (!ModelState.IsValid)
            {
                _notyf.Error("güncelleme başarısız");
                return View(dto);
            }


            var result  = await userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                result.Errors.ToList().ForEach(x => ModelState.AddModelError(x.Code, x.Description));

                //ViewBag.email = user.Email;

                _notyf.Error("güncelleme başarısız");
                return View(dto);

            }

            _notyf.Success("güncelleme başarılı");
                await signInManager.SignOutAsync();
            await signInManager.SignInAsync(user,true);
            return Redirect("/Profile/Myprofile");


        }
        
        public async Task<IActionResult> MyAppointments()
        {
            if (TempData["commennt_error"] != null)
            {
                _notyf.Error("yorum ekleme başarız");
            }
            if (TempData["commennt_success"] != null)
            {
                _notyf.Success("yorum ekleme başarılı");
            }
            if (TempData["commennt_delete_error"] != null)
            {
                _notyf.Error("yorum silme başarısız");
            }
            if (TempData["commennt_delete_ok"] != null)
            {
                _notyf.Success("yorum silme başarılı");
            }
                var  data = await  adminService.GetUserdetails(User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value);

            return View(data.Data);
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(int appointmentid,string comment)
        {


            var data = await uow.GetRepository<Appointment>()
                .GetQueryable()
                .Where(x => x.Id == appointmentid)
                .Include(x => x.Order)
                .ThenInclude(x => x.AppUser)
                .FirstOrDefaultAsync();

            if (data == null)
            {
                TempData["commennt_error"] = "asd";
                return RedirectToAction("MyAppointments");
            }
            var currentUserid = User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;

            if (currentUserid != data.Order.AppUser.Id)
            {
                TempData["commennt_error"] = "asd";
                return RedirectToAction("MyAppointments");
            }

            if ( string.IsNullOrEmpty(comment))
            {
                TempData["commennt_error"] = "asd";
                return RedirectToAction("MyAppointments");
            }
            if (comment.Length > 200 )
            {
                TempData["commennt_error"] = "asd";
                return RedirectToAction("MyAppointments");
            }
            data.UserAppointmentComment = comment;

            uow.GetRepository<Appointment>().Update(data);
            await uow.CommitAsync();
            TempData["commennt_success"] = "asd";
            return RedirectToAction("MyAppointments");
        }

        public async Task<IActionResult> CommentDelete(int id)
        {

            var data = await uow.GetRepository<Appointment>()
               .GetQueryable()
               .Where(x => x.Id == id)
               .Include(x => x.Order)
               .ThenInclude(x => x.AppUser)
               .FirstOrDefaultAsync();

            if (data == null)
            {
                TempData["commennt_delete_error"] = "asd";
            }
            var currentUserid = User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;

            if (currentUserid != data.Order.AppUser.Id)
            {
                TempData["commennt_delete_error"] = "asd";
            }

            data.UserAppointmentComment = null;

            uow.GetRepository<Appointment>().Update(data);
            await uow.CommitAsync();
            TempData["commennt_delete_ok"] = "asd";
            return RedirectToAction("MyAppointments");
        }



    }
}

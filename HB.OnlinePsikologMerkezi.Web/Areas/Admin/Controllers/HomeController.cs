using AspNetCoreHero.ToastNotification.Abstractions;
using AutoMapper;
using HB.OnlinePsikologMerkezi.Business.Services;
using HB.OnlinePsikologMerkezi.Common.CustomEnums;
using HB.OnlinePsikologMerkezi.Data.Context;
using HB.OnlinePsikologMerkezi.Data.Interface;

using HB.OnlinePsikologMerkezi.Data.UnitOfWork;
using HB.OnlinePsikologMerkezi.Dto.Dtos;
using HB.OnlinePsikologMerkezi.Entities.Entities;
using HB.OnlinePsikologMerkezi.Web.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.Drawing.Printing;
using System.IO;

namespace HB.OnlinePsikologMerkezi.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles ="admin")]
    public class HomeController : Controller
    {
        private readonly IAdminService adminService;
        private readonly IUow uow;
        private readonly IFileProvider _fileProvider;
        private readonly IMapper mapper;
        private readonly INotyfService _notyf;
        private readonly UserManager<AppUser> userManager;
        private readonly AppDbContext context;
        public HomeController(
            IAdminService adminService,
            IUow uow,
            IFileProvider fileProvider,
            IMapper mapper,
            INotyfService notyf,
            UserManager<AppUser> userManager,
            AppDbContext context)
        {
            this.adminService = adminService;
            this.uow = uow;
            _fileProvider = fileProvider;
            this.mapper = mapper;
            _notyf = notyf;
            this.userManager = userManager;
            this.context = context;
        }


        [HttpGet("admin/home/index/{size?}/{page?}")]
        public async Task<IActionResult> Index(int size=50,int page=1)
        {

            //toplam kazanç

            var toplamKazanc = await uow.GetRepository<Appointment>().GetQueryable()
                .Where(x => (x.Status == (int)AppointmentEnum.sold  || x.Status == (int)AppointmentEnum.Completed ))
                 .SumAsync(x=>x.Price);

            var toplamSatiş = await uow.GetRepository<Appointment>().GetQueryable()
             .Where(x => x.Status == (int)AppointmentEnum.sold || x.Status == (int)AppointmentEnum.Completed)
              .AsNoTracking().CountAsync();

            var toplamKUllanıcıSAyıcı = await uow.GetRepository<AppUser>().GetQueryable().CountAsync();

            ViewBag.toplamkazanc = toplamKazanc;
            ViewBag.toplamSatiş = toplamSatiş;
            ViewBag.toplamKUllanıcıSAyıcı = toplamKUllanıcıSAyıcı;



            var count = await context.Users
                .Where(x => !context.Set<Psychologist>().Any(b => b.Psychologist_ID == x.Id)).CountAsync();
               

            ViewBag.count = count;

            var users = await adminService.GetUserAsync(size, page);





            return View(users);
        }
        public async Task<IActionResult> GetUserInformation(string id)
        {
            var data = await adminService.GetUserByEmail(id);
            if (data.Data != null)
            {
                return PartialView("_UserQueryPartialView", data.Data);

            }

            return PartialView("_UserQueryPartialView", data.Data);

        }
        public async Task<IActionResult> GetUserById(string id)
        {
            var data = await adminService.GetUserdetails(id);

            return View(data.Data);
        }
        public async Task<IActionResult> PsychologistAdd(string id)
        {

            ViewBag.id = id;


            var ispsk = await uow.GetRepository<Psychologist>().GetByFilterAsync(x => x.Psychologist_ID == id);

            if (ispsk != null)
            {
                RedirectToAction("GetUserInformation", "Home", new { id = id });
            }

            var category = await uow.GetRepository<Category>().GetAllAsync(false);
            var SelectList = new List<SelectListItem>();

            category.ToList().ForEach(x =>
            {
                SelectList.Add(new SelectListItem()
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                });
            });

            ViewBag.liste = SelectList;


            return View();
        }
        [HttpPost]
        public async Task<IActionResult> PsychologistAdd(PskAddViewModel vm, string[] cateValues)
        {
            if (ModelState.IsValid)
            {
                if (vm.Photo != null && vm.Photo.Length > 0)
                {


                    var pskimageBasePath = Path.Combine(_fileProvider.GetFileInfo("assets").PhysicalPath, "pksimage");

                    var randomName = Guid.NewGuid() + Path.GetExtension(vm.Photo.FileName);

                    var path = Path.Combine(pskimageBasePath, randomName);


                    var mappeddata = mapper.Map<PsychologistAddDto>(vm);

                    cateValues.ToList().ForEach(x => mappeddata.PsychologistCategories.Add(new() { CategorId = int.Parse(x) }));
                    mappeddata.ProfilePhotoPath = randomName;

                    var result = await adminService.PsychologistRoleAddAsync(mappeddata);

                    if (result.ResponseType == Common.CustomResponse.ResponseType.Success)
                    {
                        using var stream = new FileStream(path, FileMode.Create);

                        await vm.Photo.CopyToAsync(stream);
                        _notyf.Success("işlem başarılı");
                        return View();
                    }
                    else if (result.ResponseType == Common.CustomResponse.ResponseType.Fail)
                    {
                        _notyf.Error("bu kullanıcı zaten psikolog");
                        return View();
                    }

                    else if (result.ResponseType == Common.CustomResponse.ResponseType.ValidationError)
                    {
                        result?.Errors!.ForEach(x => ModelState.AddModelError(x.PropertyName, x.ErrorMessage));
                        _notyf.Error("işlem başarızı giriş yapılan değerleri kontrol ediniz");

                        ViewBag.id = vm.Psychologist_ID;

                        var category = await uow.GetRepository<Category>().GetAllAsync(false);
                        var SelectList = new List<SelectListItem>();

                        category.ToList().ForEach(x =>
                        {
                            SelectList.Add(new SelectListItem()
                            {
                                Text = x.Name,
                                Value = x.Id.ToString()
                            });
                        });

                        ViewBag.liste = SelectList;

                        return View(vm);
                    }

                }
                else
                {
                    ViewBag.id = vm.Psychologist_ID;
                    var category = await uow.GetRepository<Category>().GetAllAsync(false);
                    var SelectList = new List<SelectListItem>();

                    category.ToList().ForEach(x =>
                    {
                        SelectList.Add(new SelectListItem()
                        {
                            Text = x.Name,
                            Value = x.Id.ToString()
                        });
                    });

                    ViewBag.liste = SelectList;


                    _notyf.Error("işlem başarız");
                    return View(vm);

                }

            }



            ViewBag.id = vm.Psychologist_ID;
            var category2 = await uow.GetRepository<Category>().GetAllAsync(false);
            var SelectList2 = new List<SelectListItem>();
            category2.ToList().ForEach(x =>
            {
                SelectList2.Add(new SelectListItem()
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                });
            });
            ViewBag.liste = SelectList2;
            _notyf.Error("işlem başarız");
            return View();
        }
        public async Task<IActionResult> PsychologistUpdate(string id)
        {

            var data = await adminService.GetPskForUpdate(id);


            var mappeddata = mapper.Map<PskUpdateViewModel>(data.Data);


            var category = await uow.GetRepository<Category>().GetAllAsync(false);
            var SelectList = new List<SelectListItem>();

            category.ToList().ForEach(x =>
            {
                SelectList.Add(new SelectListItem()
                {
                    Text = x.Name,
                    Value = x.Id.ToString(),
                    Selected = cateControl(data.Data.PsychologistCategories, x.Id)
                });
            });

            ViewBag.liste = SelectList;

            ViewBag.id = id;






            return View(mappeddata);
        }
        [HttpPost]
        public async Task<IActionResult> PsychologistUpdate(PskUpdateViewModel vm, string[] cateValues)
        {

            var deletedimagepathdata = await uow.GetRepository<Psychologist>().GetQueryable().AsNoTracking().Where(x => x.Psychologist_ID == vm.Psychologist_ID).FirstOrDefaultAsync();
            var deletedimagepath = deletedimagepathdata.ProfilePhotoPath;
            if (!ModelState.IsValid)
            {
                var data = await adminService.GetPskForUpdate(vm.Psychologist_ID);

                 deletedimagepath = data.Data.ProfilePhotoPath;
                var mappeddata = mapper.Map<PskUpdateViewModel>(data.Data);


                var category = await uow.GetRepository<Category>().GetAllAsync(false);
                var SelectList = new List<SelectListItem>();

                category.ToList().ForEach(x =>
                {
                    SelectList.Add(new SelectListItem()
                    {
                        Text = x.Name,
                        Value = x.Id.ToString(),
                        Selected = cateControl(data.Data.PsychologistCategories, x.Id)
                    });
                });

                ViewBag.liste = SelectList;

                _notyf.Error("işlem başarız bilgileri kontrol edin");
                return View(mappeddata);

            }

            if (vm.Photo != null && vm.Photo.Length > 0)
            {

                var mappeddata = mapper.Map<PsychologistUpdateDto>(vm);

                var pskimageBasePath = Path.Combine(_fileProvider.GetFileInfo("assets").PhysicalPath, "pksimage");
              
               
                var randomName = Guid.NewGuid() + Path.GetExtension(vm.Photo.FileName);

                var path = Path.Combine(pskimageBasePath, randomName);

                cateValues.ToList().ForEach(x => mappeddata.PsychologistCategories.Add(new() { CategorId = int.Parse(x) }));

                mappeddata.ProfilePhotoPath = randomName;

                var result = await adminService.PskUpdate(mappeddata);
                if (result.ResponseType == Common.CustomResponse.ResponseType.Success)
                {
                    using var stream = new FileStream(path, FileMode.Create);

                    await vm.Photo.CopyToAsync(stream);

                    var deletePhotoPath = Path.Combine(_fileProvider.GetFileInfo("assets").PhysicalPath, "pksimage", deletedimagepath);
                    System.IO.File.Delete(deletePhotoPath);

                    RedirectToAction("index");
                }
                //validation error
                else
                {
                    result?.Errors!.ForEach(x => ModelState.AddModelError(x.PropertyName, x.ErrorMessage));
                    _notyf.Error("işlem başarızı giriş yapılan değerleri kontrol ediniz");

                    var data = await adminService.GetPskForUpdate(vm.Psychologist_ID);

                    var mappeddata2 = mapper.Map<PskUpdateViewModel>(data.Data);

                    var category = await uow.GetRepository<Category>().GetAllAsync(false);
                    var SelectList = new List<SelectListItem>();

                    category.ToList().ForEach(x =>
                    {
                        SelectList.Add(new SelectListItem()
                        {
                            Text = x.Name,
                            Value = x.Id.ToString(),
                            Selected = cateControl(data.Data.PsychologistCategories, x.Id)
                        });
                    });

                    ViewBag.liste = SelectList;

                    _notyf.Error("işlem başarız bilgileri kontrol edin");
                    return View(mappeddata2);


                }


            }
            else
            {
                var mappeddata = mapper.Map<PsychologistUpdateDto>(vm);
                cateValues.ToList().ForEach(x => mappeddata.PsychologistCategories.Add(new() { CategorId = int.Parse(x) }));

                var psk = await uow.GetRepository<Psychologist>().GetByFilterAsync(x => x.Psychologist_ID == vm.Psychologist_ID);

                mappeddata.ProfilePhotoPath = psk.ProfilePhotoPath;

                var result = await adminService.PskUpdate(mappeddata);


                if (result.ResponseType == Common.CustomResponse.ResponseType.Success)
                {
                    _notyf.Success("işlem başarılı");
                    return RedirectToAction("index");

                }
                else
                {
                    result?.Errors!.ForEach(x => ModelState.AddModelError(x.PropertyName, x.ErrorMessage));
                    _notyf.Error("işlem başarızı giriş yapılan değerleri kontrol ediniz");

                    var data = await adminService.GetPskForUpdate(vm.Psychologist_ID);

                    var mappeddata2 = mapper.Map<PskUpdateViewModel>(data.Data);

                    var category = await uow.GetRepository<Category>().GetAllAsync(false);
                    var SelectList = new List<SelectListItem>();

                    category.ToList().ForEach(x =>
                    {
                        SelectList.Add(new SelectListItem()
                        {
                            Text = x.Name,
                            Value = x.Id.ToString(),
                            Selected = cateControl(data.Data.PsychologistCategories, x.Id)
                        });
                    });

                    ViewBag.liste = SelectList;

                    _notyf.Error("işlem başarız bilgileri kontrol edin");
                    return View(mappeddata2);
                }

            }


            return View();
        }
        public async Task<IActionResult> PskList()
        {

            var data = await uow.GetRepository<Psychologist>()
                .GetQueryable()
                .Include(x => x.AppUser)
                .ToListAsync();

            var mappeddata = mapper.Map<List<PsychologistListDto>>(data);

            return View(mappeddata);
        }
        public async Task<IActionResult> PskDeatils(string id)
        {

            var data =  await uow.GetRepository<Psychologist>()
                .GetQueryable()
                .Where(x=>x.Psychologist_ID == id)
                .Include(x => x.AppUser)
                .Include(x => x.Appointments)
                .ThenInclude(x => x.Order)
                .ThenInclude(x=>x.AppUser)
                .FirstOrDefaultAsync();

            var mappedData = mapper.Map<PsychologistListDto>(data);


            return View(mappedData);
        }
        private bool cateControl(List<CategoryPskAddDto> pskcategory, int id)
        {

            bool control = false;

            foreach (var item in pskcategory)
            {
                if (item.CategorId == id)
                {
                    control = true;
                }
            }

            return control;
        }
        public async Task<IActionResult> PaymentPskforAppointment(int id)
        {
            var data = await uow.GetRepository<Order>().GetQueryable().Where(x => x.AppointmentId == id).FirstOrDefaultAsync();

            
            data.IsItPaid= !data.IsItPaid;

            uow.GetRepository<Order>().Update(data);

            await uow.CommitAsync();

            return Ok();
        }
        public async Task<IActionResult> BlockUser(string id)
        {

            var user = await userManager.FindByIdAsync(id);
            await adminService.UserBlockAsync(user.UserName);
            _notyf.Success("kullanıcı engellendi oturumu en fazla 30dk içine sonlanacaktır");
           return  Redirect("/admin/home/GetUserById/" + $"{id}");
        }
        public async Task<IActionResult> UnBlockUser(string id)
        {

            var user = await userManager.FindByIdAsync(id);
            await adminService.UserUnBlockAsync(user.UserName);
            _notyf.Success("kullanıcı engeli kaldırıldı");
            return Redirect("/admin/home/GetUserById/" + $"{id}");
        }
        public async Task<IActionResult> CategoryList()
        {

            var data = await uow.GetRepository<Category>().GetAllAsync(false);

            return View(data);
        }
        public async Task<IActionResult> CategoryAdd(string Name)
        {


            if (string.IsNullOrEmpty(Name))
            {
                TempData["kod"] = "data";
                return RedirectToAction("CategoryList");
            }

            var data = await uow.GetRepository<Category>().GetByFilterAsync(x => x.Name == Name);
            if (Name == "")
            {
                TempData["kod"] = "data";
                return RedirectToAction("CategoryList");

            }

            if (data != null )
            {

                TempData["kod"] = "data";
                return RedirectToAction("CategoryList");

            }

            uow.GetRepository<Category>().Add(new Category() { Name = Name });
                await uow.CommitAsync();
            
            return RedirectToAction("CategoryList");


        }
        public async Task<IActionResult> CategoryRemove(int id)
        {

            var data = await uow.GetRepository<Category>().GetQueryable().Where(x => x.Id == id).FirstAsync();
            uow.GetRepository<Category>().Delete(data);
            await uow.CommitAsync();

            return RedirectToAction("CategoryList");
        }


        public async Task<IActionResult> BlogList()
        {
            var data = await uow.GetRepository<Blog>().GetAllAsync(false);

            if (TempData["blog-update-error"] != null)
            {
                _notyf.Error("güncelleme başarısız");
            }
            if (TempData["blog-update-ok"] != null)
            {
                _notyf.Success("güncelleme başarılı");
            }



            return View(data);
        }

        public IActionResult BlogAdd() => View();
        
        [HttpPost]
        public async Task<IActionResult> BlogAdd(BlogAddDto dto)
        {
            if (!ModelState.IsValid)
            {
                _notyf.Warning("işlem başarısız");
                return View();
            }
            if (dto.Image != null && dto.Image.Length>0)
            {

                var blogimagebasepath = Path.Combine(_fileProvider.GetFileInfo("assets").PhysicalPath, "blogimage");
                var randomName = Guid.NewGuid() + Path.GetExtension(dto.Image.FileName);
                var path = Path.Combine(blogimagebasepath, randomName);

                using var stream = new FileStream(path, FileMode.Create);

                await dto.Image.CopyToAsync(stream);


                var mappeddata = mapper.Map<Blog>(dto);

                mappeddata.BlogImagePath = randomName;
                mappeddata.PostWriteTime = DateTime.Now;

                uow.GetRepository<Blog>().Add(mappeddata);
                await uow.CommitAsync();
                return RedirectToAction("BlogList");


            }



            _notyf.Warning("işlem başarısız");
            return View();
        }

        public async Task<IActionResult> BlogUpdate(int id)
        {
            var data =  await uow.GetRepository<Blog>().GetQueryable().Where(x => x.Id == id).AsNoTracking().FirstOrDefaultAsync();

            var mappeddata = mapper.Map<BlogUpdateDto>(data);

            return View(mappeddata);
        }
        [HttpPost]
        public async Task<IActionResult> BlogUpdate(BlogUpdateDto dto)
        {

            var orginalData = await uow.GetRepository<Blog>().GetQueryable().Where(x => x.Id == dto.Id).FirstOrDefaultAsync();
            var oldimagePath = orginalData.BlogImagePath;
                

            if (!ModelState.IsValid)
            {
                TempData["blog-update-error"] = "ss";
                return RedirectToAction("BlogList");
            }
            orginalData.Author = dto.Author;
            //orginalData.PostWriteTime = dto.PostWriteTime;
            orginalData.ArticleUrl = dto.ArticleUrl;
            orginalData.Title = dto.Title;
            orginalData.Key = dto.Key;
            orginalData.Description = dto.Description;
            orginalData.Header = dto.Header;
            orginalData.Content = dto.Content;


            if (dto.Image != null && dto.Image.Length>0)
            {

                var blogimagebasepath = Path.Combine(_fileProvider.GetFileInfo("assets").PhysicalPath, "blogimage");
                
                var deleteimagepath = Path.Combine(_fileProvider.GetFileInfo("assets").PhysicalPath, "blogimage", oldimagePath);
                System.IO.File.Delete(deleteimagepath);

                var randomName = Guid.NewGuid() + Path.GetExtension(dto.Image.FileName);
                var path = Path.Combine(blogimagebasepath, randomName);
                using var stream = new FileStream(path, FileMode.Create);

                await dto.Image.CopyToAsync(stream);
                orginalData.BlogImagePath = randomName;
                uow.GetRepository<Blog>().Update(orginalData);
                await uow.CommitAsync();
                TempData["blog-update-ok"] = "ss";

                return RedirectToAction("BlogList");

            }

            uow.GetRepository<Blog>().Update(orginalData);
            await uow.CommitAsync();
            TempData["blog-update-ok"] = "ss";

            return RedirectToAction("BlogList");
        }

        public async Task<IActionResult> BlogDelete(int id) 
        {
            var data = await uow.GetRepository<Blog>().GetQueryable().Where(x => x.Id == id).FirstOrDefaultAsync();


            if (data == null)
            {
                return RedirectToAction("BlogList");
            }
            uow.GetRepository<Blog>().Delete(data);
            await uow.CommitAsync();
            return RedirectToAction("BlogList");
        }

        public async Task<IActionResult> MessageList()
        {

            var data = await uow.GetRepository<Message>()
                .GetQueryable().AsNoTracking().ToListAsync();


            return View(data);
        }
        public async Task<IActionResult> MessageDelete(int id)
        {

            var data = await uow.GetRepository<Message>().GetQueryable().Where(x => x.Id == id).FirstOrDefaultAsync();

            if (data !=null)
            {
                uow.GetRepository<Message>().Delete(data);
                await uow.CommitAsync();
            }

            return  RedirectToAction("MessageList");
        }


        public  async Task<IActionResult> AppointmentsControl()
        {

            if (TempData["trash-ok"] != null || TempData["trash-ok2"] != null)
            {
                _notyf.Success("Temizleme işlemi başarılı");
            }
            if (TempData["appointment-not-found"] != null)
            {
                _notyf.Warning("randevu bulunamad");
            }



            var data = await context.Set<Appointment>()
                .Include(x=>x.Psychologist)
                .ThenInclude(x=>x.AppUser)
                .Include(x=>x.Order)
                .ThenInclude(x=>x.AppUser)
                .Where(x => ( (x.Status == (int)AppointmentEnum.sold || x.Status ==(int)AppointmentEnum.Completed) && x.Order.DurchaseDate > DateTime.Now.AddDays(-30)) || (x.AppointmentDate>DateTime.Now && x.Status == (int)AppointmentEnum.new_appointment) )
                .ToListAsync();




            return View(data);
        }

        public async Task<IActionResult> CleanAppointment()
        {

            var data = await context.Set<Appointment>().Where(x => x.Status == (int)AppointmentEnum.new_appointment && x.AppointmentDate.AddDays(1) < DateTime.Now).ToListAsync();


            if (data != null)
            {
                if (data.Count>0)
                {
                    context.Set<Appointment>().RemoveRange(data);
                    await context.SaveChangesAsync();

                    TempData["trash-ok"] = "ss";

                }
            }

            var data2 = await context.Set<Appointment>().Where(x => x.Status == (int)AppointmentEnum.s3dCheck && x.AppointmentDate.AddDays(1) < DateTime.Now).ToListAsync();


            if (data2 != null)
            {
                if (data2.Count > 0)
                {
                    context.Set<Appointment>().RemoveRange(data2);
                    await context.SaveChangesAsync();
                    TempData["trash-ok2"] = "ss";

                }
            }



            return RedirectToAction("AppointmentsControl");
            
        }

        [HttpPost]
        public async Task<IActionResult> AppointmentSearch(int id)
        {

            var data =  await uow.GetRepository<Appointment>().GetQueryable()
                .Where(x => x.Id == id)
                .Include(x=>x.Psychologist)
                .ThenInclude(x=>x.AppUser)
                .Include(x=>x.Order)
                .ThenInclude(x=>x.AppUser)
                .FirstOrDefaultAsync();


            if (data == null)
            {
                TempData["appointment-not-found"] = "ss";
                return RedirectToAction("AppointmentsControl");
            }
            return View(data);
        }

        public async Task<IActionResult> AppointmenntCancel(int id)
        {


            var data = await uow.GetRepository<Appointment>()
                .GetQueryable()
                .Where(x => x.Id == id)
                .Include(x=>x.Order)
                .FirstOrDefaultAsync();



            //uow.GetRepository<Order>().Delete(data!.Order);

            data.Status = (int)AppointmentEnum.cancel;


            await uow.CommitAsync();


            return RedirectToAction("AppointmentsControl");
        }


    }
}

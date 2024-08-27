using AspNetCoreHero.ToastNotification.Abstractions;
using AutoMapper;
using HB.OnlinePsikologMerkezi.Business.Services;
using HB.OnlinePsikologMerkezi.Data.Interface;
using HB.OnlinePsikologMerkezi.Dto.Dtos;
using HB.OnlinePsikologMerkezi.Entities.Entities;
using HB.OnlinePsikologMerkezi.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.Text;
using System.Xml;

namespace HB.OnlinePsikologMerkezi.Web.Controllers
{
    public class HomeController : Controller
    {

        private readonly IAuthService authService;
        private readonly IAdminService adminService;
        private readonly RoleManager<AppRole> roleManager;
        private readonly IPsychologistService psychologistService;
        private readonly IPaymentService paymentService;
        private readonly INotyfService _notyf;
        private readonly IMemoryCache _memoryCache;
        private readonly ICategoryService categoryService;
        private readonly IUow uow;
        private readonly IMapper mapper;

        string psk_cache_key = "psk_liste_data";
        string cate_cache_key = "cate_liste_data";
        string blog_cache_key = "blog_liste_data";
        public HomeController(
            IAuthService authService,
            IAdminService adminService,
            RoleManager<AppRole> roleManager,
            IPsychologistService psychologistService,
            IPaymentService paymentService,
            INotyfService notyf,
            IMemoryCache memoryCache,
            ICategoryService categoryService,
            IUow uow,
            IMapper mapper)
        {
            this.authService = authService;
            this.adminService = adminService;
            this.roleManager = roleManager;
            this.psychologistService = psychologistService;
            this.paymentService = paymentService;
            _notyf = notyf;
            _memoryCache = memoryCache;
            this.categoryService = categoryService;
            this.uow = uow;
            this.mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {



         
            if (TempData["join_timeout"] != null)
            {
                _notyf.Warning("süresi geçmiş bir toplantıya katılamazsınız");
            }
            if(TempData["join_error"] != null)
            {
                _notyf.Error("toplantı bağlantısı hatalı veya geçersiz");
            }
            if (TempData["hatalı_link"] != null)
            {
                _notyf.Warning("bağlantınız bozuk yada süresi geçmiş");
            }
            if (TempData["aktivasyon_hata"] != null)
            {
                if (TempData["aktivasyon_hata"]!.ToString() == "dolu")
                {
                    _notyf.Warning("aktivasyon başarız");
                }
                if (TempData["aktivasyon_hata"]!.ToString() == "yok")
                {
                    _notyf.Success("aktivasyon başarılı giriş yapabilirsiniz");
                }

            }
            if (TempData["sifre_degisim"] != null)
            {
                _notyf.Success("şifre başarıyla değiştirildi");
            }
            if (TempData["e_mail_gonderildi"] != null)
            {
                _notyf.Success("işlem başarılı lütfen e-posta adresinizi kontrol ediniz");
            }
            if (TempData["hesap_olustu"] != null)
            {
                _notyf.Success("hesap oluşturuldu mailinize gelen aktivasyon bağlantısına tıklayın");
            }
            if (TempData["isblock"] != null)
            {
                _notyf.Error("hesabın erişimi engellendi yöneticiler ile görüşün");
            }
            if (TempData["3dok"] != null)
            {
                _notyf.Success("ödeme başarılı randevularım alanında randevunuzu görüntüleyebilirsiniz");
            }
            if (TempData["3d"] != null)
            {
                _notyf.Error("3d ödeme tamalanamadı!");
            }

            List<CategoryListDto> categoryLists;

            if (!_memoryCache.TryGetValue(cate_cache_key, out categoryLists))
            {
                var categorydata = await categoryService.GetCategories();

                Console.WriteLine("###### DATA TEMİZLENDİ " + DateTime.Now.ToString());
                categoryLists = categorydata!.Data;
                MemoryCacheEntryOptions options = new MemoryCacheEntryOptions();



                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(600);

                _memoryCache.Set<List<CategoryListDto>>(cate_cache_key, categoryLists,options);
            }

            ViewBag.categories = categoryLists;

            List<PsychologistListDto> psyList;
            if (!_memoryCache.TryGetValue(psk_cache_key, out psyList))
            {
                var result = await psychologistService.GetPsycholistsWithCategort();
                psyList = result!.Data;
                MemoryCacheEntryOptions options = new MemoryCacheEntryOptions();

                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(600);

                _memoryCache.Set<List<PsychologistListDto>>(psk_cache_key, psyList,options);
            }
            List<Blog> blogs;

            if (!_memoryCache.TryGetValue(blog_cache_key, out blogs))
            {
                var result = await uow.GetRepository<Blog>().GetQueryable().OrderByDescending(x => x.PostWriteTime).Take(10).Select(x=>new Blog()
                {
                    Id=x.Id,
                    Author=x.Author,
                    ArticleUrl=x.ArticleUrl,
                    BlogImagePath=x.BlogImagePath,
                    Content=x.Content,
                    Description = x.Description,
                    Header= x.Header.Substring(0, 28) + " ...",
                    Key=x.Key,
                    PostWriteTime=x.PostWriteTime,
                    Title=x.Title
                    
                    


            }).ToListAsync();
                blogs = result;
                MemoryCacheEntryOptions options = new MemoryCacheEntryOptions();

                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(600);

                _memoryCache.Set<List<Blog>>(blog_cache_key, blogs,options);
            }

         

            ViewBag.blogs = blogs;
            return View(psyList);
        }
        [Authorize(Roles ="admin")]
        public async Task<IActionResult> DeleteComment(int id)
        {

            var data = await uow.GetRepository<Appointment>()
                .GetQueryable()
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound();

            }

            data.UserAppointmentComment = null;

            uow.GetRepository<Appointment>().Update(data);
            await uow.CommitAsync();

            return Ok();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult Status(int? code)
        {
            return View();
        }
        public IActionResult FAQ()
        {
            return View();
        }
        public IActionResult Contact() 
        {
            if (TempData["error-user-message"] != null)
            {
                _notyf.Error("mesaj gönderilemedi bilgilerinizi kontrol edin");
            }
            if (TempData["ss-user-message"] !=null)
            {
                _notyf.Success("mesaj gönderildi en kısa sürede yanıt verilecek");
            }
            if(TempData["ss-time-user-message"] != null)
            {
                _notyf.Warning("Tekrar mesaj gönderebilmek için 10dakika bekleyiniz");
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Contact(Message message)
        {
            if (!ModelState.IsValid)
            {
                TempData["error-user-message"] = "ss";
               return RedirectToAction("Contact");
            }

            var cookiedata = HttpContext.Request.Cookies["msg-time"];

            if (cookiedata != null)
            {
                var time =  Convert.ToDateTime(cookiedata);

                if (time.AddMinutes(10) < DateTime.Now )
                {

                    var mappeddata = mapper.Map<Message>(message);


                   var ip =  HttpContext?.Connection.RemoteIpAddress?.ToString() ?? " ";


                    mappeddata.UserMessage += Environment.NewLine +$"[Tarih] {DateTime.Now.ToString()} [IP ADRES] [{ip}]";


                    uow.GetRepository<Message>().Add(mappeddata);
                    await uow.CommitAsync();

                    TempData["ss-time-user-message"] = "ss";
                    return RedirectToAction("Contact");

                }
                else
                {
                    TempData["error-user-message"] = "ss";
                    return RedirectToAction("Contact");
                }

            }

            else
            {
                var mappeddata = mapper.Map<Message>(message);

                var ip = HttpContext?.Connection.RemoteIpAddress?.ToString() ?? " ";


                mappeddata.UserMessage += Environment.NewLine + $" [Tarih] {DateTime.Now.ToString()} [IP ADRES] [{ip}]";


                uow.GetRepository<Message>().Add(mappeddata);
                await uow.CommitAsync();

                HttpContext.Response.Cookies.Append("msg-time", DateTime.Now.ToString(), new CookieOptions()
                {
                    Expires = DateTime.Now.AddMinutes(12)
                });

                TempData["ss-user-message"] = "ss";
                return RedirectToAction("Contact");
            }

          

           
        }
        public IActionResult OnamFormu()
        {
            return View();

        }
        public IActionResult KVKK()
        {
            return View();
        }
        public  IActionResult Gizlilik()
        {
            return View();
        }



        [Route("/sitemap.xml")]
        public async Task SitemapXml()
        {


          var data =  await uow.GetRepository<Blog>().GetQueryable().ToArrayAsync();
            //var data2 =  await uow.GetRepository<Psychologist>().GetQueryable().Include(p=>p.AppUser).ToArrayAsync();

            var pskData = await psychologistService.GetPsycholistsWithCategort();

            string host = Request.Scheme + "://" + Request.Host;

            Response.ContentType = "application/xml";

            using (var xml = XmlWriter.Create(Response.Body, new XmlWriterSettings { Indent = true }))
            {
                xml.WriteStartDocument();
                xml.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");

                xml.WriteStartElement("url");
                xml.WriteElementString("loc", host);
                xml.WriteElementString("priority", "1");
                xml.WriteEndElement();


                if (data != null)
                {

                    foreach (var item in data)
                    {
                        xml.WriteStartElement("url");
                        xml.WriteElementString("loc", host+$"/Blog/Article/{item.ArticleUrl}");
                        xml.WriteElementString("priority", "1");
                        xml.WriteEndElement();
                    }

                }



                if (pskData != null)
                {

                    foreach (var item in pskData.Data)
                    {
                        xml.WriteStartElement("url");

                        string pskName = item.AppUser.Name.Trim() + " " + item.AppUser.LastName.Trim();
                        xml.WriteElementString("loc", host + $"/Psychologist/PsychologistInfo/{ConvertTurkishToEnglish(pskName)}/{item.SecondKey}");
                        xml.WriteElementString("priority", "1");
                        xml.WriteEndElement();
                    }

                }



        


                xml.WriteStartElement("url");
                xml.WriteElementString("loc", host+ "/Psychologist");
                xml.WriteElementString("priority", "1");
                xml.WriteEndElement();



                xml.WriteStartElement("url");
                xml.WriteElementString("loc", host + "/Home/FAQ");
                xml.WriteElementString("priority", "1");
                xml.WriteEndElement();



                xml.WriteStartElement("url");
                xml.WriteElementString("loc", host + "/Home/Contact");
                xml.WriteElementString("priority", "1");
                xml.WriteEndElement();


                xml.WriteStartElement("url");
                xml.WriteElementString("loc", host + "/Account/SignUp");
                xml.WriteElementString("priority", "1");
                xml.WriteEndElement();


                xml.WriteStartElement("url");
                xml.WriteElementString("loc", host + "/Account/login");
                xml.WriteElementString("priority", "1");
                xml.WriteEndElement();



                xml.WriteStartElement("url");
                xml.WriteElementString("loc", host + "/Blog/Index");
                xml.WriteElementString("priority", "1");
                xml.WriteEndElement();



                xml.WriteStartElement("url");
                xml.WriteElementString("loc", host + "/Psychologist/Index");
                xml.WriteElementString("priority", "1");
                xml.WriteEndElement();


                xml.WriteEndElement();


                xml.WriteEndDocument();
                xml.Flush();
                xml.Close();


            }
        }
        public IActionResult AboutUs()
        {
            return View();
        } 
        public IActionResult BackOrder()
        {
            return View();
        }
        public IActionResult OurServices()
        {
            return View();
        }






        string ConvertTurkishToEnglish(string turkishText)
        {
            StringBuilder englishText = new StringBuilder();

            turkishText = turkishText.Replace(" ", "-").ToLower();

            foreach (char c in turkishText)
            {
                switch (c)
                {
                    case 'ı':
                        englishText.Append('i');
                        break;

                    case 'ş':
                        englishText.Append('s');
                        break;
                    case 'ğ':
                        englishText.Append('g');
                        break;
                    case 'ç':
                        englishText.Append('c');
                        break;
                    case 'ö':
                        englishText.Append('o');
                        break;
                    case 'ü':
                        englishText.Append('u');
                        break;
                    default:
                        englishText.Append(c);
                        break;
                }
            }

            return englishText.ToString();
        }




    }
}
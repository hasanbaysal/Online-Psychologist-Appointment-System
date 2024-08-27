using HB.OnlinePsikologMerkezi.Data.Interface;
using HB.OnlinePsikologMerkezi.Entities.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HB.OnlinePsikologMerkezi.Web.Controllers
{
    public class BlogController : Controller
    {

        //blog add
        //blog update
        //blog active - passive
        //get blog by name
        //blog delete
        //get recent blogs
        private readonly IUow uow;

        public BlogController(IUow uow)
        {
            this.uow = uow;
        }


        //[HttpGet("{page?}")]
        [HttpGet("Blog/Index/{page?}")]
        public async Task<IActionResult> Index(int page=1)
        {



            var data = await uow.GetRepository<Blog>().GetQueryable().OrderByDescending(x=>x.PostWriteTime).Skip((page - 1) * 6).Take(6).ToListAsync();

            ViewBag.currentPage = page;

            if (data == null)
            {
                var data2 = await uow.GetRepository<Blog>().GetQueryable().OrderByDescending(x => x.PostWriteTime).Skip((0) * 6).Take(6).ToListAsync();
                ViewBag.currentPage = 1;
                return View(data2);

            }
            if (data.Count == 0)
            {
                var data2 = await uow.GetRepository<Blog>().GetQueryable().OrderByDescending(x => x.PostWriteTime).Skip((0) * 6).Take(6).ToListAsync();
                ViewBag.currentPage = 1;
                return View(data2);
            }

            return View(data);


        }

        [HttpGet("Blog/Article/{myarticlekey}")]
        public async Task<IActionResult> Article(string myarticlekey)
        {


            var routes = Request.RouteValues["myarticlekey"];

            var data = await uow.GetRepository<Blog>().GetQueryable().Where(x => x.ArticleUrl == routes).FirstOrDefaultAsync();

            //if (data == null)
            //{
            //    return notfoun
            //}



            return View(data);
        }


        

    }
}

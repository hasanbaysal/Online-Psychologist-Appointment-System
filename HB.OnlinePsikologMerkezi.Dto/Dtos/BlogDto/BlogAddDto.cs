using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.OnlinePsikologMerkezi.Dto.Dtos
{
    public class BlogAddDto
    {


        #region meta tag

        // <meta name = "robots" content="index, follow">

        //<link rel="canonical" href="orijinal-gonderi-url">
        [Required]
        public string? ArticleUrl { get; set; } = null!;

        //<title>Başlık Etiketi</title>
        [Required]
        public string? Title { get; set; }

        //<meta name="keywords" content="anahtar kelime1, anahtar kelime2, anahtar kelime3">
        [Required]
        public string? Key { get; set; }

        //<meta name="description" content="Gönderi açıklaması">
        [Required]
        public string? Description { get; set; }


        #endregion

        [Required]
        public string? Header { get; set; } = null!;
        public DateTime? PostWriteTime { get; set; }
        [Required]
        public string? Author { get; set; }
        ///html içerik <summary>
        /// html içerik
        /// </summary>

        [Required]
        public string? Content { get; set; } = null!;

        [ValidateNever]
        public string? BlogImagePath { get; set; }


        [ValidateNever]
        public IFormFile? Image { get; set; }



    }
}

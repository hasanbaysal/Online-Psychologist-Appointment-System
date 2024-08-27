using HB.OnlinePsikologMerkezi.Entities.Interface;

namespace HB.OnlinePsikologMerkezi.Entities.Entities
{
    public class Blog : IBaseEntity
    {


        public int Id { get; set; }

        #region meta tag

        // <meta name = "robots" content="index, follow">

        //<link rel="canonical" href="orijinal-gonderi-url">
        public string ArticleUrl { get; set; } = null!;

        //<title>Başlık Etiketi</title>
        public string? Title { get; set; }

        //<meta name="keywords" content="anahtar kelime1, anahtar kelime2, anahtar kelime3">
        public string? Key { get; set; }

        //<meta name="description" content="Gönderi açıklaması">
        public string? Description { get; set; }


        #endregion


        public string? Header { get; set; } = null!;
        public DateTime? PostWriteTime { get; set; }

        public string? Author { get; set; }
        ///html içerik
        public string? Content { get; set; } = null!;

        public string? BlogImagePath { get; set; }


    }

}

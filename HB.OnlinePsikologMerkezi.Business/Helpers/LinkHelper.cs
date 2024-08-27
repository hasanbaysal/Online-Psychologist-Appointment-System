using Microsoft.AspNetCore.Http;
using System.Web;

namespace HB.OnlinePsikologMerkezi.Business.Helpers
{

    public interface ILinkHelper
    {
        /// <summary>
        /// userid ve token değerlerini query string olarak yakalacağın 
        /// method'da isimlendirmelere dikkat et
        /// ve encode etmeyi unutma!
        /// userId ve  token şekelinde isimlendir
        /// </summary>
        /// <param name="action"></param>
        /// <param name="controller"></param>
        /// <param name="userid">HttpUtility.UrlDecode(userId)</param>
        /// <param name="token">HttpUtility.UrlDecode(token)</param>
        /// <returns></returns>
        string CrateLinkForActivationorResetPassword(string action, string controller, string userid, string token);
    }
    public class LinkHelper : ILinkHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LinkHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// userid ve token değerlerini query string olarak yakalacağın 
        /// method'da isimlendirmelere dikkat et
        /// ve encode etmeyi unutma!
        /// userId ve  token şekelinde isimlendir
        /// </summary>
        /// <param name="action"></param>
        /// <param name="controller"></param>
        /// <param name="userid">HttpUtility.UrlDecode(userId)</param>
        /// <param name="token">HttpUtility.UrlDecode(token)</param>
        /// <returns></returns>
        public string CrateLinkForActivationorResetPassword(string action, string controller, string userid, string token)
        {

            var request = _httpContextAccessor!.HttpContext!.Request;
            var baseUrl = $"{request.Scheme}://{request.Host.Value}";
            var uriBuilder = new UriBuilder(baseUrl);

            uriBuilder.Path = $"/{controller}/{action}";

            var queryString = HttpUtility.ParseQueryString(string.Empty);



            //System.Net.WebUtility.UrlDecode => güncel versiyon

            queryString["userId"] = HttpUtility.UrlEncode(userid);
            queryString["token"] = HttpUtility.UrlEncode(token);

            uriBuilder.Query = queryString.ToString();

            return uriBuilder.ToString();
        }



    }

}

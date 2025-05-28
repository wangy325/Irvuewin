using System.Net.Http;

namespace Irvuewin.Helpers
{

    ///<summary>
    ///Author: wangy325
    ///Date: 2020/01/01 18:18:01
    ///Desc: 
    ///</summary>
    public interface IHttpService
    {
        Task<HttpResponseMessage> GetAsync(string url);

        HttpClient Client();
    }
    
    public class HttpClientWrapper : IHttpService
    {
        private readonly HttpClient _httpClient = new();
    
        public Task<HttpResponseMessage> GetAsync(string url) => _httpClient.GetAsync(url);
    

        // public HttpClient Client { get; }

        public HttpClient Client()
        {
            return _httpClient;
        }
    }
}
using System.Net.Http;
using System.Net.Http.Headers;

namespace PR.Persistence.APIClient
{
    public static class ApiHelper
    {
        public static HttpClient ApiClient { get; set; }

        static ApiHelper()
        {
            ApiClient = new HttpClient();
            ApiClient.DefaultRequestHeaders.Accept.Clear();
            ApiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
    }
}

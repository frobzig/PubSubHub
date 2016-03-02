using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using PubSubHub.Models;
using Newtonsoft.Json;

namespace PubSubHub
{
    public static class HttpPusher
    {
        public async static Task<bool> PublishMessage(Uri dest, IPubSubMessage message)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, dest);

                    request.Content = new StringContent(JsonConvert.SerializeObject(message));
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    HttpResponseMessage response = await client.SendAsync(request);
                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

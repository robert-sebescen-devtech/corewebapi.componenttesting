using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Api.ThirdParty;

namespace Tests
{
    public static class Extensions
    {
        public static T As<T>(this HttpContent httpContent)
        {
            var responseData = httpContent.ReadAsStringAsync().GetAwaiter().GetResult();
            var response = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(responseData);

            return response;
        }
    }
}

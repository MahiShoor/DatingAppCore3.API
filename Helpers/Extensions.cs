using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingAppCore3.API.Helpers
{
    public static class Extensions
    {
        public static void AdApplicationError(this HttpResponse response,string massage)
        {

            response.Headers.Add("Application-Error", massage);
            response.Headers.Add("Access-Control-Expose-Headers", "Application-Error");
            response.Headers.Add("Acccess-Control-Allow-Origin", "*");

        }
    }
}

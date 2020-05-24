using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
         
        public static void AddPagination(this HttpResponse response , int currentPage, int itemsPerPage, int totalItems ,int totalPages)
        {
            var paginationHeader = new PaginationHeader(currentPage, itemsPerPage, totalItems, totalPages);
            var camelCaseFormatter = new JsonSerializerSettings();
            camelCaseFormatter.ContractResolver = new CamelCasePropertyNamesContractResolver();
            response.Headers.Add("Pagination", JsonConvert.SerializeObject(paginationHeader, camelCaseFormatter));
            response.Headers.Add("Access-Control-Expose-Headers", "Pagination ");
        }

        public static int CaculateAge(this DateTime dateTime)
        {
            var age = DateTime.Today.Year - dateTime.Year;
            if(dateTime.AddYears(age)> DateTime.Today)
            {
                age--;
            }
            return age;

        }
    }
}

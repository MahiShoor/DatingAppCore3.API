using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingAppCore3.API.Dtos
{
    public class UserForUpdateDto
    {
    
        public string Intoduction { get; set; }

        public string LokkingFor { get; set; }
        public string Interests { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }
}

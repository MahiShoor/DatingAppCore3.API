using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DatingAppCore3.API.Dtos
{
    public class UserForRegisterDto
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        [StringLength(8,MinimumLength =4,ErrorMessage ="Password length must be between 4 to 8 characters") ]
        public string Password { get; set; }

    }
}

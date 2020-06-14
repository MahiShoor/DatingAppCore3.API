using DatingAppCore3.API.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace DatingApp.API.Models
{
    public class User:IdentityUser<int>
    {
  
        public string  Gender { get; set; }

        public DateTime DateOfBirth { get; set; }
        public string KnownAs { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastActive { get; set; }

        public string Intoduction { get; set; }

        public string LokkingFor { get; set; }
        public string Interests { get; set; }
        public string City { get; set; }
        public string Country { get; set; }

        public virtual ICollection<Photo> Photos { get; set; }

        public virtual ICollection<Like> Likers { get; set; }
        public virtual ICollection<Like> Likees { get; set; }

        public virtual ICollection<Message> MessagesSent { get; set; }

        public virtual ICollection<Message> MessagesReceived { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; }

    }
}
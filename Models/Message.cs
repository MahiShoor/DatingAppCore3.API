using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Models;
using DatingAppCore3.API.Models;

namespace DatingAppCore3.API.Models
{
    public class Message
    {
        public int Id { get; set; }

        public int SenderId { get; set; }

        public int RecipientId { get; set; }

        public virtual User Sender { get; set; }

        public virtual User Recipient { get; set; }

        public string Content { get; set; }

        public bool IsRead { get; set; }

        public DateTime? DateRead { get; set; }

        public DateTime MassageSent { get; set; }

        public bool SenderDeleted { get; set; }

        public bool  Deleted { get; set; }

    }
}

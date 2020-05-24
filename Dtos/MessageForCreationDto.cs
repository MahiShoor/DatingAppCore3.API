using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingAppCore3.API.Dtos
{
    public class MessageForCreationDto
    {
        public int SenderId { get; set; }

        public int RecipientId { get; set; }

        public DateTime  MesssageSent { get; set; }

        public string Content { get; set; }


        public MessageForCreationDto()
        {
            MesssageSent = DateTime.Now;
        }

    }
}

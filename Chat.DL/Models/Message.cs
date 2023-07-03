using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat.DL.Models
{
    public class Message : BaseEntity
    {

        [ForeignKey("SenderId")]
        public string SenderId { get; set; }

        [ForeignKey("ReceiverId")]
        public string ReceiverId { get; set; }

        public DateTime Timestamp { get; set; }
        public string Content { get; set; }

        public virtual User User { get; set; }
    }
}

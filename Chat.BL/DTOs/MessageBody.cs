using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat.BL.DTOs
{
    public class MessageBody
    {

        public string? MessageId { get; set; }

        public string? ReceiverId { get; set; }

        public string? Content { get; set; }
    }

    public class ReqLog
    {
        public long? EndTime { get; set; }

        public long? StartTime { get; set; }

    }
}

 
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat.DL.Models
{
    public class Log:BaseEntity
    { 
        public string IpAddress { get; set; }
        public string RequestPath { get; set; }
        public string RequestBody { get; set; }
        public string Message { get; set; }
        public string Username { get; set; }
        public string Level { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

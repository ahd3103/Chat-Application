using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat.BL.DTOs
{
    public class LogResponse
    {
        public string IPOfCaller { get; set; }
        public string Method { get; set; }
        public string Path { get; set; }
        public QueryString QueryString { get; set; }
        public string RequestBody { get; set; }
        public long TimeOfCall { get; set; }
        public string UserName { get; set; }
    }
}

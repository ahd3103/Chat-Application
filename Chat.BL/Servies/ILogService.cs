using Chat.DL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat.BL.Servies
{
    public interface ILogService
    {
        IEnumerable<Log> GetLogs();
        Task<Log> AddLogs(Log logs); 
        Task<bool> DeleteLogs(Guid logId);
    }
}

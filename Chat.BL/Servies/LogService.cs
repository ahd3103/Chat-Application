using Chat.DL.Models;
using Chat.DL.Repostiory;

namespace Chat.BL.Servies
{
    public class LogService : ILogService
    {
        readonly IRepository<Log> _logRepository;
        public LogService(IRepository<Log> logRepository)
        {
            _logRepository = logRepository;
        }

        public async Task<Log> AddLogs(Log logs)
        {
            return await _logRepository.Insert(logs);
        }

        public Task<bool> DeleteLogs(Guid logId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Log> GetLogs()
        {
            return  _logRepository.GetAll();
        }
    }
}

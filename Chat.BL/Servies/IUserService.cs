using Chat.DL.Models;
 

namespace Chat.BL.Servies
{
    public interface IUserService
    {
        Task<User> Get(Guid id);
        Task<User> CheckUser(string email, string password);
        Task<IEnumerable<User>> GetAll();
        Task Insert(User user);
        Task<User> GetByEmail(string email);
    }
}

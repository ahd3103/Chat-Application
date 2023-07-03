using Chat.DL.DbContexts;
using Chat.DL.Models;
using Microsoft.EntityFrameworkCore;

namespace Chat.BL.Servies
{ 
        public class UserRepository : IUserService
        {
            private readonly ChatDbContext _context;

            public UserRepository(ChatDbContext context)
            {
                _context = context;
            }

            public async Task<User> Get(Guid id)
            {
                return await _context.Users.FindAsync(id);
            }

            public async Task<IEnumerable<User>> GetAll()
            {
                return await _context.Users.ToListAsync();
            }

            public async Task Insert(User user)
            {
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
            }

            public async Task<User> GetByEmail(string email)
            {
                return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            }

            public async Task<User> CheckUser(string email, string password)
            {
                // Find the user with the provided email and password
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.Password == password);

                return user;
            }
        }
}



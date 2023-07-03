
using Chat.DL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat.DL.DbContexts
{
    public class ChatDbContext :DbContext
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Log> LogEntries { get; set; }
        public override int SaveChanges()
        {
            return base.SaveChanges();
        }





    }

}

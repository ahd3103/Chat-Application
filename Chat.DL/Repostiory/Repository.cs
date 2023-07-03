using Chat.DL.DbContexts;
using Chat.DL.Models;
using Microsoft.EntityFrameworkCore;

namespace Chat.DL.Repostiory
{
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly ChatDbContext context;
        private readonly DbSet<T> entities;

        public Repository(ChatDbContext _context)
        {
            this.context = _context;
            entities = context.Set<T>();
        }

        public async Task<T> Get(Guid id)
        {
            return await entities.SingleOrDefaultAsync(s => s.Id == id);
        }

        public IEnumerable<T> GetAll()
        {
            return entities.AsEnumerable();
        }

        public async Task<T>  Insert(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            entities.Add(entity);
            context.SaveChanges();
            return entity;
        }

        public async Task InsertAll(List<T> entityList)
        {
            if (entityList == null)
            {
                throw new ArgumentNullException("entityList");
            }
            foreach (T entity in entities)
            {
                entities.Add(entity);
            }
            context.SaveChanges();
        }

        public async Task<T> Update(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            entities.Update(entity);
            context.SaveChanges();
            return entity;
        }

        public async Task Delete(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            entities.Remove(entity);
            context.SaveChanges();
        }

        public async Task DeleteAll(List<T> entityList)
        {
            if (entityList == null)
            {
                throw new ArgumentNullException("entityList");
            }
            foreach (T entity in entities)
            {
                entities.Remove(entity);
            }
            context.SaveChanges();
        }

        public async Task Remove(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            entities.Remove(entity);
        }

        public void SaveChanges()
        {   
            context.SaveChanges();  
        }
    }
}

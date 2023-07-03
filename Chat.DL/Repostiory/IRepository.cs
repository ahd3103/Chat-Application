using Chat.DL.Models;

namespace Chat.DL.Repostiory
{
    public interface IRepository<T> where T : BaseEntity
    {
        IEnumerable<T> GetAll();
        Task<T> Get(Guid id);
        Task<T> Insert(T entity);
        Task<T> Update(T entity);
        Task Delete(T entity);
        Task DeleteAll(List<T> entityList);
        Task Remove(T entity);
        Task InsertAll(List<T> entityList);
        void SaveChanges();
    }
}

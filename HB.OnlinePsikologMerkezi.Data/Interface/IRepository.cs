using HB.OnlinePsikologMerkezi.Entities.Interface;
using System.Linq.Expressions;

namespace HB.OnlinePsikologMerkezi.Data.Interface
{
    public interface IRepository<T> where T : IBaseEntity
    {
        Task<IEnumerable<T>> GetAllAsync(bool asnotracking);
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> filter);
        Task<T?> GetByFilterAsync(Expression<Func<T, bool>> filter);
        IQueryable<T> GetQueryable();
        void Add(T entity);
        void AddRange(List<T> values);
        void Delete(T entity);
        void Update(T entity);

        void Update(T enttiy, T unchanged);

    }
}

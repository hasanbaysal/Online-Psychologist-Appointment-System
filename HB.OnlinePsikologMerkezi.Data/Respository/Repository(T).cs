using HB.OnlinePsikologMerkezi.Data.Context;
using HB.OnlinePsikologMerkezi.Data.Interface;
using HB.OnlinePsikologMerkezi.Entities.Interface;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace HB.OnlinePsikologMerkezi.Data.Respository
{
    public class Repository<T> : IRepository<T> where T : class, IBaseEntity, new()
    {
        private readonly AppDbContext context;
        private DbSet<T> dbset;
        public Repository(AppDbContext context)
        {
            this.context = context;
            dbset = context.Set<T>();

        }

        public void Add(T entity)
        {
            dbset.Add(entity);
        }

        public void Delete(T entity)
        {
            dbset.Remove(entity);
        }
        public void Update(T entity)
        {
            dbset.Update(entity);
        }
        public async Task<IEnumerable<T>> GetAllAsync(bool asnotracking = false)
        {

            return asnotracking ?
                                 await dbset.ToListAsync() :
                                 await dbset.AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> filter)
        {
            return await dbset.Where(filter).ToListAsync();

        }

        public async Task<T?> GetByFilterAsync(Expression<Func<T, bool>> filter)
        {
            return await dbset.Where(filter).FirstOrDefaultAsync();
        }

        public IQueryable<T> GetQueryable()
        {
            return dbset.AsQueryable<T>();
        }

        public void AddRange(List<T> values)
        {
            dbset.AddRange(values);
        }

        public void Update(T enttiy, T unchanged)
        {


            context.Entry(unchanged!).CurrentValues.SetValues(enttiy);


        }
    }
}

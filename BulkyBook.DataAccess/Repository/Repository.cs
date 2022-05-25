using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _db;
        protected readonly DbSet<T> _set;

        public Repository(ApplicationDbContext db)
        {
            _db = db;
            _set = _db.Set<T>();
        }

        public void Add(T item)
        {
            _set.Add(item);
        }

        public IEnumerable<T> GetAll(string? includeProperties = null)
        {
            IQueryable<T> query = _set;
            if (includeProperties != null)
            {
                foreach (var property in includeProperties.Split(",", StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(property);
                }
            }
            return query.ToList();
        }

        public IEnumerable<T> Where(Expression<Func<T, bool>> filter, string? includeProperties = null)
        {
            var query = _set.Where(filter);
            if (includeProperties != null)
            {
                foreach (var prop in includeProperties.Split(','))
                {
                    query = query.Include(prop);
                }
            }
            return query;
        }

        public T GetFirstOrDefault(Expression<Func<T, bool>> filter, string? includeProperties = null, bool trackChanges = true)
        {
            IQueryable<T> query = _set;
            if (!trackChanges)
            {
                query = query.AsNoTracking();
            }
            if (includeProperties != null)
            {
                foreach (var property in includeProperties.Split(",", StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(property);
                }
            }
            return query.FirstOrDefault(filter);
        }

        public void Remove(T item)
        {
            _set.Remove(item);
        }

        public void RemoveRange(IEnumerable<T> items)
        {
            _set.RemoveRange(items);
        }
    }
}

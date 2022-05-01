using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll(string? includeProperties = null);

        IEnumerable<T> Where(Expression<Func<T, bool>> filter, string? includeProperties = null);

        /// <summary>
        /// Get first item matching specidied <paramref name="filter"/>
        /// conditions or null (if there is no such an item).
        /// </summary>
        /// <param name="filter">Lambda expression used to filter records.</param>
        /// <param name="includeProperties">A string containing navigation property names separated with ',' (coma).</param>
        /// <returns></returns>
        T GetFirstOrDefault(Expression<Func<T, bool>> filter, string? includeProperties = null);

        void Add(T item);

        void Remove(T item);

        void RemoveRange(IEnumerable<T> item);
    }
}

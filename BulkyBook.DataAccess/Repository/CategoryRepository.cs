using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext db)
            : base(db)
        {
        }

        public Category Find(int id)
        {
            return _set.Find(id);
        }

        public void Update(Category category)
        {
            _set.Update(category);
        }
    }
}

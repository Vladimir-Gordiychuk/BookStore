using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class CoverTypeRepository : Repository<CoverType>, ICoverTypeRepository
    {
        public CoverTypeRepository(ApplicationDbContext db)
            : base(db)
        {
        }

        public CoverType Find(int id)
        {
            return _set.Find(id);
        }

        public void Update(CoverType coverType)
        {
            _set.Update(coverType);
        }
    }
}

using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        public CompanyRepository(ApplicationDbContext db)
            : base(db)
        {
        }

        public Company Find(int id)
        {
            return _set.Find(id);
        }

        public void Update(Company company)
        {
            _set.Update(company);
        }
    }
}

using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        public ApplicationUserRepository(ApplicationDbContext db)
            : base(db)
        {
        }

        public ApplicationUser Find(string id)
        {
            return _set.Find(id);
        }

        public void Update(ApplicationUser user)
        {
            _set.Update(user);
        }
    }
}

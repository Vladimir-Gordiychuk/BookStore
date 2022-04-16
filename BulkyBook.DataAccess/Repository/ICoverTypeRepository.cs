using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public interface ICoverTypeRepository : IRepository<CoverType>
    {
        CoverType Find(int id);
        void Update(CoverType coverType);
    }
}

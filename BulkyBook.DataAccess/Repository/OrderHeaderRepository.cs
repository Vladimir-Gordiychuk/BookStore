using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        public OrderHeaderRepository(ApplicationDbContext db)
            : base(db)
        {
        }

        public OrderHeader Find(int id)
        {
            return _set.Find(id);
        }

        public void Update(OrderHeader order)
        {
            _set.Update(order);
        }
    }
}

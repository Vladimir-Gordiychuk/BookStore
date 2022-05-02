using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        public OrderDetailRepository(ApplicationDbContext db)
            : base(db)
        {
        }

        public OrderDetail Find(int id)
        {
            return _set.Find(id);
        }

        public void Update(OrderDetail order)
        {
            _set.Update(order);
        }
    }
}

using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        public ShoppingCartRepository(ApplicationDbContext db)
            : base(db)
        {
        }

        public ShoppingCart Find(int id)
        {
            return _set.Find(id);
        }

        public void Update(ShoppingCart cart)
        {
            _set.Update(cart);
        }
    }
}

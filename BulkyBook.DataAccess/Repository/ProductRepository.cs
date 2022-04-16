using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext db)
            : base(db)
        {
        }

        public Product Find(int id)
        {
            return _set.Find(id);
        }

        public void Update(Product product)
        {
            if (product.ImageUrl != null)
            {
                _set.Update(product);
            }
            else
            {
                var target = _set.Find(product);

                target.Title = product.Title;
                target.ISBN = product.ISBN;
                target.ListPrice = product.ListPrice;
                target.Price = product.Price;
                target.Price50 = product.Price50;
                target.Price100 = product.Price100;
                target.CategoryId = product.CategoryId;
                target.CoverTypeId = product.CoverTypeId;
            }
        }
    }
}

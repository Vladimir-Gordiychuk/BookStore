using BulkyBook.Models;

namespace BulkyBook.DataAccess.Repository
{
    public class ImageRepository : Repository<Image>, IImageRepository
    {
        public ImageRepository(ApplicationDbContext db)
            : base(db)
        {
        }

        public Image Find(int id)
        {
            return _set.Find(id);
        }

        public void Update(Image order)
        {
            _set.Update(order);
        }
    }
}

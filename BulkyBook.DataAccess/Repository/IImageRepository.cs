using BulkyBook.Models;

namespace BulkyBook.DataAccess.Repository
{
    public interface IImageRepository : IRepository<Image>
    {
        Image Find(int id);
        void Update(Image order);
    }
}

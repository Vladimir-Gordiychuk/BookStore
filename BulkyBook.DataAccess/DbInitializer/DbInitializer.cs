using BulkyBook.DataAccess.Repository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BulkyBook.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;

        public DbInitializer(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }

        public void Initialize()
        {
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex)
            {

            }

            if (!_roleManager.RoleExistsAsync(SD.RoleAdmin).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.RoleAdmin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.RoleEmployee)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.RoleCustomerIdividual)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.RoleCustomerCompany)).GetAwaiter().GetResult();

                var admin = new ApplicationUser()
                {
                    UserName = "admin@bulkybook.com",
                    Email = "admin@bulkybook.com",
                    Name = "Admin",
                    PhoneNumber = "987654321",
                    StreetAddress = "",
                    City = "Kiev",
                    State = "Ukraine",
                    PostalCode = "54321",
                };

                _userManager.CreateAsync(admin, "Admin@123").GetAwaiter().GetResult();

                var user = _db.Users.FirstOrDefault(record => record.Email == admin.Email);

                _userManager.AddToRoleAsync(user, SD.RoleAdmin).GetAwaiter().GetResult();
            }
        }
    }
}

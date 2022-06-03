using BulkyBook.Config;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BulkyBook.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;
        private readonly AdminConfig _config;

        public DbInitializer(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db,
            IOptions<AdminConfig> config)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
            _config = config.Value;
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
                    UserName = _config.Login,
                    Email = _config.Email,
                    Name = _config.Name,
                    PhoneNumber = "",
                    StreetAddress = "",
                    City = "",
                    State = "",
                    PostalCode = "",
                };

                _userManager.CreateAsync(admin, _config.Password).GetAwaiter().GetResult();

                var user = _db.Users.FirstOrDefault(record => record.Email == admin.Email);

                _userManager.AddToRoleAsync(user, SD.RoleAdmin).GetAwaiter().GetResult();
            }
        }
    }
}

namespace Infrastructure.Identity.Seeders
{
    using Bogus;
    using Domain.Enums;
    using Infrastructure.Identity.Entities;
    using Microsoft.AspNetCore.Identity;
    using System.Linq;
    using System.Threading.Tasks;

    public static class pdevitoSeeder
    {
        public static async Task SeedUserAsync(UserManager<ApplicationUser> userManager)
        {
            var defaultUser = new ApplicationUser
            {
                UserName = $"pdevito3",
                Email = "pdevito3@gmail.com",
                FirstName = "Paul",
                LastName = "DeVito",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };

            if (userManager.Users.All(u => u.Id != defaultUser.Id))
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "this is a long password 2");
                    await userManager.AddToRoleAsync(defaultUser, Role.Basic.ToString());
                    await userManager.AddToRoleAsync(defaultUser, Role.Admin.ToString());
                    await userManager.AddToRoleAsync(defaultUser, Role.SuperAdmin.ToString());
                }
            }
        }
    }
}
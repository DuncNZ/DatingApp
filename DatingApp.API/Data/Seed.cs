using System.Collections.Generic;
using System.Linq;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace DatingApp.API.Data
{
    public class Seed
    {
        // public static void SeedUsers(DataContext context)
        public static void SeedUsers(UserManager<User> userManager, RoleManager<Role> roleManager)
        {
           // if (!context.Users.Any())
           if (!userManager.Users.Any())
            {
                var userData = System.IO.File.ReadAllText("Data/UserSeedData.json");
                var users = JsonConvert.DeserializeObject<List<User>>(userData);

                // Create some roles
                var roles = new List<Role>
                {
                    new Role{ Name = "Member" },
                    new Role{ Name = "Admin" },
                    new Role{ Name = "Moderator" },
                    new Role{ Name = "VIP" }
                };

                foreach (var role in roles)
                {
                    roleManager.CreateAsync(role).Wait();
                }

                foreach (var user in users)
                {
                    userManager.CreateAsync(user, "password").Wait();
                    userManager.AddToRoleAsync(user, "Member").Wait();

                    // byte[] passwordHash, passwordSalt;
                    // CreatePasswordHash("password", out passwordHash, out passwordSalt);

                    // user.PasswordHash = passwordHash;
                    // user.PasswordSalt = passwordSalt;
                    // user.UserName = user.UserName.ToLowerInvariant();

                    // context.Users.Add(user);
                }

                var adminUser = new User
                {
                    UserName = "Admin"
                };

                userManager.CreateAsync(adminUser, "password").Wait();
                var admin = userManager.FindByNameAsync("Admin").Result;

                userManager.AddToRolesAsync(admin, new[] { "Admin", "Moderator" }).Wait();


                // var result = userManager.CreateAsync(adminUser, "password").Result;

                // if (result.Succeeded)
                // {
                //     var admin = userManager.FindByNameAsync("Admin").Result;
                //     userManager.AddToRolesAsync(admin, new[] { "Admin", "Moderator" });
                // }

                // context.SaveChanges();

            }

        }

        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
    }
}
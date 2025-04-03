using IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Data
{
    public class SeedData
    {
        private static readonly string doctorEmail = "doctor@example.com"; //Identifiant de connexion du médecin
        private static readonly string doctorPassword = "Doctor@123"; //Mot de passe du médecin

        public static async void Initialize(IServiceProvider serviceProvider)
        {
            #region ----- INITIALIZATION -----

            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            dbContext.Database.Migrate();

            #endregion ----- INITIALIZATION -----

            #region ----- ADDING USERS -----

            // Récupérer les services UserManager et RoleManager
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Vérifier si le rôle "Doctor" existe, sinon le créer
            if (!await roleManager.RoleExistsAsync("Doctor"))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole("DOCTOR"));
                if (!roleResult.Succeeded)
                {
                    foreach (var error in roleResult.Errors)
                    {
                        Console.WriteLine($"Error creating Doctor role: {error.Description}");
                    }
                }
            }

            if (await userManager.FindByEmailAsync("doctor@example.com") == null)
            {
                var user = new ApplicationUser
                {
                    UserName = doctorEmail,
                    Email = doctorEmail,
                    EmailConfirmed = true,
                    LockoutEnabled = false
                };
                var result = await userManager.CreateAsync(user, doctorPassword);

                if (result.Succeeded)
                {
                    // Ajout de l'utilisateur au rôle "Doctor"
                    var addToRoleResult = await userManager.AddToRoleAsync(user, "Doctor");
                    if (addToRoleResult.Succeeded)
                    {
                        Console.WriteLine("DoctorUser user created successfully and added to Doctor role.");
                    }
                    else
                    {
                        foreach (var error in addToRoleResult.Errors)
                        {
                            Console.WriteLine($"Error adding user to Doctor role: {error.Description}");
                        }
                    }
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"Error: {error.Description}");
                    }
                }

                #endregion
            }
        }
    }
}

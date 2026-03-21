using FoodSafetyInspection.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FoodSafetyInspection.MVC.Data
{
    public static class SeedData
    {
        public static async Task InitialiseAsync(IServiceProvider serviceProvider)
        {
            using var context = new AppDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>());

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // Seed Roles
            string[] roles = { "Admin", "Inspector", "Viewer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Seed Users
            await CreateUser(userManager, "admin@foodsafety.com", "Letmein01*", "Admin");
            await CreateUser(userManager, "inspector@foodsafety.com", "Letmein01*", "Inspector");
            await CreateUser(userManager, "viewer@foodsafety.com", "Letmein01*", "Viewer");

            // Seed Premises
            if (!context.Premises.Any())
            {
                var premises = new List<Premises>
                {
                    new Premises { Name = "McDonald's Grafton Street", Address = "62 Grafton St", Town = "Dublin", RiskRating = "Medium" },
                    new Premises { Name = "Bunsen", Address = "22 Wexford St", Town = "Dublin", RiskRating = "Medium" },
                    new Premises { Name = "Base Wood Fired Pizza", Address = "50 Dame St", Town = "Dublin", RiskRating = "Low" },
                    new Premises { Name = "Wetherspoon - The Silver Penny", Address = "9-10 Lower Abbey St", Town = "Dublin", RiskRating = "Medium" },
                    new Premises { Name = "Supermac's", Address = "23 O'Connell St Upper", Town = "Dublin", RiskRating = "Medium" },
                    new Premises { Name = "Boojum", Address = "63 Abbey St Upper", Town = "Dublin", RiskRating = "High" },
                    new Premises { Name = "Milano", Address = "1 Dawson St", Town = "Dublin", RiskRating = "Low" },
                    new Premises { Name = "Eddie Rocket's", Address = "7 Dame St", Town = "Dublin", RiskRating = "Medium" },
                    new Premises { Name = "Camile Thai", Address = "34 Exchequer St", Town = "Dublin", RiskRating = "High" },
                    new Premises { Name = "Zizzi", Address = "17 Suffolk St", Town = "Dublin", RiskRating = "Low" },
                    new Premises { Name = "Five Guys", Address = "1 South Anne St", Town = "Dublin", RiskRating = "Medium" },
                    new Premises { Name = "Leo Burdock", Address = "4 Crown Alley", Town = "Dublin", RiskRating = "High" },
                };
                context.Premises.AddRange(premises);
                await context.SaveChangesAsync();
            }

            // Seed Inspections
            if (!context.Inspections.Any())
            {
                var inspections = new List<Inspection>
                {
                    new Inspection { PremisesId = 1, InspectionDate = DateTime.Now.AddDays(-5), Score = 45, Outcome = "Fail", Notes = "Poor hygiene standards." },
                    new Inspection { PremisesId = 1, InspectionDate = DateTime.Now.AddDays(-60), Score = 72, Outcome = "Pass", Notes = "Generally acceptable." },
                    new Inspection { PremisesId = 2, InspectionDate = DateTime.Now.AddDays(-10), Score = 88, Outcome = "Pass", Notes = "Good standards." },
                    new Inspection { PremisesId = 2, InspectionDate = DateTime.Now.AddDays(-90), Score = 40, Outcome = "Fail", Notes = "Rodent evidence found." },
                    new Inspection { PremisesId = 3, InspectionDate = DateTime.Now.AddDays(-3), Score = 91, Outcome = "Pass", Notes = "Excellent." },
                    new Inspection { PremisesId = 3, InspectionDate = DateTime.Now.AddDays(-120), Score = 55, Outcome = "Fail", Notes = "Storage issues." },
                    new Inspection { PremisesId = 4, InspectionDate = DateTime.Now.AddDays(-7), Score = 60, Outcome = "Fail", Notes = "Temperature violations." },
                    new Inspection { PremisesId = 4, InspectionDate = DateTime.Now.AddDays(-45), Score = 78, Outcome = "Pass", Notes = "Improved since last visit." },
                    new Inspection { PremisesId = 5, InspectionDate = DateTime.Now.AddDays(-2), Score = 95, Outcome = "Pass", Notes = "Outstanding cleanliness." },
                    new Inspection { PremisesId = 5, InspectionDate = DateTime.Now.AddDays(-30), Score = 50, Outcome = "Fail", Notes = "Expired products found." },
                    new Inspection { PremisesId = 6, InspectionDate = DateTime.Now.AddDays(-15), Score = 83, Outcome = "Pass", Notes = "Well maintained." },
                    new Inspection { PremisesId = 6, InspectionDate = DateTime.Now.AddDays(-75), Score = 42, Outcome = "Fail", Notes = "Dirty kitchen surfaces." },
                    new Inspection { PremisesId = 7, InspectionDate = DateTime.Now.AddDays(-1), Score = 37, Outcome = "Fail", Notes = "Serious hygiene breach." },
                    new Inspection { PremisesId = 7, InspectionDate = DateTime.Now.AddDays(-50), Score = 80, Outcome = "Pass", Notes = "Satisfactory." },
                    new Inspection { PremisesId = 8, InspectionDate = DateTime.Now.AddDays(-8), Score = 77, Outcome = "Pass", Notes = "Minor issues noted." },
                    new Inspection { PremisesId = 8, InspectionDate = DateTime.Now.AddDays(-100), Score = 48, Outcome = "Fail", Notes = "No handwashing facilities." },
                    new Inspection { PremisesId = 9, InspectionDate = DateTime.Now.AddDays(-4), Score = 69, Outcome = "Pass", Notes = "Acceptable standards." },
                    new Inspection { PremisesId = 9, InspectionDate = DateTime.Now.AddDays(-55), Score = 35, Outcome = "Fail", Notes = "Pest control needed." },
                    new Inspection { PremisesId = 10, InspectionDate = DateTime.Now.AddDays(-6), Score = 90, Outcome = "Pass", Notes = "Very clean." },
                    new Inspection { PremisesId = 10, InspectionDate = DateTime.Now.AddDays(-80), Score = 44, Outcome = "Fail", Notes = "Food stored incorrectly." },
                    new Inspection { PremisesId = 11, InspectionDate = DateTime.Now.AddDays(-9), Score = 85, Outcome = "Pass", Notes = "Good practices observed." },
                    new Inspection { PremisesId = 11, InspectionDate = DateTime.Now.AddDays(-40), Score = 58, Outcome = "Fail", Notes = "Labelling issues." },
                    new Inspection { PremisesId = 12, InspectionDate = DateTime.Now.AddDays(-11), Score = 73, Outcome = "Pass", Notes = "Mostly compliant." },
                    new Inspection { PremisesId = 12, InspectionDate = DateTime.Now.AddDays(-65), Score = 41, Outcome = "Fail", Notes = "Cross contamination risk." },
                    new Inspection { PremisesId = 1, InspectionDate = DateTime.Now.AddDays(-20), Score = 66, Outcome = "Pass", Notes = "Improvement noted." },
                };
                context.Inspections.AddRange(inspections);
                await context.SaveChangesAsync();
            }

            // Seed FollowUps
            if (!context.FollowUps.Any())
            {
                var followUps = new List<FollowUp>
                {
                    new FollowUp { InspectionId = 1, DueDate = DateTime.Now.AddDays(-10), Status = "Open" },
                    new FollowUp { InspectionId = 4, DueDate = DateTime.Now.AddDays(-20), Status = "Open" },
                    new FollowUp { InspectionId = 6, DueDate = DateTime.Now.AddDays(-15), Status = "Open" },
                    new FollowUp { InspectionId = 7, DueDate = DateTime.Now.AddDays(7), Status = "Open" },
                    new FollowUp { InspectionId = 10, DueDate = DateTime.Now.AddDays(-5), Status = "Open" },
                    new FollowUp { InspectionId = 12, DueDate = DateTime.Now.AddDays(14), Status = "Open" },
                    new FollowUp { InspectionId = 13, DueDate = DateTime.Now.AddDays(-30), Status = "Open" },
                    new FollowUp { InspectionId = 16, DueDate = DateTime.Now.AddDays(-2), Status = "Closed", ClosedDate = DateTime.Now.AddDays(-1) },
                    new FollowUp { InspectionId = 18, DueDate = DateTime.Now.AddDays(-25), Status = "Closed", ClosedDate = DateTime.Now.AddDays(-10) },
                    new FollowUp { InspectionId = 20, DueDate = DateTime.Now.AddDays(-8), Status = "Closed", ClosedDate = DateTime.Now.AddDays(-3) },
                };
                context.FollowUps.AddRange(followUps);
                await context.SaveChangesAsync();
            }
        }

        private static async Task CreateUser(UserManager<IdentityUser> userManager, string email, string password, string role)
        {
            if (await userManager.FindByEmailAsync(email) == null)
            {
                var user = new IdentityUser { UserName = email, Email = email };
                await userManager.CreateAsync(user, password);
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }
}
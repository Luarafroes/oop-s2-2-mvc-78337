using FoodSafetyInspection.Domain;
using FoodSafetyInspection.MVC.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodSafetyInspection.Tests.Helpers
{
    public static class TestDbHelper
    {
        public static AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);

            // Seed test premises
            var premises1 = new Premises
            {
                Id = 1,
                Name = "Boojum",
                Address = "63 Abbey St Upper",
                Town = "Dublin",
                RiskRating = "High"
            };
            var premises2 = new Premises
            {
                Id = 2,
                Name = "Milano",
                Address = "1 Dawson St",
                Town = "Dublin",
                RiskRating = "Low"
            };
            context.Premises.AddRange(premises1, premises2);

            // Seed test inspections
            var inspection1 = new Inspection
            {
                Id = 1,
                PremisesId = 1,
                InspectionDate = DateTime.Today.AddDays(-5),
                Score = 40,
                Outcome = "Fail",
                Notes = "Hygiene standards below acceptable level"
            };
            var inspection2 = new Inspection
            {
                Id = 2,
                PremisesId = 2,
                InspectionDate = DateTime.Today.AddDays(-3),
                Score = 90,
                Outcome = "Pass",
                Notes = "Excellent cleanliness observed"
            };
            context.Inspections.AddRange(inspection1, inspection2);

            // Seed test follow-ups
            var followUp1 = new FollowUp
            {
                Id = 1,
                InspectionId = 1,
                DueDate = DateTime.Today.AddDays(-10), // overdue
                Status = "Open"
            };
            var followUp2 = new FollowUp
            {
                Id = 2,
                InspectionId = 1,
                DueDate = DateTime.Today.AddDays(5), // not overdue yet
                Status = "Open"
            };
            var followUp3 = new FollowUp
            {
                Id = 3,
                InspectionId = 2,
                DueDate = DateTime.Today.AddDays(-20), // overdue but closed
                Status = "Closed",
                ClosedDate = DateTime.Today.AddDays(-5)
            };
            context.FollowUps.AddRange(followUp1, followUp2, followUp3);

            context.SaveChanges();
            return context;
        }
    }
}
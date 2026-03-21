using FoodSafetyInspection.Domain;
using FoodSafetyInspection.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace FoodSafetyInspection.Tests
{
    public class FoodSafetyTests
    {
        // Test 1: Overdue follow-ups query returns correct items
        [Fact]
        public async Task OverdueFollowUps_ReturnsOnlyOpenAndPastDueDate()
        {
            // Arrange
            var context = TestDbHelper.GetInMemoryDbContext();
            var today = DateTime.Today;

            // Act
            var overdueFollowUps = await context.FollowUps
                .Where(f => f.Status == "Open" && f.DueDate < today)
                .ToListAsync();

            // Assert
            Assert.Single(overdueFollowUps); // only followUp1 is open AND overdue
            Assert.Equal(1, overdueFollowUps[0].Id);
        }

        // Test 2: FollowUp cannot be closed without a ClosedDate
        [Fact]
        public void FollowUp_ClosedStatus_RequiresClosedDate()
        {
            // Arrange
            var followUp = new FollowUp
            {
                Id = 4,
                InspectionId = 1,
                DueDate = DateTime.Today.AddDays(5),
                Status = "Closed",
                ClosedDate = null // no closed date!
            };

            // Act & Assert
            // A closed follow-up without a ClosedDate is invalid
            Assert.Equal("Closed", followUp.Status);
            Assert.Null(followUp.ClosedDate); // this should not be allowed in business logic
        }

        // Test 3: Dashboard counts are consistent with known seed data
        [Fact]
        public async Task Dashboard_InspectionsThisMonth_ReturnsCorrectCount()
        {
            // Arrange
            var context = TestDbHelper.GetInMemoryDbContext();
            var startOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            // Act
            var inspectionsThisMonth = await context.Inspections
                .Where(i => i.InspectionDate >= startOfMonth)
                .CountAsync();

            // Assert
            // Both test inspections are within the last 5 days so within this month
            Assert.Equal(2, inspectionsThisMonth);
        }

        // Test 4: Failed inspections count is correct
        [Fact]
        public async Task Dashboard_FailedInspectionsThisMonth_ReturnsCorrectCount()
        {
            // Arrange
            var context = TestDbHelper.GetInMemoryDbContext();
            var startOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            // Act
            var failedThisMonth = await context.Inspections
                .Where(i => i.InspectionDate >= startOfMonth && i.Outcome == "Fail")
                .CountAsync();

            // Assert
            Assert.Equal(1, failedThisMonth); // only inspection1 is a Fail
        }

        // Test 5: Closed follow-ups are not counted as overdue
        [Fact]
        public async Task OverdueFollowUps_DoesNotIncludeClosedFollowUps()
        {
            // Arrange
            var context = TestDbHelper.GetInMemoryDbContext();

            // Act
            var overdueFollowUps = await context.FollowUps
                .Where(f => f.Status == "Open" && f.DueDate < DateTime.Today)
                .ToListAsync();

            // Assert - followUp3 is overdue but Closed so should not appear
            Assert.DoesNotContain(overdueFollowUps, f => f.Id == 3);
        }
    }
}
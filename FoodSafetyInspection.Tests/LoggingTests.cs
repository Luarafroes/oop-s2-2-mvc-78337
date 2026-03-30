using FoodSafetyInspection.Domain;
using FoodSafetyInspection.MVC.Controllers;
using FoodSafetyInspection.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using Serilog.Sinks.InMemory;
using Serilog.Sinks.InMemory.Assertions;

namespace FoodSafetyInspection.Tests
{
    public class LoggingTests
    {
        private InspectionsController CreateController(ILogger<InspectionsController> logger)
        {
            var context = TestDbHelper.GetInMemoryDbContext();
            var controller = new InspectionsController(context, logger);

            // Set up a fake HttpContext so User.Identity works
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            return controller;
        }

        // Test 1: Creating an inspection logs an Information event
        [Fact]
        public async Task CreateInspection_LogsInformationEvent()
        {
            // Arrange - set up InMemory Serilog sink
            var inMemorySink = new InMemorySink();
            var logger = new LoggerConfiguration()
                .WriteTo.Sink(inMemorySink)
                .CreateLogger();

            var msLogger = new SerilogLoggerFactory(logger)
                .CreateLogger<InspectionsController>();

            var controller = CreateController(msLogger);

            var inspection = new Inspection
            {
                PremisesId = 1,
                InspectionDate = DateTime.Today,
                Score = 85,
                Outcome = "Pass",
                Notes = "Test inspection"
            };

            // Act
            await controller.Create(inspection);

            // Assert - check that an Information log was written
            inMemorySink.Should()
                .HaveMessage("Inspection created: ID {InspectionId} for PremisesId {PremisesId} Outcome {Outcome} by {User}")
                .Appearing().Once()
                .WithLevel(Serilog.Events.LogEventLevel.Information);
        }

        // Test 2: Creating a failed inspection logs a Warning event
        [Fact]
        public async Task CreateFailedInspection_LogsWarningEvent()
        {
            // Arrange
            var inMemorySink = new InMemorySink();
            var logger = new LoggerConfiguration()
                .WriteTo.Sink(inMemorySink)
                .CreateLogger();

            var msLogger = new SerilogLoggerFactory(logger)
                .CreateLogger<InspectionsController>();

            var controller = CreateController(msLogger);

            var inspection = new Inspection
            {
                PremisesId = 1,
                InspectionDate = DateTime.Today,
                Score = 30,
                Outcome = "Fail",
                Notes = "Failed inspection test"
            };

            // Act
            await controller.Create(inspection);

            // Assert - check that a Warning log was written for the fail
            inMemorySink.Should()
                .HaveMessage("Failed inspection recorded: ID {InspectionId} for PremisesId {PremisesId} Score {Score}")
                .Appearing().Once()
                .WithLevel(Serilog.Events.LogEventLevel.Warning);
        }

        // Test 3: FollowUp with due date before inspection date logs a Warning
        [Fact]
        public async Task CreateFollowUp_WithInvalidDueDate_LogsWarning()
        {
            // Arrange
            var inMemorySink = new InMemorySink();
            var logger = new LoggerConfiguration()
                .WriteTo.Sink(inMemorySink)
                .CreateLogger();

            var context = TestDbHelper.GetInMemoryDbContext();
            var msLogger = new SerilogLoggerFactory(logger)
                .CreateLogger<FollowUpsController>();

            var controller = new FollowUpsController(context, msLogger);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var followUp = new FollowUp
            {
                InspectionId = 1,
                DueDate = DateTime.Today.AddDays(-30), // before inspection date
                Status = "Open"
            };

            // Act
            await controller.Create(followUp);

            // Assert - warning should be logged for invalid due date
            inMemorySink.Should()
                .HaveMessage("FollowUp creation rejected: DueDate {DueDate} is before InspectionDate {InspectionDate} for InspectionId {InspectionId}")
                .Appearing().Once()
                .WithLevel(Serilog.Events.LogEventLevel.Warning);
        }
    }
}
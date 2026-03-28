using Xunit;
using FluentAssertions;
using PR.Domain.Entities.PR;
using PR.Domain.BusinessRules.PR;

namespace PR.Domain.UnitTest
{
    public class BusinessRuleCatalogTest
    {
        [Fact]
        public void Given_Person_When_PersonHasNoFirstName_Then_PersonIsRegardedAsInvalid()
        {
            // Arrange
            var businessRuleCatalog = new BusinessRuleCatalog();
            var person = new Person();

            // Act
            var result = businessRuleCatalog.ValidateAtomic(person);

            // Assert
            result.ContainsKey("FirstName").Should().BeTrue();
            result["FirstName"].Should().Be("First name is required");
        }

        [Fact]
        public void Given_Person_When_PersonHasNoStartDate_Then_PersonIsRegardedAsInvalid()
        {
            // Arrange
            var businessRuleCatalog = new BusinessRuleCatalog();
            var person = new Person();

            // Act
            var result = businessRuleCatalog.ValidateAtomic(person);

            // Assert
            result.ContainsKey("DateRange").Should().BeTrue();
            result["DateRange"].Should().Be("Start is required");
        }

        [Fact]
        public void Given_ListOfPersonVariants_When_TheirDateRangesOverlap_Then_ListIsRegardedAsInvalid()
        {
            // Arrange
            var businessRuleCatalog = new BusinessRuleCatalog();

            var timeIntervals = new List<Tuple<DateTime, DateTime>>
            {
                new(new DateTime(2002, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(2005, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
                new(new DateTime(2004, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(9999, 12, 31, 23, 59, 59, DateTimeKind.Utc))
            };

            // Act
            var result = businessRuleCatalog.ValidateCrossEntity(timeIntervals);

            // Assert
            result.ContainsKey("ValidTimeIntervals").Should().BeTrue();
            result["ValidTimeIntervals"].Should().Be("Valid time intervals overlapping");
        }
    }
}
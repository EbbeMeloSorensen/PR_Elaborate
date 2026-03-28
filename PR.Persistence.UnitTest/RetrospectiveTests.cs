using StructureMap;
using Xunit;
using FluentAssertions;
using PR.Domain.Entities.PR;
using PR.Persistence.Versioned;

namespace PR.Persistence.UnitTest
{
    public class RetrospectiveTests
    {
        private readonly UnitOfWorkFactoryFacade _unitOfWorkFactory;

        public RetrospectiveTests()
        {
            var container = Container.For<InstanceScanner>();

            var unitOfWorkFactory = container.GetInstance<IUnitOfWorkFactory>();
            unitOfWorkFactory.OverrideConnectionString("Data source=people_bitemporal.db");
            unitOfWorkFactory.Initialize(true);
            unitOfWorkFactory.Reseed();

            _unitOfWorkFactory = new UnitOfWorkFactoryFacade(unitOfWorkFactory);
            (_unitOfWorkFactory as UnitOfWorkFactoryFacade)!.DatabaseTime = null;
        }

        [Fact]
        public async Task CreateNewVariantForExistingPerson_PreceedingExistingVariants()
        {
            // Arrange
            var person = new Person
            {
                ID = new Guid("00000003-0000-0000-0000-000000000000"),
                FirstName = "Sprocket",
                Start = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                End = new DateTime(2022, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            };

            // Act
            using var unitOfWork1 = _unitOfWorkFactory.GenerateUnitOfWork();
            await unitOfWork1.People.Add(person);
            unitOfWork1.Complete();

            // Assert
        }

        [Fact]
        public async Task GetEarlierVersionOfPerson()
        {
            // Arrange
            _unitOfWorkFactory.DatabaseTime = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            using var unitOfWork = _unitOfWorkFactory.GenerateUnitOfWork();

            // Act
            var person = await unitOfWork.People.Get(
                new Guid("00000005-0000-0000-0000-000000000000"));

            // Assert
            person.FirstName.Should().Be("Chewing Gum");
        }

        [Fact]
        public async Task GetHistoricalStateOfPerson()
        {
            // Arrange
            _unitOfWorkFactory.HistoricalTime = new DateTime(2002, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            using var unitOfWork = _unitOfWorkFactory.GenerateUnitOfWork();

            // Act
            var person = await unitOfWork.People.Get(
                new Guid("00000004-0000-0000-0000-000000000000"));

            // Assert
            person.FirstName.Should().Be("Anakin Skywalker");
        }

        [Fact]
        public async Task GetCurrentPersonIncludingHistoricalComments()
        {
            // Arrange
            _unitOfWorkFactory.IncludeCurrentObjects = true;
            _unitOfWorkFactory.IncludeHistoricalObjects = true;
            using var unitOfWork = _unitOfWorkFactory.GenerateUnitOfWork();

            // Act
            var person = await unitOfWork.People.GetIncludingComments(
                new Guid("00000006-0000-0000-0000-000000000000"));

            // Assert
            person.FirstName.Should().Be("Rey Skywalker");
        }

        [Fact]
        public async Task GetHistoricalPersonIncludingHistoricalComments()
        {
            // Arrange
            _unitOfWorkFactory.IncludeCurrentObjects = true;
            _unitOfWorkFactory.IncludeHistoricalObjects = true;
            using var unitOfWork = _unitOfWorkFactory.GenerateUnitOfWork();

            // Act
            var person = await unitOfWork.People.GetIncludingComments(
                new Guid("00000004-0000-0000-0000-000000000000"));

            // Assert
            person.FirstName.Should().Be("Darth Vader");
            person.Comments.Count.Should().Be(3);
            person.Comments.Count(c => c.Text == "He is strong with the force").Should().Be(1);
            person.Comments.Count(c => c.Text == "Lives on Mustafar").Should().Be(1);
            person.Comments.Count(c => c.Text == "He is a cruel fellow").Should().Be(1);
        }

        [Fact]
        public async Task GetEarlierVersionOfPerson_BeforePersonWasCreated_Throws()
        {
            // Arrange
            _unitOfWorkFactory.DatabaseTime = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            using var unitOfWork1 = _unitOfWorkFactory.GenerateUnitOfWork();
            var id = new Guid("00000004-0000-0000-0000-000000000000");

            // Act & Assert
            var exception = await Record.ExceptionAsync(async () =>
            {
                var person = await unitOfWork1.People.Get(id);
                unitOfWork1.Complete();
            });

            Assert.NotNull(exception);
            exception.Message.Should().Be("Person doesn't exist");
        }

        [Fact]
        public async Task GetHistoricalStateOfPerson_BeforePersonExisted_Throws()
        {
            // Arrange
            _unitOfWorkFactory.HistoricalTime = new DateTime(2002, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            using var unitOfWork1 = _unitOfWorkFactory.GenerateUnitOfWork();
            var id = new Guid("00000005-0000-0000-0000-000000000000");

            // Act & Assert
            var exception = await Record.ExceptionAsync(async () =>
            {
                var person = await unitOfWork1.People.Get(id);
                unitOfWork1.Complete();
            });

            Assert.NotNull(exception);
            exception.Message.Should().Be("Person doesn't exist");
        }

        [Fact]
        public async Task GetEarlierVersionOfEntirePersonCollection()
        {
            // Arrange
            _unitOfWorkFactory.DatabaseTime = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            using var unitOfWork = _unitOfWorkFactory.GenerateUnitOfWork();

            // Act
            var people = await unitOfWork.People.GetAll();

            // Assert
            people.Count().Should().Be(4);
            people.Count(p => p.FirstName == "Max Rebo").Should().Be(1);
            people.Count(p => p.FirstName == "Chewing Gum").Should().Be(1);
            people.Count(p => p.FirstName == "Rey").Should().Be(1);
            people.Count(p => p.FirstName == "Wicket").Should().Be(1);
        }

        [Fact]
        public async Task GetHistoricStateOfEntirePersonCollection()
        {
            // Arrange
            _unitOfWorkFactory.HistoricalTime = new DateTime(2005, 1, 1, 1, 0, 0, DateTimeKind.Utc);
            using var unitOfWork = _unitOfWorkFactory.GenerateUnitOfWork();

            // Act
            var people = await unitOfWork.People.GetAll();

            // Assert
            people.Count().Should().Be(3);
            people.Count(p => p.FirstName == "Chewbacca").Should().Be(1);
            people.Count(p => p.FirstName == "Darth Vader").Should().Be(1);
            people.Count(p => p.FirstName == "Luke Skywalker").Should().Be(1);
        }

        [Fact]
        public async Task FindCurrentPeopleExclusively()
        {
            // Arrange
            _unitOfWorkFactory.IncludeHistoricalObjects = false;
            _unitOfWorkFactory.IncludeCurrentObjects = true;
            using var unitOfWork = _unitOfWorkFactory.GenerateUnitOfWork();

            // Act
            var people = await unitOfWork.People.Find(_ => _.FirstName.Contains("e"));

            // Assert
            people.Count().Should().Be(4);
            people.Count(p => p.FirstName == "Max Rebo").Should().Be(1);
            people.Count(p => p.FirstName == "Chewbacca").Should().Be(1);
            people.Count(p => p.FirstName == "Rey Skywalker").Should().Be(1);
            people.Count(p => p.FirstName == "Wicket").Should().Be(1);
        }

        [Fact]
        public async Task FindHistoricalPeopleExclusively()
        {
            // Arrange
            _unitOfWorkFactory.IncludeHistoricalObjects = true;
            _unitOfWorkFactory.IncludeCurrentObjects = false;
            using var unitOfWork = _unitOfWorkFactory.GenerateUnitOfWork();

            // Act
            var people = await unitOfWork.People.Find(_ => _.FirstName.Contains("e"));

            // Assert
            people.Count().Should().Be(2);
            people.Count(p => p.FirstName == "Darth Vader").Should().Be(1);
            people.Count(p => p.FirstName == "Luke Skywalker").Should().Be(1);
        }

        [Fact]
        public async Task FindCurrentAndHistoricalPeople()
        {
            // Arrange
            _unitOfWorkFactory.IncludeHistoricalObjects = true;
            _unitOfWorkFactory.IncludeCurrentObjects = true;
            using var unitOfWork = _unitOfWorkFactory.GenerateUnitOfWork();

            // Act
            var people = await unitOfWork.People.Find(_ => _.FirstName.Contains("e"));

            // Assert
            people.Count().Should().Be(6);
            people.Count(p => p.FirstName == "Max Rebo").Should().Be(1);
            people.Count(p => p.FirstName == "Luke Skywalker").Should().Be(1);
            people.Count(p => p.FirstName == "Chewbacca").Should().Be(1);
            people.Count(p => p.FirstName == "Rey Skywalker").Should().Be(1);
            people.Count(p => p.FirstName == "Darth Vader").Should().Be(1);
            people.Count(p => p.FirstName == "Wicket").Should().Be(1);
        }

        [Fact]
        public async Task FindHistoricalPeopleForAHistoricState()
        {
            // Arrange
            _unitOfWorkFactory.IncludeHistoricalObjects = true;
            _unitOfWorkFactory.IncludeCurrentObjects = false;
            _unitOfWorkFactory.HistoricalTime = new DateTime(2005, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            using var unitOfWork = _unitOfWorkFactory.GenerateUnitOfWork();

            // Act
            var people = await unitOfWork.People.Find(_ => _.FirstName.Contains("e"));

            // Assert
            people.Count().Should().Be(0);
        }

        [Fact]
        public async Task GetAllStatesOfAPerson()
        {
            // Arrange
            using var unitOfWork = _unitOfWorkFactory.GenerateUnitOfWork();

            // Act
            var people = await unitOfWork.People.GetAllVariants(new Guid("00000004-0000-0000-0000-000000000000"));

            // Assert
            people.Count().Should().Be(2);
            people.Count(p => p.FirstName == "Anakin Skywalker").Should().Be(1);
            people.Count(p => p.FirstName == "Darth Vader").Should().Be(1);
        }

        [Fact]
        public async Task RetroactivelyCorrectAnEarlierStateOfAPerson_ByChangingAnOrdinaryAttribute()
        {
            // Arrange
            _unitOfWorkFactory.HistoricalTime = new DateTime(2002, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            using var unitOfWork = _unitOfWorkFactory.GenerateUnitOfWork();

            var person = await unitOfWork.People.Get(
                new Guid("00000004-0000-0000-0000-000000000000"));

            person.FirstName = "Ani";

            // Act
            await unitOfWork.People.Correct(person);
            unitOfWork.Complete();

            // Assert
            using var unitOfWork2 = _unitOfWorkFactory.GenerateUnitOfWork();

            var personVariants =
                await unitOfWork2.People.GetAllVariants(new Guid("00000004-0000-0000-0000-000000000000"));

            personVariants.Count().Should().Be(2);
            personVariants.Count(p => p.FirstName == "Ani").Should().Be(1);
            personVariants.Count(p => p.FirstName == "Darth Vader").Should().Be(1);
        }

        [Fact]
        public async Task RetroactivelyCorrectAnEarlierStateOfAPerson_ByChangingTheValidTimeInterval()
        {
            // Arrange
            _unitOfWorkFactory.HistoricalTime = new DateTime(2002, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            using var unitOfWork = _unitOfWorkFactory.GenerateUnitOfWork();

            var person = await unitOfWork.People.Get(
                new Guid("00000004-0000-0000-0000-000000000000"));

            person.Start = new DateTime(1992, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            person.End = new DateTime(1997, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // Act
            await unitOfWork.People.Correct(person);
            unitOfWork.Complete();

            // Assert
            using var unitOfWork2 = _unitOfWorkFactory.GenerateUnitOfWork();

            var personVariants =
                await unitOfWork2.People.GetAllVariants(new Guid("00000004-0000-0000-0000-000000000000"));

            personVariants.Count().Should().Be(2);
            personVariants.Count(p => p.FirstName == "Anakin Skywalker").Should().Be(1);
            personVariants.Count(p => p.Start.Year == 1992).Should().Be(1);
            personVariants.Count(p => p.End.Year == 1997).Should().Be(1);
            personVariants.Count(p => p.FirstName == "Darth Vader").Should().Be(1);
        }

        [Fact]
        public async Task RetroactivelyDeleteAnEarlierStateOfAPerson()
        {
            // Arrange
            _unitOfWorkFactory.HistoricalTime = new DateTime(2002, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            using var unitOfWork = _unitOfWorkFactory.GenerateUnitOfWork();

            var person = await unitOfWork.People.Get(
                new Guid("00000004-0000-0000-0000-000000000000"));

            // Act
            await unitOfWork.People.Erase(person);
            unitOfWork.Complete();

            // Assert
            using var unitOfWork2 = _unitOfWorkFactory.GenerateUnitOfWork();

            var personVariants =
                await unitOfWork2.People.GetAllVariants(new Guid("00000004-0000-0000-0000-000000000000"));

            personVariants.Count().Should().Be(1);
            personVariants.Count(p => p.FirstName == "Darth Vader").Should().Be(1);
        }

        [Fact]
        public async Task UpdatePeopleProspectively_UsingCurrentTimeAsTimeOfChange()
        {
            // Arrange
            using var unitOfWork1 = _unitOfWorkFactory.GenerateUnitOfWork();

            var ids = new List<Guid>
            {
                new("00000006-0000-0000-0000-000000000000")
            };

            // Act
            var people1 = (await unitOfWork1.People.Find(p => ids.Contains(p.ID))).ToList();

            people1.ForEach(_ => _.FirstName = "Garfield");
            await unitOfWork1.People.UpdateRange(people1);
            unitOfWork1.Complete();

            // Assert
            // Now, the name of the person is Garfield
            using var unitOfWork2 = _unitOfWorkFactory.GenerateUnitOfWork();
            var people2 = (await unitOfWork2.People.Find(p => ids.Contains(p.ID))).ToList();
            people2.Count.Should().Be(1);
            people2.Count(p => p.FirstName == "Garfield").Should().Be(1);

            // A moment ago, the name of the person was Rey Skywalker
            _unitOfWorkFactory.HistoricalTime = DateTime.UtcNow - TimeSpan.FromSeconds(20);
            using var unitOfWork3 = _unitOfWorkFactory.GenerateUnitOfWork();
            var people3 = (await unitOfWork3.People.Find(p => ids.Contains(p.ID))).ToList();
            people3.Count.Should().Be(1);
            people3.Count(p => p.FirstName == "Rey Skywalker").Should().Be(1);
        }

        [Fact]
        public async Task UpdatePeopleProspectively_UsingEarlierTimeAsTimeOfChange()
        {
            // Arrange
            using var unitOfWork1 = _unitOfWorkFactory.GenerateUnitOfWork();

            var ids = new List<Guid>
            {
                new("00000006-0000-0000-0000-000000000000")
            };

            // Act
            var people1 = (await unitOfWork1.People.Find(p => ids.Contains(p.ID))).ToList();

            people1.ForEach(_ => _.FirstName = "Garfield");
            (unitOfWork1 as UnitOfWorkFacade)!.TimeOfChange = new DateTime(2017, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            await unitOfWork1.People.UpdateRange(people1);
            unitOfWork1.Complete();

            // Assert
            // In 2018, the name of the person was Garfield
            _unitOfWorkFactory.HistoricalTime = new DateTime(2018, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            using var unitOfWork2 = _unitOfWorkFactory.GenerateUnitOfWork();
            var people2 = (await unitOfWork2.People.Find(p => ids.Contains(p.ID))).ToList();
            people2.Count.Should().Be(1);
            people2.Count(p => p.FirstName == "Garfield").Should().Be(1);

            // In 2016, the name of the person was Rey Skywalker
            _unitOfWorkFactory.HistoricalTime = new DateTime(2016, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            using var unitOfWork3 = _unitOfWorkFactory.GenerateUnitOfWork();
            var people3 = (await unitOfWork3.People.Find(p => ids.Contains(p.ID))).ToList();
            people3.Count.Should().Be(1);
            people3.Count(p => p.FirstName == "Rey Skywalker").Should().Be(1);
        }

        [Fact]
        public async Task UpdatePeopleProspectively_UsingTooEarlyTimeAsTimeOfChange_Throws()
        {
            // Arrange
            using var unitOfWork = _unitOfWorkFactory.GenerateUnitOfWork();

            var ids = new List<Guid>
            {
                new("00000006-0000-0000-0000-000000000000")
            };

            // Act
            var exception = await Record.ExceptionAsync(async () =>
            {
                var people = (await unitOfWork.People.Find(p => ids.Contains(p.ID))).ToList();
                people.ForEach(_ => _.FirstName = "Garfield");
                // Here, we try to change Rey Skywalker using a time of change that lies before the time she changed from Rey to Rey Skywalker, which is not allowed
                (unitOfWork as UnitOfWorkFacade)!.TimeOfChange = new DateTime(2008, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                await unitOfWork.People.UpdateRange(people);
                unitOfWork.Complete();
            });

            // Assert
            Assert.NotNull(exception);
        }

        [Fact]
        public async Task UpdatePeopleProspectively_UsingFutureTimeAsTimeOfChange_Throws()
        {
            // Arrange
            using var unitOfWork = _unitOfWorkFactory.GenerateUnitOfWork();

            var ids = new List<Guid>
            {
                new("00000006-0000-0000-0000-000000000000")
            };

            // Act
            var exception = await Record.ExceptionAsync(async () =>
            {
                var people = (await unitOfWork.People.Find(p => ids.Contains(p.ID))).ToList();
                people.ForEach(_ => _.FirstName = "Garfield");
                // Here, we try to change Rey Skywalker using a time of change that lies in the future, which is not allowed
                (unitOfWork as UnitOfWorkFacade)!.TimeOfChange = DateTime.UtcNow + TimeSpan.FromHours(1);
                await unitOfWork.People.UpdateRange(people);
                unitOfWork.Complete();
            });

            // Assert
            Assert.NotNull(exception);
        }


        // Like when registering that John Doe lived a different place in a given time period
        //[Fact]
        //public async Task SqueezeInANewStateOfAPerson()
        //{
        //    // Arrange
        //    // Act
        //    // Assert
        //    throw new NotImplementedException();
        //}
    }
}

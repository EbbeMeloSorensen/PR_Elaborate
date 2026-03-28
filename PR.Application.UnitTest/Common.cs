using FluentAssertions;
using PR.Domain.Entities.PR;
using PR.Persistence;
using Xunit;

namespace PR.Application.UnitTest
{
    public static class Common
    {
        public static async Task CreatePerson(
            IUnitOfWorkFactory unitOfWorkFactory)
        {
            // Arrange
            var person = new Person
            {
                FirstName = "Han Solo"
            };

            // Act
            using var unitOfWork1 = unitOfWorkFactory.GenerateUnitOfWork();
            await unitOfWork1.People.Add(person);
            unitOfWork1.Complete();

            // Assert
            using var unitOfWork2 = unitOfWorkFactory.GenerateUnitOfWork();
            var people = await unitOfWork2.People.GetAll();
            people.Count().Should().Be(5);
            people.Count(p => p.FirstName == "Max Rebo").Should().Be(1);
            people.Count(p => p.FirstName == "Chewbacca").Should().Be(1);
            people.Count(p => p.FirstName == "Rey Skywalker").Should().Be(1);
            people.Count(p => p.FirstName == "Wicket").Should().Be(1);
            people.Count(p => p.FirstName == "Han Solo").Should().Be(1);
        }

        // Notice that this does not involve the business rule catalog, that acts in the application layer
        public static async Task CreatePersonWithoutMandatoryPropertyThrows(
            IUnitOfWorkFactory unitOfWorkFactory)
        {
            // Arrange
            var person = new Person();
            using var unitOfWork1 = unitOfWorkFactory.GenerateUnitOfWork();

            // Act & Assert
            var exception = await Record.ExceptionAsync(async () =>
            {
                await unitOfWork1.People.Add(person);
                unitOfWork1.Complete();
            });

            Assert.NotNull(exception);
        }

        public static async Task GetAllPeople(
            IUnitOfWorkFactory unitOfWorkFactory)
        {
            // Arrange
            using var unitOfWork = unitOfWorkFactory.GenerateUnitOfWork();

            // Act
            var people = await unitOfWork.People.GetAll();

            // Assert
            people.Count().Should().Be(4);
            people.Count(p => p.FirstName == "Max Rebo").Should().Be(1);
            people.Count(p => p.FirstName == "Rey Skywalker").Should().Be(1);
            people.Count(p => p.FirstName == "Chewbacca").Should().Be(1);
            people.Count(p => p.FirstName == "Wicket").Should().Be(1);
        }

        public static async Task GetPersonById(
            IUnitOfWorkFactory unitOfWorkFactory)
        {
            // Arrange
            using var unitOfWork = unitOfWorkFactory.GenerateUnitOfWork();
            var id = new Guid("00000006-0000-0000-0000-000000000000");

            // Act
            var person = await unitOfWork.People.Get(id);

            // Assert
            person.FirstName.Should().Be("Rey Skywalker");
        }

        public static async Task GetPersonIncludingCommentsById(
            IUnitOfWorkFactory unitOfWorkFactory)
        {
            // Arrange
            using var unitOfWork = unitOfWorkFactory.GenerateUnitOfWork();
            var id = new Guid("00000006-0000-0000-0000-000000000000");

            // Act
            var person = await unitOfWork.People.GetIncludingComments(id);

            // Assert
            person.FirstName.Should().Be("Rey Skywalker");
            person.Comments.Count().Should().Be(1);
            person.Comments.Single().Text.Should().Be("She is a jedi");
        }

        public static async Task FindPeopleIncludingComments(
            IUnitOfWorkFactory unitOfWorkFactory)
        {
            // Arrange
            using var unitOfWork = unitOfWorkFactory.GenerateUnitOfWork();

            var ids = new List<Guid>
            {
                new("00000001-0000-0000-0000-000000000000"),
                new("00000005-0000-0000-0000-000000000000"),
                new("00000006-0000-0000-0000-000000000000"),
            };

            // Act
            var people = (await unitOfWork.People.FindIncludingComments(p => ids.Contains(p.ID)));

            // Assert
            people.Count().Should().Be(3);
            people.Count(p => p.FirstName == "Max Rebo").Should().Be(1);
            people.Count(p => p.FirstName == "Chewbacca").Should().Be(1);
            people.Count(p => p.FirstName == "Rey Skywalker").Should().Be(1);
            people.Single(p => p.FirstName == "Chewbacca").Comments.Count.Should().Be(2);
            people.Single(p => p.FirstName == "Chewbacca").Comments.Count(_ => _.Text == "He likes his crossbow").Should().Be(1);
            people.Single(p => p.FirstName == "Chewbacca").Comments.Count(_ => _.Text == "He is a furry fellow").Should().Be(1);
            people.Single(p => p.FirstName == "Rey Skywalker").Comments.Count.Should().Be(1);
            people.Single(p => p.FirstName == "Rey Skywalker").Comments.Single().Text.Should().Be("She is a jedi");
        }

        public static async Task FindPersonById(
            IUnitOfWorkFactory unitOfWorkFactory)
        {
            // Arrange
            using var unitOfWork = unitOfWorkFactory.GenerateUnitOfWork();

            var ids = new List<Guid>
            {
                new("00000006-0000-0000-0000-000000000000")
            };

            // Act
            var people = await unitOfWork.People.Find(p => ids.Contains(p.ID));

            // Assert
            people.Count().Should().Be(1);
            people.Single().FirstName.Should().Be("Rey Skywalker");
        }

        public static async Task FindPersonById_PersonWasDeleted(
            IUnitOfWorkFactory unitOfWorkFactory)
        {
            // Arrange
            using var unitOfWork = unitOfWorkFactory.GenerateUnitOfWork();

            var ids = new List<Guid>
            {
                new("00000004-0000-0000-0000-000000000000")
            };

            // Act
            var people = await unitOfWork.People.Find(p => ids.Contains(p.ID));

            // Assert
            people.Count().Should().Be(0);
        }

        public static async Task FindPeopleById(
            IUnitOfWorkFactory unitOfWorkFactory)
        {
            // Arrange
            using var unitOfWork1 = unitOfWorkFactory.GenerateUnitOfWork();

            var ids = new List<Guid>
            {
                new("00000001-0000-0000-0000-000000000000"),
                new("00000006-0000-0000-0000-000000000000"),
            };

            // Act
            var people = (await unitOfWork1.People.Find(p => ids.Contains(p.ID))).ToList();

            people.Count.Should().Be(2);
            people.Count(p => p.FirstName == "Max Rebo").Should().Be(1);
            people.Count(p => p.FirstName == "Rey Skywalker").Should().Be(1);
        }

        public static async Task UpdatePerson(
            IUnitOfWorkFactory unitOfWorkFactory)
        {
            // Arrange
            using var unitOfWork1 = unitOfWorkFactory.GenerateUnitOfWork();
            var id = new Guid("00000006-0000-0000-0000-000000000000");
            var person1 = await unitOfWork1.People.Get(id);

            person1.FirstName = "Riley";

            // Act
            await unitOfWork1.People.Update(person1);
            unitOfWork1.Complete();

            // Assert
            using var unitOfWork2 = unitOfWorkFactory.GenerateUnitOfWork();
            var person2 = await unitOfWork2.People.Get(id);
            person2.FirstName.Should().Be("Riley");
        }

        public static async Task UpdatePeople(
            IUnitOfWorkFactory unitOfWorkFactory)
        {
            // Arrange
            using var unitOfWork1 = unitOfWorkFactory.GenerateUnitOfWork();

            var ids = new List<Guid>
            {
                new("00000006-0000-0000-0000-000000000000")
            };

            // Act
            var people1 = (await unitOfWork1.People.Find(p => ids.Contains(p.ID))).ToList();

            people1.ForEach(_ => _.FirstName = "Rudy");
            await unitOfWork1.People.UpdateRange(people1);
            unitOfWork1.Complete();

            // Assert
            using var unitOfWork2 = unitOfWorkFactory.GenerateUnitOfWork();
            var people2 = (await unitOfWork2.People.Find(p => ids.Contains(p.ID))).ToList();
            people2.Count.Should().Be(1);
            people2.Count(p => p.FirstName == "Rudy").Should().Be(1);
        }

        public static async Task DeletePerson_WithoutAnyChildObjects(
            IUnitOfWorkFactory unitOfWorkFactory)
        {
            // Arrange
            using var unitOfWork1 = unitOfWorkFactory.GenerateUnitOfWork();
            var id = new Guid("00000001-0000-0000-0000-000000000000");
            var person = await unitOfWork1.People.Get(id);

            // Act
            await unitOfWork1.People.Remove(person);
            unitOfWork1.Complete();

            // Assert
            using var unitOfWork2 = unitOfWorkFactory.GenerateUnitOfWork();
            var people = await unitOfWork2.People.GetAll();
            people.Count().Should().Be(3);
            people.Count(p => p.FirstName == "Chewbacca").Should().Be(1);
            people.Count(p => p.FirstName == "Rey Skywalker").Should().Be(1);
            people.Count(p => p.FirstName == "Wicket").Should().Be(1);
        }

        public static async Task DeletePerson_WithChildObjects_Throws(
            IUnitOfWorkFactory unitOfWorkFactory)
        {
            // Arrange
            using var unitOfWork1 = unitOfWorkFactory.GenerateUnitOfWork();
            var id = new Guid("00000005-0000-0000-0000-000000000000");
            var person = await unitOfWork1.People.Get(id);

            // Act & Assert
            var exception = await Record.ExceptionAsync(async () =>
            {
                await unitOfWork1.People.Remove(person);
                unitOfWork1.Complete();
            });

            Assert.NotNull(exception);
        }

        public static async Task DeletePeople_WithoutAnyChildObjects(
            IUnitOfWorkFactory unitOfWorkFactory)
        {
            // Arrange
            using var unitOfWork1 = unitOfWorkFactory.GenerateUnitOfWork();

            var ids = new List<Guid>
                {
                    new("00000001-0000-0000-0000-000000000000"),
                    new("00000002-0000-0000-0000-000000000000")
                };

            // Act
            var people = (await unitOfWork1.People.Find(p => ids.Contains(p.ID))).ToList();

            await unitOfWork1.People.RemoveRange(people);
            unitOfWork1.Complete();

            // Assert
            using var unitOfWork2 = unitOfWorkFactory.GenerateUnitOfWork();
            people = (await unitOfWork2.People.GetAll()).ToList();
            people.Count.Should().Be(3);
            people.Count(p => p.FirstName == "Chewbacca").Should().Be(1);
            people.Count(p => p.FirstName == "Rey Skywalker").Should().Be(1);
            people.Count(p => p.FirstName == "Wicket").Should().Be(1);
        }

        public static async Task DeletePeople_WithChildObjects_Throws(
            IUnitOfWorkFactory unitOfWorkFactory)
        {
            // Arrange
            using var unitOfWork1 = unitOfWorkFactory.GenerateUnitOfWork();

            var ids = new List<Guid>
            {
                new("00000001-0000-0000-0000-000000000000"),
                new("00000005-0000-0000-0000-000000000000")
            };

            var people = (await unitOfWork1.People.Find(p => ids.Contains(p.ID))).ToList();

            // Act & Assert
            var exception = await Record.ExceptionAsync(async () =>
            {
                await unitOfWork1.People.RemoveRange(people);
                unitOfWork1.Complete();
            });

            Assert.NotNull(exception);
        }

        public static async Task CreatePersonComment(
            IUnitOfWorkFactory unitOfWorkFactory)
        {
            // Arrange
            var personComment = new PersonComment
            {
                PersonID = new Guid("00000006-0000-0000-0000-000000000000"),
                Text = "She is suspiciously adept with the force"
            };

            // Act
            using var unitOfWork1 = unitOfWorkFactory.GenerateUnitOfWork();
            await unitOfWork1.PersonComments.Add(personComment);
            unitOfWork1.Complete();

            // Assert
            using var unitOfWork2 = unitOfWorkFactory.GenerateUnitOfWork();
            var person = await unitOfWork2.People.GetIncludingComments(new Guid("00000006-0000-0000-0000-000000000000"));

            person.Comments.Count().Should().Be(2);
            person.Comments.Where(_ => _.Text == "She is suspiciously adept with the force").Count().Should().Be(1);
        }

        public static async Task UpdatePersonComment(
            IUnitOfWorkFactory unitOfWorkFactory)
        {
            // Arrange
            using var unitOfWork1 = unitOfWorkFactory.GenerateUnitOfWork();
            var id = new Guid("00000003-0000-0000-0000-000000000000");
            var personComment = await unitOfWork1.PersonComments.Get(id);
            personComment.Text = "He is a wookie";

            // Act
            await unitOfWork1.PersonComments.Update(personComment);
            unitOfWork1.Complete();

            // Assert
            using var unitOfWork2 = unitOfWorkFactory.GenerateUnitOfWork();
            var person = await unitOfWork2.People.GetIncludingComments(new Guid("00000005-0000-0000-0000-000000000000"));

            person.FirstName.Should().Be("Chewbacca");
            person.Comments.Count.Should().Be(2);
            person.Comments.Where(_ => _.Text == "He is a wookie").Count().Should().Be(1);
        }

        public static async Task DeletePersonComment(
            IUnitOfWorkFactory unitOfWorkFactory)
        {
            // Arrange
            using var unitOfWork1 = unitOfWorkFactory.GenerateUnitOfWork();
            var id = new Guid("00000002-0000-0000-0000-000000000000");
            var personComment = await unitOfWork1.PersonComments.Get(id);

            // Act
            await unitOfWork1.PersonComments.Remove(personComment);
            unitOfWork1.Complete();

            // Assert
            using var unitOfWork2 = unitOfWorkFactory.GenerateUnitOfWork();
            var person = await unitOfWork2.People.GetIncludingComments(new Guid("00000005-0000-0000-0000-000000000000"));

            person.FirstName.Should().Be("Chewbacca");
            person.Comments.Count.Should().Be(1);
            person.Comments.Single().Text.Should().Be("He is a furry fellow");
        }

        public static async Task DeletePersonComments(
            IUnitOfWorkFactory unitOfWorkFactory)
        {
            // Arrange
            using var unitOfWork1 = unitOfWorkFactory.GenerateUnitOfWork();
            var ids = new List<Guid>
            {
                new Guid("00000001-0000-0000-0000-000000000000"),
                new Guid("00000002-0000-0000-0000-000000000000")
            };

            var personComments = await unitOfWork1.PersonComments.Find(_ => ids.Contains(_.ID));

            // Act
            await unitOfWork1.PersonComments.RemoveRange(personComments);
            unitOfWork1.Complete();

            // Assert
            using var unitOfWork2 = unitOfWorkFactory.GenerateUnitOfWork();
            var personComments2 = await unitOfWork2.PersonComments.GetAll();

            personComments2.Count().Should().Be(1);
            personComments2.Single().Text.Should().Be("He is a furry fellow");
        }
    }
}

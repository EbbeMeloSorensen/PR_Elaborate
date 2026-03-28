using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;
using Person = PR.Domain.Entities.PR.Person;

namespace PR.IO.UnitTest
{
    public class DataIOHandlerTest
    {
        [Fact]
        public void ExportDataToXML_Works()
        {
            new DataIOHandler()
                .ExportDataToXML(GenerateDataSet(), "Temp.xml");
        }

        [Fact]
        public void ImportDataFromXML_Works()
        {
            new DataIOHandler().ImportDataFromXML(@"Data/People.xml", out PRData prData);

            prData.People.Count.Should().Be(3);
            prData.People.Count(p => p.FirstName == "Ebbe").Should().Be(1);
            prData.People.Count(p => p.FirstName == "Uffe").Should().Be(1);
        }

        [Fact]
        public void ExportDataToJson_Works()
        {
            new DataIOHandler().ExportDataToJson(GenerateDataSet(), "Temp.json");
        }

        [Fact]
        public void ImportDataFromJson_Works()
        {
            var dataIOHandler = new DataIOHandler();
            dataIOHandler.ImportDataFromJson(@"Data/People.json", out var prData);

            prData.People.Count.Should().Be(3);
            prData.People.Count(p => p.FirstName == "Ebbe").Should().Be(1);
            prData.People.Count(p => p.FirstName == "Uffe").Should().Be(1);
        }

        // Helper
        private PRData GenerateDataSet()
        {
            var now = DateTime.UtcNow;

            var ebbe = new Person
            {
                ID = Guid.NewGuid(),
                FirstName = "Ebbe",
                Surname = "Melo Sørensen",
                Created = new DateTime(2022, 1, 1, 3, 3, 6).ToUniversalTime()
            };

            var ana = new Person
            {
                ID = Guid.NewGuid(),
                FirstName = "Ana Tayze",
                Surname = "Melo Sørensen",
                Created = now
            };

            var uffe = new Person
            {
                ID = Guid.NewGuid(),
                FirstName = "Uffe",
                Surname = "Sørensen",
                Created = now
            };

            return new PRData
            {
                People = new List<Person>
                {
                    ebbe,
                    ana,
                    uffe
                }
            };
        }
    }
}
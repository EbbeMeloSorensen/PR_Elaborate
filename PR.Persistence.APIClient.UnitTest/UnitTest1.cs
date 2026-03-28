using System.Text;
using System.Text.Json;
using Newtonsoft.Json;
using Xunit;
using FluentAssertions;
using PR.Persistence.APIClient.DFOS.ModelClasses;

namespace PR.Persistence.APIClient.UnitTest
{
    public class UnitTest1
    {
        [Fact]
        public async void CallSimpleWebAPIAndObtainResultWithoutKnowingStructure()
        {
            var url = "https://api.sunrise-sunset.org/json?lat=55.661954&lng=12.49001&date=today"; // Sun up/down for Dansh�jvej 33

            using (var response = await ApiHelper.ApiClient.GetAsync(url))
            {
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();

                // Parse the JSON using JsonDocument (Suitable when you don't know the structure)
                using (var doc = JsonDocument.Parse(responseBody))
                {
                    var root = doc.RootElement;

                    // Navigate through JSON dynamically
                    var results = root.GetProperty("results");
                    var sunrise = results.GetProperty("sunrise");
                    var sunset = results.GetProperty("sunset");
                }
            }
        }

        [Fact]
        public async void CallSimpleWebAPIAndDeserializeResult()
        {
            var url = "https://api.sunrise-sunset.org/json?lat=55.661954&lng=12.49001&date=today"; // Sun up/down for Dansh�jvej 33

            using var response = await ApiHelper.ApiClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();

            // When you know the structure of the json data
            var data = JsonConvert.DeserializeObject<SunResultModel>(responseBody);
            data.Results.Sunrise.Date.Should().Be(DateTime.Today);
            data.Results.Sunset.Date.Should().Be(DateTime.Today);
        }

        [Fact]
        public async void DeserializeDFOSJsonData()
        {
            var fileName = @".\Data\mock_response.json";
            var responseBody = File.ReadAllText(fileName, Encoding.UTF8);

            var data = JsonConvert.DeserializeObject<DFOSResultModel>(responseBody);
            data.Should().NotBeNull();
            data.Type.Should().Be("FeatureCollection");
            data.Features.Count().Should().Be(13);
            var details = data.Features.First().Properties.Details;
            details.Count().Should().Be(1);
            var key = "[2024-10-11T11:30:00Z,)";
            details.ContainsKey(key).Should().BeTrue();
            var observingFacility = details[key];
            observingFacility.FacilityName.Should().Be("Andeby Havn - PI1 Demo (Test)");
        }

        [Fact]
        public async void CallDFOSAPIAndObtainResultWithoutKnowingStructure()
        {
            var url = "http://dfos-api-prod.dmi.dk/collections/observing_facility/items";

            using (var response = await ApiHelper.ApiClient.GetAsync(url))
            {
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();

                // Parse the JSON using JsonDocument (Suitable when you don't know the structure)
                using (var doc = JsonDocument.Parse(responseBody))
                {
                    var root = doc.RootElement;

                    // Navigate through JSON dynamically
                    var type = root.GetProperty("type");
                    var features = root.GetProperty("features");
                }
            }
        }

        [Fact]
        public async void DFOSAPI_RetrieveAllObservingFacilities()
        {
            // Arrange
            var observingFacilities = new List<ObservingFacility>();

            // Act
            var url = "http://dfos-api-prod.dmi.dk/collections/observing_facility/items";
            //var url = "http://dfos-api-dev.dmi.dk/collections/observing_facility/items?limit=100&datetime=../..";

            using var response = await ApiHelper.ApiClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();

            var data = JsonConvert.DeserializeObject<DFOSResultModel>(responseBody);
            data.Should().NotBeNull();
            data.Type.Should().Be("FeatureCollection");

            // The "properties" element of a feature has a "details" element, where the only property
            // has a property name that is a time interval. We cannot deserialize this, so instead
            // we read the response into a JsonDocument that we then navigate through
            using (var doc = JsonDocument.Parse(responseBody))
            {
                var root = doc.RootElement;

                // Navigate through JSON dynamically
                var type = root.GetProperty("type");
                var features = root.GetProperty("features");

                if (features.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement element in features.EnumerateArray())
                    {
                        var temp1 = element.GetProperty("properties");
                        var temp2 = temp1.GetProperty("details");

                        foreach (var property in temp2.EnumerateObject())
                        {
                            var propertyName = property.Name;
                            var propertyValue = property.Value;
                            var DetailsAsRawText = propertyValue.GetRawText();
                            var observingFacility = JsonConvert.DeserializeObject<ObservingFacility>(DetailsAsRawText);
                            observingFacilities.Add(observingFacility);
                        }
                    }
                }
            }

            // Assert
            var facilityNamesReceived = observingFacilities
                .Select(o => o.FacilityName)
                .OrderBy(n => n);

            var expectedFacilityNames = new List<string>
            {
                "Andeby Havn Opdateret (Test)",
                "Karup Vest (Test)",
                "Nørre Lyngby N",
                "Uggerby"
            };

            Enumerable.SequenceEqual(
                facilityNamesReceived,
                expectedFacilityNames).Should().BeTrue();
        }

        [Fact]
        public async void GetAllPeople()
        {
            var unitOfWorkFactory = new UnitOfWorkFactory();

            using (var unitOfWork = unitOfWorkFactory.GenerateUnitOfWork())
            {
                var people = await unitOfWork.People.GetAll();
                people.Count().Should().BeGreaterThan(0);
            }
        }
    }
}
using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Craft.Logging;
using Newtonsoft.Json;
using Craft.Utils;
using PR.Domain.Entities.PR;
using PR.Persistence.Repositories.PR;

namespace PR.Persistence.APIClient.Repositories
{
    public class PersonRepository : IPersonRepository
    {
        private string _baseURL;
        private string _token;
        private DateTime? _historicalTime;
        private bool _includeHistoricalObjects;
        private DateTime? _databaseTime;

        public PersonRepository(
            string baseURL,
            DateTime? historicalTime,
            bool includeHistoricalObjects,
            DateTime? databaseTime)
        {
            _baseURL = baseURL;
            _historicalTime = historicalTime;
            _includeHistoricalObjects = includeHistoricalObjects;
            _databaseTime = databaseTime;
        }

        public int CountAll()
        {
            throw new NotImplementedException();
        }

        public int Count(
            Expression<Func<Person, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public int Count(
            IList<Expression<Func<Person, bool>>> predicates)
        {
            throw new NotImplementedException();
        }

        public ILogger Logger { get; }

        public async Task<Person> Get(
            Guid id)
        {
            await Login();

            // We call the API using the token - here we want all people (and we are not using pagination here)
            var url = $"http://localhost:5000/api/people/{id}";

            if (_databaseTime.HasValue)
            {
                //url = "http://localhost:5000/api/people?DatabaseTime=2002-01-01T00:00:00Z";
                url += $"?DatabaseTime={_databaseTime.Value.AsRFC3339(false)}";
            }

            ApiHelper.ApiClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _token);

            using var response = await ApiHelper.ApiClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            var person = JsonConvert.DeserializeObject<Person>(responseBody);
            return person;
        }

        public Task<IEnumerable<Person>> FindIncludingComments(IList<Expression<Func<Person, bool>>> predicates)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Person>> GetAllVariants(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task Correct(
            Person person)
        {
            throw new NotImplementedException();
        }

        public Task CorrectRange(
            IEnumerable<Person> people)
        {
            throw new NotImplementedException();
        }

        public Task Erase(
            Person person)
        {
            throw new NotImplementedException();
        }

        public Task EraseRange(
            IEnumerable<Person> people)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<DateTime>> GetAllValidTimeIntervalExtrema()
        {
            return await Task.Run(() =>
            {
                // Just return an empty list for now. Later add it to the API
                return new List<DateTime>();
            });
        }

        public async Task<IEnumerable<DateTime>> GetAllDatabaseWriteTimes()
        {
            return await Task.Run(() =>
            {
                // Just return an empty list for now. Later add it to the API
                return new List<DateTime>();
            });
        }

        public Task<Person> GetIncludingComments(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Person>> FindIncludingComments(Expression<Func<Person, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Person>> GetAll()
        {
            await Login();

            // We call the API using the token - here we want all people (and we are not using pagination here)
            var urlBuilder = new UriBuilder(); // Spændende - mon ikke man kan noget smart med den?
            var url = $"{_baseURL}/people";

            var arguments = new List<string>();

            if (_historicalTime.HasValue)
            {
                arguments.Add($"historicaltime={_historicalTime.Value.AsRFC3339(false)}");
            }

            if (_includeHistoricalObjects)
            {
                arguments.Add($"includehistoricalobjects=true");
            }

            if (_databaseTime.HasValue)
            {
                arguments.Add($"databasetime={_databaseTime.Value.AsRFC3339(false)}");
            }

            if (arguments.Any())
            {
                url += "?";
                url += arguments.Aggregate((c, n) => $"{c}&{n}");
            }

            ApiHelper.ApiClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _token);

            using var response = await ApiHelper.ApiClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();

            var people = JsonConvert.DeserializeObject<List<Person>>(responseBody);

            // When you know the structure of the json data
            return people;
        }

        public async Task<IEnumerable<Person>> Find(
            Expression<Func<Person, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Person>> Find(
            IList<Expression<Func<Person, bool>>> predicates)
        {
            await Login();

            var url = "http://localhost:5000/api/people";

            var arguments = new List<string>();

            if (_historicalTime.HasValue)
            {
                arguments.Add($"HistoricalTime={_historicalTime.Value.AsRFC3339(false)}");
            }

            if (_databaseTime.HasValue)
            {
                arguments.Add($"DatabaseTime={_databaseTime.Value.AsRFC3339(false)}");
            }

            if (arguments.Any())
            {
                url += "?";
                url += arguments.Aggregate((c, n) => $"{c}&{n}");
            }

            ApiHelper.ApiClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _token);

            using var response = await ApiHelper.ApiClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();

            var people = JsonConvert.DeserializeObject<List<Person>>(responseBody);

            // When you know the structure of the json data
            return people;
        }

        public Person SingleOrDefault(
            Expression<Func<Person, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public async Task Add(
            Person person)
        {
            await Login();

            var url = "http://localhost:5000/api/people";

            ApiHelper.ApiClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _token);

            var body = $"{{\"id\":\"{Guid.NewGuid()}\",\"firstName\":\"{person.FirstName}\"}}";
            var content = new StringContent(body, Encoding.UTF8, "application/json");

            using (var response = await ApiHelper.ApiClient.PostAsync(url, content))
            {
                response.EnsureSuccessStatusCode();
            }
        }

        public async Task AddRange(
            IEnumerable<Person> people)
        {
            throw new NotImplementedException();
        }

        public async Task Update(
            Person person)
        {
            await Login();

            await Task.Run(async () =>
            {
                var url = $"http://localhost:5000/api/people/{person.ID}";

                ApiHelper.ApiClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _token);

                var body = $"{{\"firstName\":\"{person.FirstName}\"}}";
                var content = new StringContent(body, Encoding.UTF8, "application/json");

                using var response = await ApiHelper.ApiClient.PutAsync(url, content);
                response.EnsureSuccessStatusCode();
            });
        }

        public async Task UpdateRange(
            IEnumerable<Person> people)
        {
            throw new NotImplementedException();
        }

        public async Task Remove(
            Person person)
        {
            await Login();

            await Task.Run(async () =>
            {
                var url = $"http://localhost:5000/api/people/{person.ID}";

                ApiHelper.ApiClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _token);

                using var response = await ApiHelper.ApiClient.DeleteAsync(url);
                response.EnsureSuccessStatusCode();
            });
        }

        public async Task RemoveRange(
            IEnumerable<Person> people)
        {
            throw new NotImplementedException();
        }

        public async Task Clear()
        {
            throw new NotImplementedException();
        }

        public void Load(
            IEnumerable<Person> people)
        {
            throw new NotImplementedException();
        }

        private async Task Login()
        {
            var url = "http://localhost:5000/api/account/login";

            var content = new StringContent("{\"email\":\"bob@test.com\",\"password\":\"Super-long-very-secure-secret-key-that-is-at-least-64-bytes-in-length!!!!\"}", Encoding.UTF8,
                "application/json");

            using (var response = await ApiHelper.ApiClient.PostAsync(url, content))
            {
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();

                // When you know the structure of the json data
                var result = JsonConvert.DeserializeObject<LoginResult>(responseBody);
                _token = result.token;
            }
        }
    }
}

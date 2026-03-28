using Craft.Domain;
using PR.Domain.Entities.PR;

namespace PR.Domain.BusinessRules.PR.AtomicRules
{
    public class CityIsValidRule : IBusinessRule<Person>
    {
        public string RuleName => "City";

        public string ErrorMessage { get; private set; } = "";

        public bool Validate(
            Person person)
        {
            if (!string.IsNullOrEmpty(person.City) && person.City.Length > 20)
            {
                ErrorMessage = "City too long (max 20 characters)";
                return false;
            }

            return true;
        }
    }
}
using Craft.Domain;
using PR.Domain.Entities.PR;

namespace PR.Domain.BusinessRules.PR.AtomicRules
{
    public class AddressIsValidRule : IBusinessRule<Person>
    {
        public string RuleName => "Address";

        public string ErrorMessage { get; private set; } = "";

        public bool Validate(
            Person person)
        {
            if (!string.IsNullOrEmpty(person.Address) && person.Address.Length > 20)
            {
                ErrorMessage = "Address too long (max 20 characters)";
                return false;
            }

            return true;
        }
    }
}
using Craft.Domain;
using PR.Domain.Entities.PR;

namespace PR.Domain.BusinessRules.PR.AtomicRules
{
    public class SurnameIsValidRule : IBusinessRule<Person>
    {
        public string RuleName => "Surname";

        public string ErrorMessage { get; private set; } = "";

        public bool Validate(
            Person person)
        {
            if (!string.IsNullOrEmpty(person.Surname) && person.Surname.Length > 20)
            {
                ErrorMessage = "Surname too long (max 20 characters)";
                return false;
            }

            return true;
        }
    }
}
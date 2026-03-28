using System;
using Craft.Domain;
using PR.Domain.Entities.PR;

namespace PR.Domain.BusinessRules.PR.AtomicRules
{
    public class BirthdayIsValidRule : IBusinessRule<Person>
    {
        public string RuleName => "Birthday";

        public string ErrorMessage { get; private set; } = "";

        public bool Validate(
            Person person)
        {
            if (person.Birthday.HasValue && person.Birthday.Value > DateTime.UtcNow)
            {
                ErrorMessage = "Birthday cannot be in the future";
                return false;
            }

            return true;
        }
    }
}
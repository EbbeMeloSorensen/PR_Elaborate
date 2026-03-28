using System;
using System.Collections.Generic;
using PR.Domain.Entities.PR;

namespace PR.Application
{
    public class PeopleEventArgs : EventArgs
    {
        public readonly IEnumerable<Person> People;

        public PeopleEventArgs(
            IEnumerable<Person> people)
        {
            People = people;
        }
    }
}
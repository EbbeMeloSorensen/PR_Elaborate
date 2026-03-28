using System;

namespace PR.Persistence
{
    public interface IUnitOfWorkFactoryVersioned : IUnitOfWorkFactory
    {
        DateTime? DatabaseTime { get; set; }
    }
}

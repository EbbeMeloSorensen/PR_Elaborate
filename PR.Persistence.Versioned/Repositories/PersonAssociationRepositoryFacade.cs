using System.Linq.Expressions;
using Craft.Logging;
using PR.Domain.Entities.PR;
using PR.Persistence.Repositories.PR;

namespace PR.Persistence.Versioned.Repositories;

public class PersonAssociationRepositoryFacade : IPersonAssociationRepository
{
    private static DateTime _maxDate;

    private UnitOfWorkFacade _unitOfWorkFacade;
    private bool _returnClonesInsteadOfRepositoryObjects = true;

    private IUnitOfWork UnitOfWork => _unitOfWorkFacade.UnitOfWork;
    private DateTime? DatabaseTime => _unitOfWorkFacade.DatabaseTime;
    private DateTime? HistoricalTime => _unitOfWorkFacade.HistoricalTime;
    private bool IncludeCurrentObjects => _unitOfWorkFacade.IncludeCurrentObjects;
    private bool IncludeHistoricalObjects => _unitOfWorkFacade.IncludeHistoricalObjects;
    private DateTime CurrentTime => _unitOfWorkFacade.TransactionTime;
    private DateTime TimeOfChange => _unitOfWorkFacade.TimeOfChange ?? CurrentTime;

    public ILogger Logger { get; }

    static PersonAssociationRepositoryFacade()
    {
        _maxDate = new DateTime(9999, 12, 31, 23, 59, 59, DateTimeKind.Utc);
    }

    public PersonAssociationRepositoryFacade(
        ILogger logger,
        UnitOfWorkFacade unitOfWorkFacade)
    {
        Logger = logger;
        _unitOfWorkFacade = unitOfWorkFacade;
    }

    public int CountAll()
    {
        throw new NotImplementedException();
    }

    public int Count(Expression<Func<PersonAssociation, bool>> predicate)
    {
        throw new NotImplementedException();
    }

    public int Count(IList<Expression<Func<PersonAssociation, bool>>> predicates)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<PersonAssociation>> GetAll()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<PersonAssociation>> Find(Expression<Func<PersonAssociation, bool>> predicate)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<PersonAssociation>> Find(IList<Expression<Func<PersonAssociation, bool>>> predicates)
    {
        throw new NotImplementedException();
    }

    public PersonAssociation SingleOrDefault(Expression<Func<PersonAssociation, bool>> predicate)
    {
        throw new NotImplementedException();
    }

    public Task Add(PersonAssociation entity)
    {
        throw new NotImplementedException();
    }

    public Task AddRange(IEnumerable<PersonAssociation> entities)
    {
        throw new NotImplementedException();
    }

    public Task Update(PersonAssociation entity)
    {
        throw new NotImplementedException();
    }

    public Task UpdateRange(IEnumerable<PersonAssociation> entities)
    {
        throw new NotImplementedException();
    }

    public Task Remove(PersonAssociation entity)
    {
        throw new NotImplementedException();
    }

    public Task RemoveRange(IEnumerable<PersonAssociation> entities)
    {
        throw new NotImplementedException();
    }

    public async Task Clear()
    {
        await UnitOfWork.PersonAssociations.Clear();
    }

    public void Load(IEnumerable<PersonAssociation> entities)
    {
        throw new NotImplementedException();
    }

    public Task<PersonAssociation> Get(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<PersonAssociation>> GetAllVariants(Guid id)
    {
        throw new NotImplementedException();
    }
}
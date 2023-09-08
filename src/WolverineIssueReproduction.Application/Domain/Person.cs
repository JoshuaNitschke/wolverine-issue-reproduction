using Marten.Schema;
using NodaTime;

namespace WolverineIssueReproduction.Application.Domain;

public class Person
{
    public Guid? Id { get; set; }

    [UniqueIndex(IndexType = UniqueIndexType.DuplicatedField)]
    public string Email { get; set; }
    
    public Instant CreatedAt { get; set; }
}
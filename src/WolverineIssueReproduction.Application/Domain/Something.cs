using Marten.Schema;
using NodaTime;

namespace WolverineIssueReproduction.Application.Domain;

public class Something
{
    [ForeignKey(typeof(Person))]
    public Guid? PersonId { get; set; }
    public Instant CreatedAt { get; set; }
}
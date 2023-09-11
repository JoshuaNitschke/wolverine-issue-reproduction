using Marten;
using NodaTime;
using Wolverine.Http;
using WolverineIssueReproduction.Application;
using WolverineIssueReproduction.Application.Sagas;

namespace WolverineIssueReproduction.WebApi.Controllers;

public class TestingEndpoint
{
    [WolverineGet("now")]
    public NowDto Now(IClock clock)
    {
        return new NowDto()
        {
            NowDateTime = DateTime.Now,
            NowInstant = clock.GetCurrentInstant()
        };
    }
}

public class NowDto
{
    public DateTime NowDateTime { get; set; }
    public Instant NowInstant { get; set; }
}

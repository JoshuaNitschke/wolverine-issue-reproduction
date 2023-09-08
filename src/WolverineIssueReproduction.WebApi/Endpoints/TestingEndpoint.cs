using Marten;
using Wolverine.Http;
using WolverineIssueReproduction.Application;
using WolverineIssueReproduction.Application.Sagas;

namespace WolverineIssueReproduction.WebApi.Controllers;

public class TestingEndpoint
{   
    [WolverinePost("WorksOnlyIfUseDurableLocalQueuesIsTrue/log")]
    public (string, LogCommand) DurableOnlyLog(IDocumentSession session)
    {
        return ("I won't log without durable queues", new LogCommand("HOWDY!"));
    }
    
    [WolverinePost("WorksOnlyIfUseDurableLocalQueuesIsTrue/start-saga")]
    public (string, StartTest) DurableOnlyTestSaga(IDocumentSession session)
    {
        var id = Guid.NewGuid();
        return (id.ToString(), new StartTest(id));
    }
    
    
    [WolverinePost("log")]
    public (string, LogCommand) Log()
    {
        return ("I always log", new LogCommand("HOWDY!"));
    }
    
    [WolverinePost("start-saga")]
    public (string, StartTest) TestSaga()
    {
        var id = Guid.NewGuid();
        return (id.ToString(), new StartTest(id));
    }
}

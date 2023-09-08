using Marten;
using Wolverine.Http;
using WolverineIssueReproduction.Application;
using WolverineIssueReproduction.Application.Sagas;

namespace WolverineIssueReproduction.WebApi.Controllers;

public class TestingEndpoint
{   
    [WolverinePost("idocument_session/log")]
    public (string, LogCommand) DurableOnlyLog(IDocumentSession session)
    {
        return ("I won't log without durable queues", new LogCommand("I won't log without durable queues!"));
    }
    
    [WolverinePost("idocument_session/start-saga")]
    public (string, StartTest) DurableOnlyTestSaga(IDocumentSession session)
    {
        var id = Guid.NewGuid();
        return (id.ToString(), new StartTest(id, "I won't start without a durable queue!"));
    }
    
    
    [WolverinePost("log")]
    public (string, LogCommand) Log()
    {
        return ("I always log!", new LogCommand("I always log!"));
    }
    
    [WolverinePost("start-saga")]
    public (string, StartTest) TestSaga()
    {
        var id = Guid.NewGuid();
        return (id.ToString(), new StartTest(id, "I always start!"));
    }
}

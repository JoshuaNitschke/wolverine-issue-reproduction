using Marten;
using Wolverine.Http;
using WolverineIssueReproduction.Application;
using WolverineIssueReproduction.Application.Sagas;

namespace WolverineIssueReproduction.WebApi.Controllers;

public class TestingEndpoint
{   
    [WolverinePost("WorksOnlyIfUseDurableLocalQueuesIsTrue/log")]
    public (string, LogCommand) DurableOnlyLog(IDocumentSession sessio)
    {
        return ("response", new LogCommand("HOWDY!"));
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
        return ("why no logs", new LogCommand("HOWDY!"));
    }
    
    [WolverinePost("start-saga")]
    public (string, StartTest) TestSaga()
    {
        var id = Guid.NewGuid();
        return (id.ToString(), new StartTest(id));
    }
}

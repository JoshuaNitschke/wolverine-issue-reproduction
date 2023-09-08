using Alba;
using Oakton;
using Shouldly;
using Wolverine.Tracking;
using WolverineIssueReproduction.Application;
using WolverineIssueReproduction.Application.Sagas;


namespace WolverineIssueReproduction.Tests;

public class AppFixture : IAsyncLifetime
{
    public IAlbaHost WebApi { get; private set; }

    public async Task InitializeAsync()
    {
        OaktonEnvironment.AutoStartHost = true;
        WebApi = await AlbaHost.For<Program>(x => { });
    }

    public async Task DisposeAsync()
    {
        await WebApi.DisposeAsync();
    }
    
    [Fact]
    public async Task idocument_session_log_sends_message()
    {
        await using var host = await AlbaHost.For<Program>(x => { });
        var (tracked, result) = await TrackedHttpCall(x =>
        {
            x.Post.Text("").ToUrl("/idocument_session/log");
        });
 
        // "tracked" is a Wolverine ITrackedSession object that lets us interrogate
        // what messages were published, sent, and handled during the testing perioc
        tracked.Sent.SingleMessage<LogCommand>().Message.ShouldBe("I won't log without durable queues!");
    }
    
    [Fact]
    public async Task idocument_session_start_saga_sends_message()
    {
        await using var host = await AlbaHost.For<Program>(x => { });
        var (tracked, result) = await TrackedHttpCall(x =>
        {
            x.Post.Text("").ToUrl("/idocument_session/start-saga");
        });
 
        // "tracked" is a Wolverine ITrackedSession object that lets us interrogate
        // what messages were published, sent, and handled during the testing perioc
        tracked.Sent.SingleMessage<StartTest>().StartMessage.ShouldBe("I won't start without a durable queue!");
    }
    
    
    [Fact]
    public async Task log_sends_message()
    {
        await using var host = await AlbaHost.For<Program>(x => { });
        var (tracked, result) = await TrackedHttpCall(x =>
        {
            x.Post.Text("").ToUrl("/log");
        });
 
        // "tracked" is a Wolverine ITrackedSession object that lets us interrogate
        // what messages were published, sent, and handled during the testing perioc
        tracked.Sent.SingleMessage<LogCommand>().Message.ShouldBe("I always log!");
    }
    
    [Fact]
    public async Task start_saga_sends_message()
    {
        await using var host = await AlbaHost.For<Program>(x => { });
        var (tracked, result) = await TrackedHttpCall(x =>
        {
            x.Post.Text("").ToUrl("/start-saga");
        });
 
        // "tracked" is a Wolverine ITrackedSession object that lets us interrogate
        // what messages were published, sent, and handled during the testing perioc
        tracked.Sent.SingleMessage<StartTest>().StartMessage.ShouldBe("I always start!");
    }
    
    
    
    
    
    // This method allows us to make HTTP calls into our system
    // in memory with Alba, but do so within Wolverine's test support
    // for message tracking to both record outgoing messages and to ensure
    // that any cascaded work spawned by the initial command is completed
    // before passing control back to the calling test
    protected async Task<(ITrackedSession, IScenarioResult)> TrackedHttpCall(Action<Scenario> configuration)
    {
        IScenarioResult result = null;
 
        // The outer part is tying into Wolverine's test support
        // to "wait" for all detected message activity to complete
        var tracked = await WebApi.ExecuteAndWaitAsync(async () =>
        {
            // The inner part here is actually making an HTTP request
            // to the system under test with Alba
            result = await WebApi.Scenario(configuration);
        });
 
        return (tracked, result);
    }
}


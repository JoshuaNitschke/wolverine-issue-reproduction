using Alba;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Oakton;
using Shouldly;
using Wolverine.Tracking;
using WolverineIssueReproduction.WebApi.Controllers;


namespace WolverineIssueReproduction.Tests;

public class AppFixture : IAsyncLifetime
{
    public IAlbaHost WebApi { get; private set; }

    public async Task InitializeAsync()
    {
        OaktonEnvironment.AutoStartHost = true;
        WebApi = await AlbaHost.For<Program>(x => { });
        var store = WebApi.Services.GetRequiredService<IDocumentStore>();
        await store.Advanced.Clean.DeleteAllDocumentsAsync();
        await store.Advanced.Clean.DeleteAllEventDataAsync();
        await store.Advanced.Clean.CompletelyRemoveAllAsync();
    }

    public async Task DisposeAsync()
    {
        await WebApi.DisposeAsync();
    }
    
    [Fact]
    public async Task fail1_endpoint_should_track_all_events()
    {
        var (tracked, _) = await TrackedHttpCall(x => { 
            x.Post.Url("/fail1");            
            x.StatusCodeShouldBe(204);
        });
        tracked.Sent.MessagesOf<NewStreamEvent>().Count().ShouldBe(1);     // PASSES
        tracked.Sent.MessagesOf<DeleteStreamEvent>().Count().ShouldBe(1); // this one won't get tracked unless I comment out the StartStream line
        tracked.Sent.MessagesOf<EventFoo>().Count().ShouldBe(2);
        tracked.Sent.MessagesOf<EventBar>().Count().ShouldBe(1);
    }
    
    [Fact]
    public async Task fail2_endpoint_should_track_all_events()
    {
        var (tracked, _) = await TrackedHttpCall(x => { 
            x.Post.Url("/fail2");            
            x.StatusCodeShouldBe(204);
        });
        tracked.Sent.MessagesOf<EventFoo>().Count().ShouldBe(2);
        tracked.Sent.MessagesOf<EventBar>().Count().ShouldBe(1);
    }
    
    [Fact]
    public async Task events_from_handlers_when_when_work_causes_projection_events_should_not_be_lost()
    {
        var (tracked, _) = await TrackedHttpCall(x => { 
            x.Post.Url("/fail3");            
            x.StatusCodeShouldBe(204);
        });
        tracked.Executed.MessagesOf<EventFooBar>().Count().ShouldBe(1);
        tracked.Sent.MessagesOf<EventFoo>().Count().ShouldBe(1);
        tracked.Sent.MessagesOf<EventBar>().Count().ShouldBe(1);
    }
    
    [Fact]
    public async Task ok_endpoint_is_ok()
    {
        var (tracked, _) = await TrackedHttpCall(x => { 
            x.Post.Url("/ok");            
            x.StatusCodeShouldBe(204);
        });
        
        tracked.Sent.MessagesOf<EventFoo>().Count().ShouldBe(2);
        tracked.Sent.MessagesOf<EventBar>().Count().ShouldBe(1);
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


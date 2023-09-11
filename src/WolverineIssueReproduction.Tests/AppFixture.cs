using Alba;
using NodaTime;
using Oakton;
using Shouldly;
using Wolverine.Tracking;
using WolverineIssueReproduction.Application;
using WolverineIssueReproduction.Application.Sagas;
using WolverineIssueReproduction.WebApi.Controllers;


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
    public async Task instant_should_serialize_to_and_from_json_string()
    {
        await using var host = await AlbaHost.For<Program>(x => { });
        var (_, result) = await TrackedHttpCall(x => { x.Get.Url("/now"); });
        
        // The JSON formatter was unable to process the raw JSON:
        // {"nowDateTime":"2023-09-11T16:52:55.1197728-07:00","nowInstant":{}}
        result.ReadAsJson<NowDto>()?.ShouldNotBeNull();
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


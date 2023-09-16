using Alba;
using Microsoft.AspNetCore.Mvc;
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
    }

    public async Task DisposeAsync()
    {
        await WebApi.DisposeAsync();
    }
    
    [Fact]
    public async Task should_not_have_results_does_not_exist_in_namespace_error()
    {
        var (_, result) = await TrackedHttpCall(x => { x.Get.Url("/problem-details-1"); });
        result.ReadAsText().ShouldBe("hi");
    }
    
    [Fact]
    public async Task problem_details_works_fine_here()
    {
        var (_, result) = await TrackedHttpCall(x => { x.Get.Url("/problem-details-2"); });
        result.ReadAsJson<ProblemDetails>().Detail.ShouldBe("Houston, we have a problem!");
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


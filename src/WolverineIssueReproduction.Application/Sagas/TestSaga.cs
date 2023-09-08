namespace WolverineIssueReproduction.Application.Sagas;

using JasperFx.Core;
using Microsoft.Extensions.Logging;
using Wolverine;

public record StartTest(Guid RequestId);
public record TestTimeout(Guid Id) : TimeoutMessage(10.Seconds());

public class TestSaga : Saga
{
    public Guid Id { get; set; }

    // to start the Saga with defaults
    public static (
        TestSaga,
        TestTimeout) Start(StartTest request, ILogger<TestSaga> logger)
    {
        logger.LogInformation("Got a new request with id {Id}", request.RequestId);
        return (
            new TestSaga { Id = request.RequestId },
            new TestTimeout(request.RequestId)
        );
    }
    
    public void Handle(TestTimeout awaitResponseTimeout, IMessageContext messageContext, ILogger<TestSaga> logger)
    {
        logger.LogInformation("{Saga} {Id} TIMEOUT OUT - MARKING COMPLETE", nameof(Saga), awaitResponseTimeout.Id);
        MarkCompleted();
    }
}

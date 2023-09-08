namespace WolverineIssueReproduction.Application.Sagas;

using JasperFx.Core;
using Microsoft.Extensions.Logging;
using Wolverine;

public record StartTest(Guid RequestId, string StartMessage);
public record TestTimeout(Guid Id) : TimeoutMessage(1.Seconds());

public class TestSaga : Saga
{
    public Guid Id { get; set; }

    // to start the Saga with defaults
    public static (
        TestSaga,
        TestTimeout) Start(StartTest request, ILogger<TestSaga> logger)
    {
        logger.LogInformation("Got a new request with id {Id} and {StartMessage}", request.RequestId, request.StartMessage);
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

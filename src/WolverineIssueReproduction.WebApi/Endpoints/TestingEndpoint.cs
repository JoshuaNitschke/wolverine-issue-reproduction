using System.ComponentModel.DataAnnotations;
using Marten;
using Marten.Events;
using Marten.Events.Projections;
using Marten.Linq;
using Marten.Schema.Identity;
using Wolverine;
using Wolverine.Attributes;
using Wolverine.Http;

namespace WolverineIssueReproduction.WebApi.Controllers;

public class FailingEndpoint
{

    public async Task<string> LoadAsync(IMessageBus bus, IDocumentSession session)
    {
        var @event = new NewStreamEvent(CombGuidIdGeneration.NewGuid());
        session.Events.StartStream<ExampleStream>(@event.Id, @event);  // if you comment out this line, the DeleteStreamEvent will get tracked
        await session.SaveChangesAsync();
        await session.Events.WriteToAggregate<ExampleStream>(@event.Id, stream => stream.AppendOne(new DeleteStreamEvent(@event.Id)));
        await bus.SendAsync(new EventBar(1));
        await bus.SendAsync(new EventFoo(2));
        await bus.SendAsync(new EventFoo(3));
        return "Blah";
    }
        
        
    [WolverinePost("fail1")]
    public async Task Post([Required] string text2 = "default")
    {
    }
}

public class FailingEndpoint2
{

    public async Task<string> LoadAsync(IMessageBus bus, IDocumentSession session)
    {
        // demonstrating unused injection is enough to cause failure
        await bus.SendAsync(new EventBar(1));
        await bus.SendAsync(new EventFoo(2));
        await bus.SendAsync(new EventFoo(3));
        return "Blah";
    }
        
        
    [WolverinePost("fail2")]
    public async Task Post([Required] string text2 = "default")
    {
    }
}


public class FailingEndpoint3
{
    [WolverinePost("fail3")]
    public async Task Post(IMessageBus bus, IDocumentSession session)
    {
        await session.Query<ExampleStreamDetails>().FirstOrDefaultAsync(p => true);
        await bus.InvokeAsync(new EventFooBar(1));
    }
}

public class OkEndpoint
{

    public async Task<string> LoadAsync(IMessageBus bus)
    {
        await bus.SendAsync(new EventBar(1));
        await bus.SendAsync(new EventFoo(2));
        await bus.SendAsync(new EventFoo(3));
        return "Blah";
    }
        
        
    [WolverinePost("ok")]
    public async Task Post([Required] string text2 = "default")
    {
    }
}


public record NewStreamEvent(Guid Id);
public record DeleteStreamEvent(Guid Id);


public record ExampleStreamDetails(Guid Id);


public record ExampleStream(Guid Id)
{
    public static ExampleStream Create(NewStreamEvent @event) => new(@event.Id);
    public ExampleStream Handle(NewStreamEvent @event, ExampleStream current) => current;

}

public record EventFoo(int number);
public record EventBar(int number);

public record EventFooBar(int number);

public class ExampleStreamProjection : EventProjection
{
    public ExampleStreamDetails Create(IEvent<NewStreamEvent> e) => new (e.Data.Id);
    public void Project(IEvent<DeleteStreamEvent> e, IDocumentOperations ops) {
        ops.DeleteWhere<ExampleStreamDetails>(x => x.Id == e.Data.Id);
    }

}


[WolverineHandler]
public class EventHandlers
{
    private readonly IMessageBus _bus;
    private readonly IDocumentSession _session;

    public EventHandlers(IMessageBus bus, IDocumentSession session)
    {
        _bus = bus;
        _session = session;
    }
    
    public void Handle(EventFoo e)
    {
        //TODO: do something
    }
    
    public void Handle(EventBar e)
    {
        //TODO: do something
    }
    
    public void Handle(NewStreamEvent e)
    {
        //TODO: do something
    }
    
    public void Handle(DeleteStreamEvent e)
    {
        //TODO: do something
    }
    
    public async Task Handle(EventFooBar e)
    {
        var @event = new NewStreamEvent(CombGuidIdGeneration.NewGuid()); // if you comment out this line, tests= 
        _session.Events.StartStream<ExampleStream>(@event.Id, @event);  // if you comment out this line, the DeleteStreamEvent will get tracked
        await _session.SaveChangesAsync();
        await _bus.SendAsync(new EventFoo(e.number));
        await _bus.SendAsync(new EventBar(e.number));
    }
}

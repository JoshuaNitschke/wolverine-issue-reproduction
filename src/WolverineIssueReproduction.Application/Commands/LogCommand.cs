using Microsoft.Extensions.Logging;

namespace WolverineIssueReproduction.Application;

public record LogCommand(string Message);
    
public class LogCommandHandler
{
    public static void Handle(LogCommand command, ILogger<LogCommandHandler> log)
    {
        log.LogInformation(command.Message);
    }   
}
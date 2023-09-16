using Marten;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using Wolverine.Http;

namespace WolverineIssueReproduction.WebApi.Controllers;

public class ProblemDetails1Endpoint
{
    public ProblemDetails Before()
    {
        return new ProblemDetails
        {
            Detail = "Houston, we have a problem!",
            Status = 400
        };
            
    }
    
    [WolverineGet("problem-details-1")]
    public string Get()
    {
        return "hi";
    }
}


public class ProblemDetails21Endpoint
{ 
    [WolverineGet("problem-details-2")]
    public ProblemDetails Get()
    {
        return new ProblemDetails
        {
            Detail = "Houston, we have a problem!",
            Status = 400
        };    
    }
}

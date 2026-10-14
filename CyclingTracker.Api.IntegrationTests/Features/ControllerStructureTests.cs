using CyclingTracker.Api.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CyclingTracker.Api.IntegrationTests.Features;

public class ControllerStrutureTests// : IClassFixture<IntegrationFactory>, IDisposable
{
    [Fact]
    public void Every_controller_declares_its_authorization()
    {
        var missing = typeof(ImportController).Assembly.GetTypes()
            .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract)
            .Where(t => !t.IsDefined(typeof(AuthorizeAttribute), inherit: true)
                     && !t.IsDefined(typeof(AllowAnonymousAttribute), inherit: true))
            .Select(t => t.Name);

        Assert.Empty(missing);
    }
}
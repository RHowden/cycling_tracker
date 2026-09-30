using CyclingTracker.Api.IntegrationTests.Infrastructure;

public abstract class WebApplicationFactoryTestBase(IntegrationFactory factory)
 : DbTestBase(factory.Postgres), IAsyncLifetime
{
    protected IntegrationFactory Factory { get; } = factory;
}

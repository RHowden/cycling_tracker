public abstract class DbTestBase(PostgresFixture postgres) : IAsyncLifetime
{
    protected PostgresFixture Postgres { get; } = postgres;

    // Only hits the DB fixture to reset
    public virtual Task InitializeAsync() => Postgres.ResetAsync();
    public virtual Task DisposeAsync() => Task.CompletedTask;
}

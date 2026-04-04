using DataAccess.Context;

namespace Tests.Core.DatabaseIntegration;

[TestClass]
public abstract class DatabaseIntegrationTestBase
{
    protected EventorDbContext? DbContext { get; private set; }

    [TestInitialize]
    public void TestInitialize()
    {
        DbContext = DatabaseIntegrationTestConfiguration.GetDbContext();
        TestCleanup();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        if (DbContext == null) return;

        DbContext.Users.RemoveRange(DbContext.Users);
        DbContext.Events.RemoveRange(DbContext.Events);
        DbContext.Locations.RemoveRange(DbContext.Locations);
        DbContext.Registrations.RemoveRange(DbContext.Registrations);
        DbContext.Days.RemoveRange(DbContext.Days);
        DbContext.Menus.RemoveRange(DbContext.Menus);
        DbContext.Items.RemoveRange(DbContext.Items);
        DbContext.Feedbacks.RemoveRange(DbContext.Feedbacks);
        DbContext.Participations.RemoveRange(DbContext.Participations);

        DbContext.SaveChanges();
    }
}
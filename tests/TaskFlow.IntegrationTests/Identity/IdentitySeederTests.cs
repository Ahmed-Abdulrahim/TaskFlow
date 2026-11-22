namespace TaskFlow.IntegrationTests.Identity;

public class IdentitySeederTests
{
    [Fact]
    public async Task SeedRolesAsync_WhenRolesDoNotExist_CreatesAdminAndMember()
    {
        await using var connection = await CreateConnectionAsync();
        using var provider = CreateServiceProvider(connection);

        await EnsureDatabaseCreatedAsync(provider);

        await IdentitySeeder.SeedRolesAsync(provider);

        using var scope = provider.CreateScope();
        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();

        Assert.True(await roleManager.RoleExistsAsync(RoleNames.Admin));
        Assert.True(await roleManager.RoleExistsAsync(RoleNames.Member));
        Assert.Equal(2, await roleManager.Roles.CountAsync());
    }

    [Fact]
    public async Task SeedRolesAsync_WhenExecutedTwice_DoesNotDuplicateRoles()
    {
        await using var connection = await CreateConnectionAsync();
        using var provider = CreateServiceProvider(connection);

        await EnsureDatabaseCreatedAsync(provider);

        await IdentitySeeder.SeedRolesAsync(provider);
        await IdentitySeeder.SeedRolesAsync(provider);

        using var scope = provider.CreateScope();
        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();

        Assert.Equal(2, await roleManager.Roles.CountAsync());
    }

    [Fact]
    public async Task SeedRolesAsync_WhenAdminAlreadyExists_PreservesExistingRole()
    {
        await using var connection = await CreateConnectionAsync();
        using var provider = CreateServiceProvider(connection);

        await EnsureDatabaseCreatedAsync(provider);

        string existingAdminId;

        using (var scope = provider.CreateScope())
        {
            var roleManager = scope.ServiceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();

            var existingAdmin = new IdentityRole(RoleNames.Admin);

            var result = await roleManager.CreateAsync(existingAdmin);

            Assert.True(result.Succeeded);

            existingAdminId = existingAdmin.Id;
        }

        await IdentitySeeder.SeedRolesAsync(provider);

        using var verificationScope = provider.CreateScope();
        var verificationRoleManager = verificationScope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();

        var adminRole = await verificationRoleManager
            .FindByNameAsync(RoleNames.Admin);

        Assert.NotNull(adminRole);
        Assert.Equal(existingAdminId, adminRole.Id);

        Assert.True(
            await verificationRoleManager.RoleExistsAsync(RoleNames.Member));

        Assert.Equal(2, await verificationRoleManager.Roles.CountAsync());
    }

    private static async Task<SqliteConnection> CreateConnectionAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        return connection;
    }

    private static ServiceProvider CreateServiceProvider(
        SqliteConnection connection)
    {
        var services = new ServiceCollection();

        services.AddLogging();

        services.AddDbContext<TaskFlowDbContext>(options =>
            options.UseSqlite(connection));

        services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<TaskFlowDbContext>();

        return services.BuildServiceProvider();
    }

    private static async Task EnsureDatabaseCreatedAsync(
        IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<TaskFlowDbContext>();

        await dbContext.Database.EnsureCreatedAsync();
    }
}

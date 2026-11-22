namespace TaskFlow.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var roleName in RoleNames.All)
        {
            if (await roleManager.RoleExistsAsync(roleName))
                continue;

            var result = await roleManager.CreateAsync(
                new IdentityRole(roleName));

            if (!result.Succeeded &&
                !await roleManager.RoleExistsAsync(roleName))
            {
                var errors = string.Join(
                    ", ",
                    result.Errors.Select(error => error.Description));

                throw new InvalidOperationException(
                    $"Failed to create role '{roleName}'. Errors: {errors}");
            }
        }
    }
}

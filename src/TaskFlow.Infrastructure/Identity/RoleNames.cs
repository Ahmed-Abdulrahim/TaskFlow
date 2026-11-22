namespace TaskFlow.Infrastructure.Identity;

public static class RoleNames
{
    public const string Admin
     = "Admin";
    public const string Member = "Member";
    public static readonly IReadOnlyCollection<string> All =
    [
        Admin,
        Member
    ];
}
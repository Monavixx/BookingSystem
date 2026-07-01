using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Users;
using Tests.Builders;

namespace Tests.Services;

public class UserTestDataService(AppDbContext dbContext)
{
    public async Task<User> CreateUserAsync(Action<UserBuilder> config)
    {
        var userBuilder = new UserBuilder();
        config.Invoke(userBuilder);
        var user = userBuilder.Build();
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        return user;
    }

    public async Task<User[]> CreateUsersAsync(Action<UsersBuilder> config)
    {
        var builder = new UsersBuilder();
        config.Invoke(builder);
        var users = builder.Select(b => b.Build()).ToArray();
        dbContext.Users.AddRange(users);
        await dbContext.SaveChangesAsync();
        return users;
    }

    public async Task<User> CreateAdminAsync(string username = "monavixx", string email = "monavixx@gmail.com",
        string phoneNumber = "+79009009090")
    {
        var user = new UserBuilder()
            .WithUsername(username)
            .WithEmail(email)
            .WithPhoneNumber(phoneNumber)
            .WithRole(UserRole.Admin)
            .Build();
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        return user;
    }

    public async Task<User> CreateManagerAsync(string username = "lofectr650", string email = "lofectr6@gmail.com",
        string phoneNumber = "+79996665544")
    {
        var user = new UserBuilder()
            .WithUsername(username)
            .WithEmail(email)
            .WithPhoneNumber(phoneNumber)
            .WithRole(UserRole.Manager)
            .Build();
        dbContext.Users.Add(user);
        dbContext.Managers.Add(Manager.Create(user.Id));
        await dbContext.SaveChangesAsync();
        return user;
    }

    public async Task<User> CreateGuestAsync(string username = "randomGuy12", string email = "popakaka42@gmail.com",
        string phoneNumber = "+78506504555")
    {
        var user = new UserBuilder()
            .WithUsername(username)
            .WithEmail(email)
            .WithPhoneNumber(phoneNumber)
            .WithRole(UserRole.Guest)
            .Build();
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        return user;
    }

    private static readonly User BaseAdmin = new UserBuilder()
        .WithUsername("monavixx")
        .WithEmail("monavixx@gmail.com")
        .WithPhoneNumber("+79009009090")
        .WithRole(UserRole.Admin)
        .Build();

    private static readonly User BaseManager = new UserBuilder()
        .WithUsername("lofectr650")
        .WithEmail("lofectr6@gmail.com")
        .WithPhoneNumber("+79996665544")
        .WithRole(UserRole.Manager)
        .Build();

    private static readonly User BaseGuest = new UserBuilder()
        .WithUsername("randomGuy12")
        .WithEmail("popakaka42@gmail.com")
        .WithPhoneNumber("+78506504555")
        .WithRole(UserRole.Guest)
        .Build();

    private static readonly User[] BaseUsers = [BaseAdmin, BaseManager, BaseGuest];

    /// <returns>[0] admin, [1] manager, [2] guest</returns>
    public async Task<User[]> CreateBase3Async()
    {
        dbContext.Users.AddRange(BaseUsers);
        dbContext.Managers.Add(Manager.Create(BaseManager.Id));
        await dbContext.SaveChangesAsync();
        return BaseUsers;
    }
}
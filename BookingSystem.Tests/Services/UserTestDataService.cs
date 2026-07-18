using BookingSystem.Application.Persistence;
using BookingSystem.Domain.Users;
using BookingSystem.Tests.Builders;

namespace BookingSystem.Tests.Services;

public class UserTestDataService(AppDbContext dbContext, TimeProvider timeProvider)
{
    public async Task<User> CreateUserAsync(Action<UserBuilder> config)
    {
        var userBuilder = new UserBuilder();
        config.Invoke(userBuilder);
        var user = userBuilder.Build(timeProvider);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        return user;
    }

    public async Task<User[]> CreateUsersAsync(Action<UsersBuilder> config)
    {
        var builder = new UsersBuilder();
        config.Invoke(builder);
        var users = builder.Select(b => b.Build(timeProvider)).ToArray();
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
            .Build(timeProvider);
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
            .Build(timeProvider);
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
            .Build(timeProvider);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        return user;
    }

    private static UserBuilder BaseAdmin => new UserBuilder()
        .WithUsername("monavixx")
        .WithEmail("monavixx@gmail.com")
        .WithPhoneNumber("+79009009090")
        .WithRole(UserRole.Admin);

    private static UserBuilder BaseManager => new UserBuilder()
        .WithUsername("lofectr650")
        .WithEmail("lofectr6@gmail.com")
        .WithPhoneNumber("+79996665544")
        .WithRole(UserRole.Manager);

    private static UserBuilder BaseGuest => new UserBuilder()
        .WithUsername("randomGuy12")
        .WithEmail("popakaka42@gmail.com")
        .WithPhoneNumber("+78506504555")
        .WithRole(UserRole.Guest);

    private static UserBuilder BaseAnotherGuest => new UserBuilder()
        .WithUsername("anotherGuest54")
        .WithEmail("anotherguest54@gmail.com")
        .WithPhoneNumber("+78106574556")
        .WithRole(UserRole.Guest);

    private static UserBuilder BaseAnotherManager => new UserBuilder()
        .WithUsername("moneger_wow228")
        .WithEmail("monegEr_wow228@gmail.com")
        .WithPhoneNumber("+79996666744")
        .WithRole(UserRole.Manager);

    private static UserBuilder[] Base3Users => [BaseAdmin, BaseManager, BaseGuest];
    private static UserBuilder[] Base4Users => [BaseAdmin, BaseManager, BaseGuest, BaseAnotherGuest];

    private static UserBuilder[] Base5Users =>
        [BaseAdmin, BaseManager, BaseAnotherManager, BaseGuest, BaseAnotherGuest];
    private User[] BuildUsers(UserBuilder[] builders) => builders.Select(b => b.Build(timeProvider)).ToArray();

    /// <returns>[0] admin, [1] manager, [2] guest</returns>
    public async Task<User[]> CreateBase3Async()
    {
        var users = BuildUsers(Base3Users);
        dbContext.Users.AddRange(users);
        dbContext.Managers.Add(Manager.Create(users[1].Id));
        await dbContext.SaveChangesAsync();
        return users;
    }

    public async Task<(User Admin, User Manager, User Guest, User AnotherGuest)> CreateBase4Async()
    {
        var users = BuildUsers(Base4Users);
        dbContext.Users.AddRange(users);
        dbContext.Managers.Add(Manager.Create(users[1].Id));
        await dbContext.SaveChangesAsync();
        return (users[0], users[1], users[2], users[3]);
    }
    public async Task<Base5Users> CreateBase5Async()
    {
        var users = BuildUsers(Base5Users);
        dbContext.Users.AddRange(users);
        dbContext.Managers.AddRange(Manager.Create(users[1].Id), Manager.Create(users[2].Id));
        await dbContext.SaveChangesAsync();
        return new Base5Users(users[0], users[1], users[2], users[3], users[4]);
    }
}

public sealed record Base5Users(User Admin, User Manager, User AnotherManager, User Guest, User AnotherGuest);
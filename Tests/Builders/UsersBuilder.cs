namespace Tests.Builders;

public class UsersBuilder : List<UserBuilder>
{
    public UserBuilder New()
    {
        var builder = new UserBuilder();
        Add(builder);
        return builder;
    }
}
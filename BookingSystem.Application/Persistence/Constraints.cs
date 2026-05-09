namespace BookingSystem.Application.Persistence;

public static class Constraints
{
    public static class Unique
    {
        public static readonly ConstraintData UserUsername = new(TableNames.Users, "IX_Users_Username");
        public static readonly ConstraintData UserEmail = new(TableNames.Users, "IX_Users_Email");
        public static readonly ConstraintData UserPhoneNumber = new(TableNames.Users, "IX_Users_PhoneNumber");
    }

    public static class ForeignKey
    {
        public static readonly ConstraintData FavoriteRestaurantsUser =
            new(TableNames.FavoriteRestaurants, "FK_FavoriteRestaurants_Users_UserId");
        public static readonly ConstraintData SessionsUser = new(TableNames.Sessions, "FK_Sessions_Users_UserId");
        public static readonly ConstraintData TablesRestaurant = new(TableNames.Tables, "FK_Tables_Restaurants_RestaurantId");
        public static readonly ConstraintData FavoriteRestaurantsRestaurant =
            new(TableNames.FavoriteRestaurants, "FK_FavoriteRestaurants_Restaurants_RestaurantId");
    }

    public static string PrimaryKey(string tableName)
        => $"PK_{tableName}";

    public static bool IsPrimaryKeyConstraint(string constraintName)
        => constraintName.StartsWith("PK_");
}
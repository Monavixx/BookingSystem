using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingSystem.Application.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    password_hash = table.Column<byte[]>(type: "bytea", maxLength: 144, nullable: false),
                    registration_date_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    is_blocked = table.Column<bool>(type: "boolean", nullable: false),
                    blocked_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "managers",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_managers", x => x.user_id);
                    table.ForeignKey(
                        name: "fk_managers_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    refresh_token_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    refresh_token_token = table.Column<byte[]>(type: "bytea", maxLength: 32, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sessions", x => x.id);
                    table.ForeignKey(
                        name: "fk_sessions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "restaurants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    contact_phone_number = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    description = table.Column<string>(type: "character varying(350)", maxLength: 350, nullable: true),
                    image_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    address_apartment_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    address_city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    address_country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    address_house_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    address_state = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    address_street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    address_zip_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_restaurants", x => x.id);
                    table.ForeignKey(
                        name: "fk_restaurants_managers_owner_id",
                        column: x => x.owner_id,
                        principalTable: "managers",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "favorite_restaurants",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    restaurant_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_favorite_restaurants", x => new { x.user_id, x.restaurant_id });
                    table.ForeignKey(
                        name: "fk_favorite_restaurants_restaurants_restaurant_id",
                        column: x => x.restaurant_id,
                        principalTable: "restaurants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_favorite_restaurants_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "restaurant_daily_schedules",
                columns: table => new
                {
                    day_of_week = table.Column<byte>(type: "smallint", nullable: false),
                    restaurant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    opening_time = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    closing_time = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    is_closed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_restaurant_daily_schedules", x => new { x.restaurant_id, x.day_of_week });
                    table.ForeignKey(
                        name: "fk_restaurant_daily_schedules_restaurants_restaurant_id",
                        column: x => x.restaurant_id,
                        principalTable: "restaurants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tables",
                columns: table => new
                {
                    restaurant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    table_number = table.Column<int>(type: "integer", nullable: false),
                    capacity = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tables", x => new { x.restaurant_id, x.table_number });
                    table.ForeignKey(
                        name: "fk_tables_restaurants_restaurant_id",
                        column: x => x.restaurant_id,
                        principalTable: "restaurants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bookings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    guest_id = table.Column<Guid>(type: "uuid", nullable: false),
                    guest_count = table.Column<int>(type: "integer", nullable: false),
                    restaurant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    table_number = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    end_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    start_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bookings", x => x.id);
                    table.ForeignKey(
                        name: "fk_bookings_tables_restaurant_id_table_number",
                        columns: x => new { x.restaurant_id, x.table_number },
                        principalTable: "tables",
                        principalColumns: new[] { "restaurant_id", "table_number" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_bookings_users_guest_id",
                        column: x => x.guest_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cancellation_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    who_cancelled_id = table.Column<Guid>(type: "uuid", nullable: true),
                    booking_id = table.Column<Guid>(type: "uuid", nullable: true),
                    canceled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    reason = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cancellation_records", x => x.id);
                    table.ForeignKey(
                        name: "fk_cancellation_records_bookings_booking_id",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_cancellation_records_users_who_cancelled_id",
                        column: x => x.who_cancelled_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_bookings_guest_id",
                table: "bookings",
                column: "guest_id");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_restaurant_id_table_number",
                table: "bookings",
                columns: new[] { "restaurant_id", "table_number" });

            migrationBuilder.CreateIndex(
                name: "ix_cancellation_records_booking_id",
                table: "cancellation_records",
                column: "booking_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cancellation_records_who_cancelled_id",
                table: "cancellation_records",
                column: "who_cancelled_id");

            migrationBuilder.CreateIndex(
                name: "ix_favorite_restaurants_restaurant_id",
                table: "favorite_restaurants",
                column: "restaurant_id");

            migrationBuilder.CreateIndex(
                name: "ix_restaurants_owner_id",
                table: "restaurants",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_sessions_user_id",
                table: "sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_phone_number",
                table: "users",
                column: "phone_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_username",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cancellation_records");

            migrationBuilder.DropTable(
                name: "favorite_restaurants");

            migrationBuilder.DropTable(
                name: "restaurant_daily_schedules");

            migrationBuilder.DropTable(
                name: "sessions");

            migrationBuilder.DropTable(
                name: "bookings");

            migrationBuilder.DropTable(
                name: "tables");

            migrationBuilder.DropTable(
                name: "restaurants");

            migrationBuilder.DropTable(
                name: "managers");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}

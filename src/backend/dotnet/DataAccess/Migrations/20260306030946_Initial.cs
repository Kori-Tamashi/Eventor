using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:gender", "Мужчина,Женщина")
                .Annotation("Npgsql:Enum:registration_type", "Cтандартный,VIP,Организатор")
                .Annotation("Npgsql:Enum:user_role", "Администратор,Зарегистрированный пользователь,Гость");

            migrationBuilder.CreateTable(
                name: "items",
                columns: table => new
                {
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "varchar(255)", nullable: false),
                    cost = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_items", x => x.item_id);
                    table.CheckConstraint("CK_Items_Cost", "\"cost\" >= 0");
                });

            migrationBuilder.CreateTable(
                name: "locations",
                columns: table => new
                {
                    location_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "varchar(255)", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    cost = table.Column<decimal>(type: "numeric", nullable: false),
                    capacity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_locations", x => x.location_id);
                    table.CheckConstraint("CK_Location_Capacity", "\"capacity\" >= 0");
                    table.CheckConstraint("CK_Location_Cost", "\"cost\" >= 0");
                });

            migrationBuilder.CreateTable(
                name: "menu",
                columns: table => new
                {
                    menu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "varchar(255)", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menu", x => x.menu_id);
                    table.CheckConstraint("CK_Menu_DescriptionLength", "char_length(description) <= 8192");
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "varchar(255)", nullable: false),
                    phone = table.Column<string>(type: "varchar(255)", nullable: false),
                    gender = table.Column<int>(type: "gender", nullable: false),
                    password_hash = table.Column<string>(type: "varchar(255)", nullable: false),
                    role = table.Column<int>(type: "user_role", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "events",
                columns: table => new
                {
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "varchar(255)", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    location_id = table.Column<Guid>(type: "uuid", nullable: false),
                    days_count = table.Column<int>(type: "integer", nullable: false),
                    percent = table.Column<double>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_events", x => x.event_id);
                    table.CheckConstraint("CK_Event_DaysCount", "\"days_count\" >= 0");
                    table.CheckConstraint("CK_Event_DescriptionLength", "char_length(description) <= 8192");
                    table.CheckConstraint("CK_Event_Percent", "\"percent\" >= 0");
                    table.ForeignKey(
                        name: "FK_events_locations_location_id",
                        column: x => x.location_id,
                        principalTable: "locations",
                        principalColumn: "location_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "menu_items",
                columns: table => new
                {
                    menu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menu_items", x => new { x.menu_id, x.item_id });
                    table.ForeignKey(
                        name: "FK_menu_items_items_item_id",
                        column: x => x.item_id,
                        principalTable: "items",
                        principalColumn: "item_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_menu_items_menu_menu_id",
                        column: x => x.menu_id,
                        principalTable: "menu",
                        principalColumn: "menu_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "days",
                columns: table => new
                {
                    day_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    menu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "varchar(255)", nullable: false),
                    sequence_number = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_days", x => x.day_id);
                    table.CheckConstraint("CK_Days_DescriptionLength", "char_length(description) <= 8192");
                    table.CheckConstraint("CK_Days_SequenceNumber_Positive", "\"sequence_number\" > 0");
                    table.ForeignKey(
                        name: "FK_days_events_event_id",
                        column: x => x.event_id,
                        principalTable: "events",
                        principalColumn: "event_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_days_menu_menu_id",
                        column: x => x.menu_id,
                        principalTable: "menu",
                        principalColumn: "menu_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "registrations",
                columns: table => new
                {
                    registration_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<int>(type: "registration_type", nullable: false),
                    payment = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_registrations", x => x.registration_id);
                    table.ForeignKey(
                        name: "FK_registrations_events_event_id",
                        column: x => x.event_id,
                        principalTable: "events",
                        principalColumn: "event_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_registrations_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "feedbacks",
                columns: table => new
                {
                    feedback_id = table.Column<Guid>(type: "uuid", nullable: false),
                    RegistrationId = table.Column<Guid>(type: "uuid", nullable: false),
                    comment = table.Column<string>(type: "text", nullable: false),
                    rate = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feedbacks", x => x.feedback_id);
                    table.CheckConstraint("CK_Feedback_CommentLength", "char_length(comment) <= 4096");
                    table.CheckConstraint("CK_Feedback_Rate", "\"rate\" >= 1 AND \"rate\" <= 5");
                    table.ForeignKey(
                        name: "FK_feedbacks_registrations_RegistrationId",
                        column: x => x.RegistrationId,
                        principalTable: "registrations",
                        principalColumn: "registration_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "participation",
                columns: table => new
                {
                    day_id = table.Column<Guid>(type: "uuid", nullable: false),
                    registration_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_participation", x => new { x.day_id, x.registration_id });
                    table.ForeignKey(
                        name: "FK_participation_days_day_id",
                        column: x => x.day_id,
                        principalTable: "days",
                        principalColumn: "day_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_participation_registrations_registration_id",
                        column: x => x.registration_id,
                        principalTable: "registrations",
                        principalColumn: "registration_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_days_event_id",
                table: "days",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "IX_days_menu_id",
                table: "days",
                column: "menu_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_events_location_id",
                table: "events",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "IX_feedbacks_RegistrationId",
                table: "feedbacks",
                column: "RegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_menu_items_item_id",
                table: "menu_items",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_participation_registration_id",
                table: "participation",
                column: "registration_id");

            migrationBuilder.CreateIndex(
                name: "IX_registrations_event_user",
                table: "registrations",
                columns: new[] { "event_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_registrations_user_id",
                table: "registrations",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_phone",
                table: "users",
                column: "phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_role",
                table: "users",
                column: "role");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "feedbacks");

            migrationBuilder.DropTable(
                name: "menu_items");

            migrationBuilder.DropTable(
                name: "participation");

            migrationBuilder.DropTable(
                name: "items");

            migrationBuilder.DropTable(
                name: "days");

            migrationBuilder.DropTable(
                name: "registrations");

            migrationBuilder.DropTable(
                name: "menu");

            migrationBuilder.DropTable(
                name: "events");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "locations");
        }
    }
}

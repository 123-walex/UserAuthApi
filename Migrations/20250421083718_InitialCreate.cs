using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace User_Authapi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RestoredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "DeletedAt", "Email", "IsDeleted", "Password", "RestoredAt", "UpdatedAt", "UserName" },
                values: new object[,]
                {
                    { -3, new DateTime(2024, 4, 19, 12, 0, 0, 0, DateTimeKind.Utc), null, "elizabeth@gmail.com", false, "AQAAAAIAAYagAAAAENcXqWZsR2qRhf1O5H1jdfbwPQkY2u7S93z5WMr9ixgKeB3l0KkJ4Xb3KmUjDLQNDg==", null, null, "elizabeth fagbemi" },
                    { -2, new DateTime(2024, 4, 19, 12, 0, 0, 0, DateTimeKind.Utc), null, "moji@gmail.com", false, "AQAAAAIAAYagAAAAEM1eECxq3JHBoRxHJdSe+LRfkpRdn0+wxlmAGXxjP6u4bqpN28TmU7DD2chPb6heqA==", null, null, "moji fagbemi" },
                    { -1, new DateTime(2024, 4, 19, 12, 0, 0, 0, DateTimeKind.Utc), null, "wale@gmail.com", false, "AQAAAAIAAYagAAAAELZsXLwYZc74iPl1YpZq9E31HbU1M8HBIUX1p5rfeM2MbYxq+vJXqXfsBQcOq1bYlw==", null, null, "wale fagbemi" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}

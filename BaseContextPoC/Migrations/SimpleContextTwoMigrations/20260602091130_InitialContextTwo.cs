using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseContextPoC.Migrations.SimpleContextTwoMigrations
{
    /// <inheritdoc />
    public partial class InitialContextTwo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Custom");

            migrationBuilder.CreateTable(
                name: "SimpleEntityTwo",
                schema: "Custom",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimpleEntityTwo", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SimpleEntityTwo",
                schema: "Custom");
        }
    }
}

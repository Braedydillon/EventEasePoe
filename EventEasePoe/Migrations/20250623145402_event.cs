using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventEasePoe.Migrations
{
    /// <inheritdoc />
    public partial class @event : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EventSpecificInfo",
                table: "Event",
                newName: "EventType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EventType",
                table: "Event",
                newName: "EventSpecificInfo");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskBoard.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDeletedToColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Columns",
                type: "bit",
                nullable: true,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Columns",
                type: "bit",
                nullable: true,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DeletedOn",
                table: "Columns",
                type: "datetimeoffset",
                nullable: true,
                defaultValue: false
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                    name: "IsDeleted",
                    table: "Columns",
                    schema: "dbo");
            migrationBuilder.DropColumn(
                    name: "IsArchived",
                    table: "Columns",
                    schema: "dbo");
            migrationBuilder.DropColumn(
                    name: "DeletedOn",
                    table: "Columns",
                    schema: "dbo");
        }
    }
}

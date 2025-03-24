using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Research_Assistant.Migrations
{
    /// <inheritdoc />
    public partial class AddExtractedTextToPapers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExtractedText",
                table: "Papers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtractedText",
                table: "Papers");
        }
    }
}

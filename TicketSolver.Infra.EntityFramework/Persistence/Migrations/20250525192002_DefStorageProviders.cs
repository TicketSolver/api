using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TicketSolver.Infra.EntityFramework.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DefStorageProviders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "Attachments",
                newName: "UserId");

            migrationBuilder.AddColumn<short>(
                name: "DefStorageProviderId",
                table: "Attachments",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<decimal>(
                name: "FileSize",
                table: "Attachments",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Key",
                table: "Attachments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "Attachments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "DefStorageProviders",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefStorageProviders", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_DefStorageProviderId",
                table: "Attachments",
                column: "DefStorageProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_UserId",
                table: "Attachments",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_DefStorageProviders_DefStorageProviderId",
                table: "Attachments",
                column: "DefStorageProviderId",
                principalTable: "DefStorageProviders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_IdentityUsers_UserId",
                table: "Attachments",
                column: "UserId",
                principalTable: "IdentityUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_DefStorageProviders_DefStorageProviderId",
                table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_IdentityUsers_UserId",
                table: "Attachments");

            migrationBuilder.DropTable(
                name: "DefStorageProviders");

            migrationBuilder.DropIndex(
                name: "IX_Attachments_DefStorageProviderId",
                table: "Attachments");

            migrationBuilder.DropIndex(
                name: "IX_Attachments_UserId",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "DefStorageProviderId",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "Key",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "Attachments");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Attachments",
                newName: "FilePath");
        }
    }
}

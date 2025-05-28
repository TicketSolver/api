using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TicketSolver.Infra.EntityFramework.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TechStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FirstResponseAt",
                table: "TicketUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "DefUserSatisfactionId",
                table: "Tickets",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.CreateTable(
                name: "DefUserSatisfaction",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefUserSatisfaction", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_DefUserSatisfactionId",
                table: "Tickets",
                column: "DefUserSatisfactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_DefUserSatisfaction_DefUserSatisfactionId",
                table: "Tickets",
                column: "DefUserSatisfactionId",
                principalTable: "DefUserSatisfaction",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_DefUserSatisfaction_DefUserSatisfactionId",
                table: "Tickets");

            migrationBuilder.DropTable(
                name: "DefUserSatisfaction");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_DefUserSatisfactionId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "FirstResponseAt",
                table: "TicketUsers");

            migrationBuilder.DropColumn(
                name: "DefUserSatisfactionId",
                table: "Tickets");
        }
    }
}

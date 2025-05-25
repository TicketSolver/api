using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TicketSolver.Infra.EntityFramework.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTickets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_IdentityUsers_AssignedToId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_AssignedToId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "AssignedToId",
                table: "Tickets");

            migrationBuilder.RenameColumn(
                name: "Priority",
                table: "Tickets",
                newName: "DefTicketPriorityId");

            migrationBuilder.RenameColumn(
                name: "Category",
                table: "Tickets",
                newName: "DefTicketCategoryId");

            migrationBuilder.CreateTable(
                name: "Chat",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TicketId = table.Column<int>(type: "integer", nullable: false),
                    ChatHistory = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "[]"),
                    TotalMessages = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LastMessageAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketChats_Tickets",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_DefTicketCategoryId",
                table: "Tickets",
                column: "DefTicketCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_DefTicketPriorityId",
                table: "Tickets",
                column: "DefTicketPriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketChats_ChatHistory_Gin",
                table: "Chat",
                column: "ChatHistory")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_TicketChats_LastMessageAt",
                table: "Chat",
                column: "LastMessageAt");

            migrationBuilder.CreateIndex(
                name: "IX_TicketChats_TicketId",
                table: "Chat",
                column: "TicketId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_DefTicketCategories_DefTicketCategoryId",
                table: "Tickets",
                column: "DefTicketCategoryId",
                principalTable: "DefTicketCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_DefTicketPriorities_DefTicketPriorityId",
                table: "Tickets",
                column: "DefTicketPriorityId",
                principalTable: "DefTicketPriorities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_DefTicketCategories_DefTicketCategoryId",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_DefTicketPriorities_DefTicketPriorityId",
                table: "Tickets");

            migrationBuilder.DropTable(
                name: "Chat");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_DefTicketCategoryId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_DefTicketPriorityId",
                table: "Tickets");

            migrationBuilder.RenameColumn(
                name: "DefTicketPriorityId",
                table: "Tickets",
                newName: "Priority");

            migrationBuilder.RenameColumn(
                name: "DefTicketCategoryId",
                table: "Tickets",
                newName: "Category");

            migrationBuilder.AddColumn<string>(
                name: "AssignedToId",
                table: "Tickets",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_AssignedToId",
                table: "Tickets",
                column: "AssignedToId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_IdentityUsers_AssignedToId",
                table: "Tickets",
                column: "AssignedToId",
                principalTable: "IdentityUsers",
                principalColumn: "Id");
        }
    }
}

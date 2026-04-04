using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GastoSmart.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIdempotencyKeyToTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "IdempotencyKey",
                table: "Transactions",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                table: "Transactions");
        }
    }
}

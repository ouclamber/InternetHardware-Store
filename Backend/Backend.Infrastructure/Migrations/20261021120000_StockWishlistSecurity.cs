using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <summary>
    /// Склад (остатки товаров), избранное, метка безопасности пользователей
    /// и зашифрованные контакты покупателя в заказах.
    /// </summary>
    public partial class StockWishlistSecurity : Migration
    {
        /// <summary>Остаток, который получают уже существующие товары.</summary>
        private const int InitialStockForExistingProducts = 10;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Товары: остаток на складе ─────────────────────────────
            migrationBuilder.AddColumn<int>(
                name: "StockQuantity",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Существующие товары не должны внезапно стать «нет в наличии»
            migrationBuilder.Sql(
                $@"UPDATE ""Products"" SET ""StockQuantity"" = {InitialStockForExistingProducts};");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Category_Active_Price",
                table: "Products",
                columns: new[] { "CategoryId", "IsActive", "Price" });

            // ── Пользователи: метка безопасности для отзыва JWT ───────
            migrationBuilder.AddColumn<string>(
                name: "SecurityStamp",
                table: "Users",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            // У каждого существующего пользователя — своя случайная метка
            // (старые токены без метки после обновления перестанут приниматься — нужен повторный вход)
            migrationBuilder.Sql(
                @"UPDATE ""Users"" SET ""SecurityStamp"" = replace(gen_random_uuid()::text, '-', '');");

            // ── Заказы: контакты покупателя и флаг резерва склада ─────
            migrationBuilder.AddColumn<string>(
                name: "CustomerContact",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "StockReserved",
                table: "Orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // ── Избранное ─────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "WishlistItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WishlistItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WishlistItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WishlistItems_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WishlistItems_ProductId",
                table: "WishlistItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_WishlistItems_User_Product",
                table: "WishlistItems",
                columns: new[] { "UserId", "ProductId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WishlistItems");

            migrationBuilder.DropIndex(
                name: "IX_Products_Category_Active_Price",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "StockQuantity",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SecurityStamp",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CustomerContact",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "StockReserved",
                table: "Orders");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialWithRealCart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Baskets_Cart_CartId1",
                table: "Baskets");

            migrationBuilder.DropForeignKey(
                name: "FK_Baskets_Products_ProductId",
                table: "Baskets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Cart",
                table: "Cart");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Baskets",
                table: "Baskets");

            migrationBuilder.DropIndex(
                name: "IX_Baskets_CartId1",
                table: "Baskets");

            migrationBuilder.DropColumn(
                name: "CartId1",
                table: "Baskets");

            migrationBuilder.RenameTable(
                name: "Cart",
                newName: "Carts");

            migrationBuilder.RenameTable(
                name: "Baskets",
                newName: "CartItems");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "CartItems",
                newName: "CartId");

            migrationBuilder.RenameIndex(
                name: "IX_Baskets_UserId_ProductId",
                table: "CartItems",
                newName: "IX_CartItems_Cart_Product");

            migrationBuilder.RenameIndex(
                name: "IX_Baskets_ProductId",
                table: "CartItems",
                newName: "IX_CartItems_ProductId");

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "CartItems",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Carts",
                table: "Carts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CartItems",
                table: "CartItems",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_UserId",
                table: "Carts",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Carts_CartId",
                table: "CartItems",
                column: "CartId",
                principalTable: "Carts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Products_ProductId",
                table: "CartItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Carts_CartId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Products_ProductId",
                table: "CartItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Carts",
                table: "Carts");

            migrationBuilder.DropIndex(
                name: "IX_Carts_UserId",
                table: "Carts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CartItems",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "CartItems");

            migrationBuilder.RenameTable(
                name: "Carts",
                newName: "Cart");

            migrationBuilder.RenameTable(
                name: "CartItems",
                newName: "Baskets");

            migrationBuilder.RenameColumn(
                name: "CartId",
                table: "Baskets",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_CartItems_ProductId",
                table: "Baskets",
                newName: "IX_Baskets_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_CartItems_Cart_Product",
                table: "Baskets",
                newName: "IX_Baskets_UserId_ProductId");

            migrationBuilder.AddColumn<int>(
                name: "CartId1",
                table: "Baskets",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Cart",
                table: "Cart",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Baskets",
                table: "Baskets",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Baskets_CartId1",
                table: "Baskets",
                column: "CartId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Baskets_Cart_CartId1",
                table: "Baskets",
                column: "CartId1",
                principalTable: "Cart",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Baskets_Products_ProductId",
                table: "Baskets",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

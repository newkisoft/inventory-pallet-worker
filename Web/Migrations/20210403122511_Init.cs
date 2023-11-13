using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Web.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
          
    
            migrationBuilder.CreateTable(
                name: "Pallet",
                columns: table => new
                {
                    PalletId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PalletNumber = table.Column<string>(type: "text", nullable: true),
                    Barcode = table.Column<string>(type: "text", nullable: true),
                    YarnType = table.Column<string>(type: "text", nullable: true),
                    Denier = table.Column<int>(type: "integer", nullable: false),
                    Filament = table.Column<int>(type: "integer", nullable: false),
                    Lustre = table.Column<string>(type: "text", nullable: true),
                    Intermingle = table.Column<string>(type: "text", nullable: true),
                    Color = table.Column<string>(type: "text", nullable: true),
                    ColorCode = table.Column<string>(type: "text", nullable: true),
                    BobbinSize = table.Column<string>(type: "text", nullable: true),
                    PalletName = table.Column<string>(type: "text", nullable: true),
                    Weight = table.Column<double>(type: "double precision", nullable: false),
                    Price = table.Column<double>(type: "double precision", nullable: false),
                    RemainWeight = table.Column<double>(type: "double precision", nullable: false),
                    Sold = table.Column<bool>(type: "boolean", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    Lot = table.Column<string>(type: "text", nullable: true),
                    Warehouse = table.Column<string>(type: "text", nullable: true),
                    Details = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Image = table.Column<string>(type: "text", nullable: true),
                    ThumbnailImage = table.Column<string>(type: "text", nullable: true),
                    NumberOfItems = table.Column<int>(type: "integer", nullable: false),
                    RemainingItems = table.Column<int>(type: "integer", nullable: false),
                    IsOnlineProduct = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelivered = table.Column<bool>(type: "boolean", nullable: false),
                    DeliveryDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pallets", x => x.PalletId);
                });

            
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
           
           

            migrationBuilder.DropTable(
                name: "Pallets");

   
          
        }
    }
}

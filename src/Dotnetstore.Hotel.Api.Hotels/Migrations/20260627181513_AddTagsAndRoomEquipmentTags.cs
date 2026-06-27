using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetstore.Hotel.Api.Hotels.Migrations
{
    /// <inheritdoc />
    public partial class AddTagsAndRoomEquipmentTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_room_equipment",
                table: "room_equipment");

            migrationBuilder.DropIndex(
                name: "IX_room_equipment_RoomId",
                table: "room_equipment");

            migrationBuilder.AddPrimaryKey(
                name: "PK_room_equipment",
                table: "room_equipment",
                columns: new[] { "RoomId", "EquipmentId" });

            migrationBuilder.CreateTable(
                name: "tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "room_equipment_tags",
                columns: table => new
                {
                    TagsId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomEquipmentRoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomEquipmentEquipmentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_room_equipment_tags", x => new { x.TagsId, x.RoomEquipmentRoomId, x.RoomEquipmentEquipmentId });
                    table.ForeignKey(
                        name: "FK_room_equipment_tags_room_equipment_RoomEquipmentRoomId_Room~",
                        columns: x => new { x.RoomEquipmentRoomId, x.RoomEquipmentEquipmentId },
                        principalTable: "room_equipment",
                        principalColumns: new[] { "RoomId", "EquipmentId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_room_equipment_tags_tags_TagsId",
                        column: x => x.TagsId,
                        principalTable: "tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_room_equipment_EquipmentId",
                table: "room_equipment",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_room_equipment_tags_RoomEquipmentRoomId_RoomEquipmentEquipm~",
                table: "room_equipment_tags",
                columns: new[] { "RoomEquipmentRoomId", "RoomEquipmentEquipmentId" });

            migrationBuilder.CreateIndex(
                name: "IX_tags_Name",
                table: "tags",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "room_equipment_tags");

            migrationBuilder.DropTable(
                name: "tags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_room_equipment",
                table: "room_equipment");

            migrationBuilder.DropIndex(
                name: "IX_room_equipment_EquipmentId",
                table: "room_equipment");

            migrationBuilder.AddPrimaryKey(
                name: "PK_room_equipment",
                table: "room_equipment",
                columns: new[] { "EquipmentId", "RoomId" });

            migrationBuilder.CreateIndex(
                name: "IX_room_equipment_RoomId",
                table: "room_equipment",
                column: "RoomId");
        }
    }
}

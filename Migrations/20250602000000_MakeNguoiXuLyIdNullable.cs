using Microsoft.EntityFrameworkCore.Migrations;

#nullable enable

namespace QuanLyKhuDanCu.Migrations
{
    public partial class MakeNguoiXuLyIdNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YeuCauDichVus_AspNetUsers_NguoiXuLyId",
                table: "YeuCauDichVus");

            migrationBuilder.AlterColumn<string>(
                name: "NguoiXuLyId",
                table: "YeuCauDichVus",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)");

            migrationBuilder.AddForeignKey(
                name: "FK_YeuCauDichVus_AspNetUsers_NguoiXuLyId",
                table: "YeuCauDichVus",
                column: "NguoiXuLyId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YeuCauDichVus_AspNetUsers_NguoiXuLyId",
                table: "YeuCauDichVus");

            migrationBuilder.AlterColumn<string>(
                name: "NguoiXuLyId",
                table: "YeuCauDichVus",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_YeuCauDichVus_AspNetUsers_NguoiXuLyId",
                table: "YeuCauDichVus",
                column: "NguoiXuLyId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

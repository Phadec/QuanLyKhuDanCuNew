using Microsoft.EntityFrameworkCore.Migrations;

#nullable enable

namespace QuanLyKhuDanCu.Migrations
{
    public partial class MakeYeuCauDichVuNguoiXuLyIdNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "NguoiXuLyId",
                table: "YeuCauDichVus",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "NguoiXuLyId",
                table: "YeuCauDichVus",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true);
        }
    }
}

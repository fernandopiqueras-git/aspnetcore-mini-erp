using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MiniErp.Data;

#nullable disable

namespace MiniErp.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260917213000_AddIdentityAndAudit")]
public partial class AddIdentityAndAudit : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable("AspNetRoles", table => new
        {
            Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
            Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
        }, constraints: table => table.PrimaryKey("PK_AspNetRoles", x => x.Id));

        migrationBuilder.CreateTable("AspNetUsers", table => new
        {
            Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
            UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
            PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
            SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
            ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
            PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
            PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
            TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
            LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
            LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
            AccessFailedCount = table.Column<int>(type: "int", nullable: false)
        }, constraints: table => table.PrimaryKey("PK_AspNetUsers", x => x.Id));

        migrationBuilder.CreateTable("AuditEntries", table => new
        {
            Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
            UserName = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
            Action = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
            EntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
        }, constraints: table => table.PrimaryKey("PK_AuditEntries", x => x.Id));

        migrationBuilder.CreateTable("AspNetRoleClaims", table => new
        {
            Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
            RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
            ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
            ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
            table.ForeignKey("FK_AspNetRoleClaims_AspNetRoles_RoleId", x => x.RoleId, "AspNetRoles", "Id", onDelete: ReferentialAction.Cascade);
        });

        migrationBuilder.CreateTable("AspNetUserClaims", table => new
        {
            Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
            UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
            ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
            ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
            table.ForeignKey("FK_AspNetUserClaims_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id", onDelete: ReferentialAction.Cascade);
        });

        migrationBuilder.CreateTable("AspNetUserLogins", table => new
        {
            LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
            ProviderKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
            ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
            UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
            table.ForeignKey("FK_AspNetUserLogins_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id", onDelete: ReferentialAction.Cascade);
        });

        migrationBuilder.CreateTable("AspNetUserRoles", table => new
        {
            UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
            RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
            table.ForeignKey("FK_AspNetUserRoles_AspNetRoles_RoleId", x => x.RoleId, "AspNetRoles", "Id", onDelete: ReferentialAction.Cascade);
            table.ForeignKey("FK_AspNetUserRoles_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id", onDelete: ReferentialAction.Cascade);
        });

        migrationBuilder.CreateTable("AspNetUserTokens", table => new
        {
            UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
            LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
            Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
            Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
            table.ForeignKey("FK_AspNetUserTokens_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id", onDelete: ReferentialAction.Cascade);
        });

        migrationBuilder.CreateIndex("IX_AspNetRoleClaims_RoleId", "AspNetRoleClaims", "RoleId");
        migrationBuilder.CreateIndex("RoleNameIndex", "AspNetRoles", "NormalizedName", unique: true, filter: "[NormalizedName] IS NOT NULL");
        migrationBuilder.CreateIndex("IX_AspNetUserClaims_UserId", "AspNetUserClaims", "UserId");
        migrationBuilder.CreateIndex("IX_AspNetUserLogins_UserId", "AspNetUserLogins", "UserId");
        migrationBuilder.CreateIndex("IX_AspNetUserRoles_RoleId", "AspNetUserRoles", "RoleId");
        migrationBuilder.CreateIndex("EmailIndex", "AspNetUsers", "NormalizedEmail");
        migrationBuilder.CreateIndex("UserNameIndex", "AspNetUsers", "NormalizedUserName", unique: true, filter: "[NormalizedUserName] IS NOT NULL");
        migrationBuilder.CreateIndex("IX_AuditEntries_Timestamp", "AuditEntries", "Timestamp");
        migrationBuilder.CreateIndex("IX_AuditEntries_UserName", "AuditEntries", "UserName");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("AspNetRoleClaims");
        migrationBuilder.DropTable("AspNetUserClaims");
        migrationBuilder.DropTable("AspNetUserLogins");
        migrationBuilder.DropTable("AspNetUserRoles");
        migrationBuilder.DropTable("AspNetUserTokens");
        migrationBuilder.DropTable("AuditEntries");
        migrationBuilder.DropTable("AspNetRoles");
        migrationBuilder.DropTable("AspNetUsers");
    }
}

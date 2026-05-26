using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    /// <inheritdoc />
    public partial class AddPessoaToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Temporariamente desabilita verificação de chaves estrangeiras para permitir alterações seguras
            migrationBuilder.Sql("SET FOREIGN_KEY_CHECKS = 0;");

            // Remover índices antigos se existirem (silencioso)
            migrationBuilder.Sql("DROP INDEX IF EXISTS `IX_UserRole_user_id` ON `UserRole`;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS `IX_RolePermission_role_id_permission_id` ON `RolePermission`;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS `IX_RolePermission_role_id` ON `RolePermission`;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS `IX_User_email` ON `User`;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS `IX_User_created_at` ON `User`;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS `IX_User_is_active` ON `User`;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS `IX_UserRole_user_id_role_id` ON `UserRole`;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS `IX_Role_is_active` ON `Role`;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS `IX_Role_name` ON `Role`;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS `IX_Permission_action` ON `Permission`;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS `IX_Permission_is_active` ON `Permission`;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS `IX_Permission_resource` ON `Permission`;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS `IX_Permission_name` ON `Permission`;");
            
                

            // Reabilita verificação de chaves estrangeiras
            migrationBuilder.Sql("SET FOREIGN_KEY_CHECKS = 1;");

            // Adiciona a coluna como nullable para evitar falha ao criar FK quando valores inválidos existirem
            migrationBuilder.AddColumn<int>(
                name: "PessoaID",
                table: "User",
                type: "int",
                nullable: true);

            // Se existirem valores em User.PessoaID que não correspondem a Pessoa.Id, substituí-los por NULL
            // (evita erro "Cannot add or update a child row" ao criar a FK)
            migrationBuilder.Sql(@"
                UPDATE `User` u
                LEFT JOIN `Pessoa` p ON u.PessoaID = p.Id
                SET u.PessoaID = NULL
                WHERE u.PessoaID IS NOT NULL AND p.Id IS NULL;
            ");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "Role",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_user_id_role_id",
                table: "UserRole",
                columns: new[] { "user_id", "role_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_created_at",
                table: "User",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_User_email",
                table: "User",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_is_active",
                table: "User",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_User_PessoaID",
                table: "User",
                column: "PessoaID");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermission_role_id_permission_id",
                table: "RolePermission",
                columns: new[] { "role_id", "permission_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Role_is_active",
                table: "Role",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_Role_name",
                table: "Role",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permission_action",
                table: "Permission",
                column: "action");

            migrationBuilder.CreateIndex(
                name: "IX_Permission_is_active",
                table: "Permission",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_Permission_name",
                table: "Permission",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permission_resource",
                table: "Permission",
                column: "resource");

            // Cria a FK permitindo NULLs (ON DELETE SET NULL), assim não falha se houver User sem Pessoa
            migrationBuilder.AddForeignKey(
                name: "FK_User_Pessoa_PessoaID",
                table: "User",
                column: "PessoaID",
                principalTable: "Pessoa",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Pessoa_PessoaID",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_UserRole_user_id_role_id",
                table: "UserRole");

            migrationBuilder.DropIndex(
                name: "IX_User_created_at",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_email",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_is_active",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_PessoaID",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_RolePermission_role_id_permission_id",
                table: "RolePermission");

            migrationBuilder.DropIndex(
                name: "IX_Role_is_active",
                table: "Role");

            migrationBuilder.DropIndex(
                name: "IX_Role_name",
                table: "Role");

            migrationBuilder.DropIndex(
                name: "IX_Permission_action",
                table: "Permission");

            migrationBuilder.DropIndex(
                name: "IX_Permission_is_active",
                table: "Permission");

            migrationBuilder.DropIndex(
                name: "IX_Permission_name",
                table: "Permission");

            migrationBuilder.DropIndex(
                name: "IX_Permission_resource",
                table: "Permission");

            migrationBuilder.DropColumn(
                name: "PessoaID",
                table: "User");

            migrationBuilder.AlterColumn<int>(
                name: "name",
                table: "Role",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PermissionRole",
                columns: table => new
                {
                    PermissionsId = table.Column<int>(type: "int", nullable: false),
                    RolesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    migrationBuilder.CreateIndex(
                        name: "IX_PermissionRole_RolesId",
                        table: "PermissionRole",
                        column: "RolesId");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RoleUser",
                columns: table => new
                {
                    RolesId = table.Column<int>(type: "int", nullable: false),
                    UsersId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    migrationBuilder.CreateIndex(
                        name: "IX_RoleUser_UsersId",
                        table: "RoleUser",
                        column: "UsersId");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_user_id",
                table: "UserRole",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermission_role_id",
                table: "RolePermission",
                column: "role_id");
        }
    }
}
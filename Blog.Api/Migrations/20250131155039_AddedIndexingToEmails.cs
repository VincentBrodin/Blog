using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogVb.Api.Migrations {
	/// <inheritdoc />
	public partial class AddedIndexingToEmails : Migration {
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder) {
			migrationBuilder.CreateIndex(
				name: "IX_Accounts_Email",
				table: "Accounts",
				column: "Email");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder) {
			migrationBuilder.DropIndex(
				name: "IX_Accounts_Email",
				table: "Accounts");
		}
	}
}

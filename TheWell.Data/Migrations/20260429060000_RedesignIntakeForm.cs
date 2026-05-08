using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheWell.Data.Migrations
{
    [Migration("20260429060000_RedesignIntakeForm")]
    public partial class RedesignIntakeForm : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""IntakeQuestions""
                    DROP COLUMN IF EXISTS ""Q1_Response"",
                    DROP COLUMN IF EXISTS ""Q2_Response"",
                    DROP COLUMN IF EXISTS ""Q3_Response"";

                ALTER TABLE ""IntakeQuestions""
                    ADD COLUMN IF NOT EXISTS ""MyHabit""               text NOT NULL DEFAULT '',
                    ADD COLUMN IF NOT EXISTS ""MyGoal""                text NOT NULL DEFAULT '',
                    ADD COLUMN IF NOT EXISTS ""IAmPersonWho""          text NOT NULL DEFAULT '',
                    ADD COLUMN IF NOT EXISTS ""Strategy1""             text NOT NULL DEFAULT '',
                    ADD COLUMN IF NOT EXISTS ""Strategy2""             text NOT NULL DEFAULT '',
                    ADD COLUMN IF NOT EXISTS ""ToImproveMyselfIWill""  text NOT NULL DEFAULT '',
                    ADD COLUMN IF NOT EXISTS ""RewardMyselfWith""      text NOT NULL DEFAULT '',
                    ADD COLUMN IF NOT EXISTS ""PeopleForEncouragement"" text NOT NULL DEFAULT '';
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "MyHabit",               table: "IntakeQuestions");
            migrationBuilder.DropColumn(name: "MyGoal",                table: "IntakeQuestions");
            migrationBuilder.DropColumn(name: "IAmPersonWho",          table: "IntakeQuestions");
            migrationBuilder.DropColumn(name: "Strategy1",             table: "IntakeQuestions");
            migrationBuilder.DropColumn(name: "Strategy2",             table: "IntakeQuestions");
            migrationBuilder.DropColumn(name: "ToImproveMyselfIWill",  table: "IntakeQuestions");
            migrationBuilder.DropColumn(name: "RewardMyselfWith",      table: "IntakeQuestions");
            migrationBuilder.DropColumn(name: "PeopleForEncouragement","IntakeQuestions");

            migrationBuilder.AddColumn<string>("Q1_Response", "IntakeQuestions", nullable: false, defaultValue: "");
            migrationBuilder.AddColumn<string>("Q2_Response", "IntakeQuestions", nullable: false, defaultValue: "");
            migrationBuilder.AddColumn<string>("Q3_Response", "IntakeQuestions", nullable: false, defaultValue: "");
        }
    }
}

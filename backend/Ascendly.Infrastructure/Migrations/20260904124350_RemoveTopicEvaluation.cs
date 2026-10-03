using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ascendly.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTopicEvaluation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuestionEvaluations_TopicEvaluations_TopicEvaluationId",
                table: "QuestionEvaluations");

            migrationBuilder.DropTable(
                name: "TopicEvaluations");

            migrationBuilder.DropIndex(
                name: "IX_QuestionEvaluations_TopicEvaluationId",
                table: "QuestionEvaluations");

            migrationBuilder.DropColumn(
                name: "TopicEvaluationId",
                table: "QuestionEvaluations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TopicEvaluationId",
                table: "QuestionEvaluations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "TopicEvaluations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InterviewTopicId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvaluatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopicEvaluations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TopicEvaluations_InterviewTopics_InterviewTopicId",
                        column: x => x.InterviewTopicId,
                        principalTable: "InterviewTopics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionEvaluations_TopicEvaluationId",
                table: "QuestionEvaluations",
                column: "TopicEvaluationId");

            migrationBuilder.CreateIndex(
                name: "IX_TopicEvaluations_InterviewTopicId",
                table: "TopicEvaluations",
                column: "InterviewTopicId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionEvaluations_TopicEvaluations_TopicEvaluationId",
                table: "QuestionEvaluations",
                column: "TopicEvaluationId",
                principalTable: "TopicEvaluations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

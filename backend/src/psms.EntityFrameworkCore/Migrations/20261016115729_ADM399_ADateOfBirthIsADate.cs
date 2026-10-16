using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace psms.Migrations
{
    /// <summary>
    /// A date of birth is a date, not a moment.
    /// <para>
    /// Stored as a timestamp, 14 March 2015 became <c>2015-03-13T22:00:00Z</c> —
    /// the right instant for midnight in Johannesburg, and the wrong date to
    /// anything formatting in UTC. The same learner read as born on the 14th on
    /// the apply form and the 13th on the workflow summary, and an assessment
    /// set for Monday the 26th showed as the 25th on the screen the principal
    /// decides from.
    /// </para>
    /// <para>
    /// <b>The conversion has to be done by hand.</b> The scaffolded
    /// <c>AlterColumn</c> becomes a plain <c>TYPE date</c>, which Postgres
    /// casts in the <i>session</i> time zone — UTC on the server — so every
    /// stored value would have lost a day on the way across and the fault would
    /// have been baked in permanently rather than fixed. Converting through
    /// Africa/Johannesburg recovers the date that was actually entered.
    /// </para>
    /// </summary>
    public partial class ADM399_ADateOfBirthIsADate : Migration
    {
        private const string Zone = "Africa/Johannesburg";

        private static readonly (string Table, string Column)[] Columns =
        {
            ("Applications", "DateOfBirth"),
            ("Students", "DateOfBirth"),
            ("AdmissionInterviews", "ScheduledDate"),
            ("AdmissionAssessments", "ScheduledDate"),
        };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var (table, column) in Columns)
            {
                migrationBuilder.Sql($@"
                    ALTER TABLE ""{table}""
                    ALTER COLUMN ""{column}"" TYPE date
                    USING ((""{column}"" AT TIME ZONE '{Zone}')::date);");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Back to the instant it used to be: midnight of that date, in
            // Johannesburg, which is what was there before.
            foreach (var (table, column) in Columns)
            {
                migrationBuilder.Sql($@"
                    ALTER TABLE ""{table}""
                    ALTER COLUMN ""{column}"" TYPE timestamp with time zone
                    USING ((""{column}""::timestamp) AT TIME ZONE '{Zone}');");
            }
        }
    }
}

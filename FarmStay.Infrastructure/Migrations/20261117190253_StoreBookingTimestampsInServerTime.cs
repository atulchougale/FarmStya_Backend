using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmStay.Infrastructure.Migrations
{
    public partial class StoreBookingTimestampsInServerTime : Migration
    {
        private static readonly (string Table, string[] Columns)[] Tables =
        [
            ("Bookings", ["CreatedDate", "ModifiedDate", "OwnerDeadlineUtc", "ApprovedAtUtc", "CustomerDeadlineUtc"]),
            ("BookingDateReservations", ["CreatedDateUtc", "ExpiresAtUtc"]),
            ("BookingStayDates", ["CreatedDate", "ModifiedDate"]),
            ("BookingEventDates", ["CreatedDate", "ModifiedDate"]),
            ("BookingStatusHistory", ["CreatedDate"]),
            ("BookingDocuments", ["CreatedDate", "ModifiedDate", "VerifiedAtUtc", "ReuploadDeadlineUtc"]),
            ("PaymentTransactions", ["CreatedDate", "ModifiedDate", "PaidAtUtc", "VerifiedAtUtc"]),
            ("BookingReceipts", ["CreatedDate", "IssuedAtUtc"]),
            ("BookingActionTokens", ["CreatedDate", "ExpiresAtUtc", "ConsumedAtUtc"]),
            ("NotificationOutbox", ["CreatedDate", "NextAttemptAtUtc", "SentAtUtc"])
        ];

        private static readonly (string Table, string OldName, string NewName)[] Indexes =
        [
            ("NotificationOutbox", "IX_NotificationOutbox_Status_NextAttemptAtUtc_CreatedDate", "IX_NotificationOutbox_Status_NextAttemptAt_CreatedDate"),
            ("BookingDateReservations", "IX_BookingDateReservations_FarmHouseId_ReservationDate_ExpiresAtUtc", "IX_BookingDateReservations_FarmHouseId_ReservationDate_ExpiresAt"),
            ("BookingActionTokens", "IX_BookingActionTokens_ExpiresAtUtc_ConsumedAtUtc", "IX_BookingActionTokens_ExpiresAt_ConsumedAt")
        ];

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            RequireIndiaServerTime();
            foreach (var (table, columns) in Tables)
            {
                Shift(migrationBuilder, table, columns, 330);
                foreach (var column in columns.Where(column => column.EndsWith("Utc", StringComparison.Ordinal)))
                    migrationBuilder.RenameColumn(column, table, column[..^3]);
            }
            foreach (var (table, oldName, newName) in Indexes)
                migrationBuilder.RenameIndex(oldName, newName, table);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var (table, columns) in Tables)
            {
                foreach (var column in columns.Where(column => column.EndsWith("Utc", StringComparison.Ordinal)))
                    migrationBuilder.RenameColumn(column[..^3], table, column);
                Shift(migrationBuilder, table, columns, -330);
            }
            foreach (var (table, oldName, newName) in Indexes)
                migrationBuilder.RenameIndex(newName, oldName, table);
        }

        private static void RequireIndiaServerTime()
        {
            if (TimeZoneInfo.Local.BaseUtcOffset != TimeSpan.FromMinutes(330) || TimeZoneInfo.Local.SupportsDaylightSavingTime)
                throw new InvalidOperationException("Run this migration on the FarmStay server configured for India Standard Time.");
        }

        private static void Shift(MigrationBuilder migrationBuilder, string table, string[] columns, int minutes)
        {
            var assignments = columns.Select(column => FormattableString.Invariant(
                $"[{column}] = CASE WHEN [{column}] >= '1900-01-02' AND [{column}] < '9999-12-30' THEN DATEADD(minute, {minutes}, [{column}]) ELSE [{column}] END"));
            migrationBuilder.Sql($"UPDATE [dbo].[{table}] SET {string.Join(", ", assignments)};");
        }
    }
}

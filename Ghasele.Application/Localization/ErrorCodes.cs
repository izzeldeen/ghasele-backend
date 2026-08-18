namespace Ghasele.Application.Localization
{
    /// <summary>
    /// Stable, language-independent identifiers for every message the API returns to a client.
    /// The wire response carries the code alongside the localized text, so clients may render
    /// their own translation instead of the server's without needing a backend change.
    /// </summary>
    public static class ErrorCodes
    {
        // Common
        public const string InternalError = "common.internal_error";
        public const string InvalidToken = "common.invalid_token";

        // Auth
        public const string PhoneAlreadyExists = "auth.phone_already_exists";
        public const string InvalidCredentials = "auth.invalid_credentials";
        public const string AppleTokenMissing = "auth.apple_token_missing";
        public const string AppleTokenNoSubject = "auth.apple_token_no_subject";
        public const string AppleTokenInvalid = "auth.apple_token_invalid";
        public const string PhoneNotRegistered = "auth.phone_not_registered";
        public const string AccountNotFound = "auth.account_not_found";
        public const string UserNotFound = "auth.user_not_found";
        public const string PhoneAlreadyVerified = "auth.phone_already_verified";
        public const string OtpInvalidOrExpired = "auth.otp_invalid_or_expired";
        public const string PhoneVerified = "auth.phone_verified";
        public const string OtpSent = "auth.otp_sent";
        public const string OtpVerified = "auth.otp_verified";
        public const string PasswordReset = "auth.password_reset";
        public const string FcmTokenUpdated = "auth.fcm_token_updated";

        // Orders
        public const string OrderNotFound = "order.not_found";
        public const string OrderPendingExists = "order.pending_exists";
        public const string OrderNotPartOfTrip = "order.not_part_of_trip";
        public const string OrderDeleted = "order.deleted";

        // Trips
        public const string TripNotFound = "trip.not_found";
        public const string TripCleanerAndDriverRequired = "trip.cleaner_and_driver_required";
        public const string TripOrdersRequired = "trip.orders_required";
        public const string TripOrdersOutOfSequence = "trip.orders_out_of_sequence";
        public const string TripMustBeCollected = "trip.must_be_collected";

        /// <summary>Takes the order reference number as <c>{0}</c>.</summary>
        public const string TripOrderAlreadyAssigned = "trip.order_already_assigned";

        // Cleaners
        public const string CleanerNotFound = "cleaner.not_found";
        public const string CleanerDeleted = "cleaner.deleted";

        // Drivers
        public const string DriverNotFound = "driver.not_found";
        public const string DriverDeleted = "driver.deleted";

        // Item types
        public const string ItemTypeNotFound = "item_type.not_found";

        // Marketing codes
        public const string MarketingCodeNotFound = "marketing_code.not_found";
        public const string MarketingCodeInvalidOrInactive = "marketing_code.invalid_or_inactive";
        public const string MarketingCodeDeleted = "marketing_code.deleted";

        // User locations
        public const string LocationDeleted = "location.deleted";

        // Maintenance
        public const string PurgeComplete = "maintenance.purge_complete";
        public const string PurgeFailed = "maintenance.purge_failed";
    }
}

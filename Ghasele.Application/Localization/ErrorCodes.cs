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
        public const string RegistrationNotFound = "auth.registration_not_found";
        public const string RegistrationOtpNotVerified = "auth.registration_otp_not_verified";
        public const string OtpVerified = "auth.otp_verified";
        public const string PasswordReset = "auth.password_reset";
        public const string FcmTokenUpdated = "auth.fcm_token_updated";

        /// <summary>Takes the OTP code as <c>{0}</c>.</summary>
        public const string WhatsAppRegistrationOtpMessage = "auth.whatsapp_registration_otp_message";

        /// <summary>Takes the upstream WhatsApp Cloud API error message as <c>{0}</c>.</summary>
        public const string WhatsAppSendFailed = "auth.whatsapp_send_failed";

        // Firebase Phone Authentication (client-side SMS; separate from the WhatsApp OTP flow).
        public const string FirebaseTokenMissing = "auth.firebase_token_missing";
        public const string FirebaseTokenInvalid = "auth.firebase_token_invalid";

        // Orders
        public const string OrderNotFound = "order.not_found";
        public const string OrderPendingExists = "order.pending_exists";

        /// <summary>A guest checkout arrived with no contact number.</summary>
        public const string GuestContactNumberRequired = "order.guest_contact_number_required";
        public const string OrderNotPartOfTrip = "order.not_part_of_trip";
        public const string OrderDeleted = "order.deleted";
        public const string OrderItemNotFound = "orderitem.not_found";
        public const string OrderItemDeleted = "orderitem.deleted";

        // Trips
        public const string TripNotFound = "trip.not_found";
        public const string TripCleanerAndDriverRequired = "trip.cleaner_and_driver_required";
        public const string TripOrdersRequired = "trip.orders_required";
        public const string TripOrdersOutOfSequence = "trip.orders_out_of_sequence";
        public const string TripMustBeCollected = "trip.must_be_collected";
        public const string TripNotAssignedToCaller = "trip.not_assigned_to_caller";
        public const string TripDeleted = "trip.deleted";

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

        // Settings
        public const string DeliveryPriceNegative = "settings.delivery_price_negative";

        // Delivery windows (scheduled trips)
        public const string DeliveryWindowNotFound = "delivery_window.not_found";
        public const string DeliveryWindowInvalidRange = "delivery_window.invalid_range";
        public const string DeliveryWindowInvalidCapacity = "delivery_window.invalid_capacity";

        // Support tickets
        public const string TicketAttachmentTooLarge = "ticket.attachment_too_large";
        public const string TicketAttachmentInvalidType = "ticket.attachment_invalid_type";

        // Maintenance
        public const string PurgeComplete = "maintenance.purge_complete";
        public const string PurgeFailed = "maintenance.purge_failed";
    }
}

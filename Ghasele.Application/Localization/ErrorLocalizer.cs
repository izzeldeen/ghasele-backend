using System.Globalization;

namespace Ghasele.Application.Localization
{
    /// <summary>
    /// In-memory English/Arabic message catalog. Kept as plain dictionaries rather than .resx so the
    /// two languages sit side by side in one file and stay easy to diff and review.
    /// </summary>
    public class ErrorLocalizer : IErrorLocalizer
    {
        public string DefaultLanguage => "en";

        public IReadOnlyCollection<string> SupportedLanguages { get; } = new[] { "en", "ar" };

        private static readonly Dictionary<string, string> English = new(StringComparer.OrdinalIgnoreCase)
        {
            [ErrorCodes.InternalError] = "An unexpected error occurred. Please try again.",
            [ErrorCodes.InvalidToken] = "Invalid token.",

            [ErrorCodes.PhoneAlreadyExists] = "An account with this phone number already exists.",
            [ErrorCodes.InvalidCredentials] = "Invalid phone number or password.",
            [ErrorCodes.AppleTokenMissing] = "Missing Apple identity token.",
            [ErrorCodes.AppleTokenNoSubject] = "Apple identity token is not valid.",
            [ErrorCodes.AppleTokenInvalid] = "Apple sign-in could not be verified. Please try again.",
            [ErrorCodes.PhoneNotRegistered] = "No account is registered with this phone number.",
            [ErrorCodes.AccountNotFound] = "Account not found.",
            [ErrorCodes.UserNotFound] = "User not found.",
            [ErrorCodes.PhoneAlreadyVerified] = "This phone number is already verified.",
            [ErrorCodes.OtpInvalidOrExpired] = "Invalid or expired verification code.",
            [ErrorCodes.PhoneVerified] = "Phone number verified successfully.",
            [ErrorCodes.OtpSent] = "Verification code sent via WhatsApp successfully.",
            [ErrorCodes.OtpVerified] = "Verification code confirmed successfully.",
            [ErrorCodes.PasswordReset] = "Password reset successfully.",
            [ErrorCodes.FcmTokenUpdated] = "Token updated successfully.",

            [ErrorCodes.OrderNotFound] = "Order not found.",
            [ErrorCodes.OrderPendingExists] = "You already have a pending order. Please wait for it to be processed.",
            [ErrorCodes.OrderNotPartOfTrip] = "This order is not part of a trip.",
            [ErrorCodes.OrderDeleted] = "Order deleted successfully.",

            [ErrorCodes.TripNotFound] = "Trip not found.",
            [ErrorCodes.TripCleanerAndDriverRequired] = "A laundry and a driver must be selected to create a trip.",
            [ErrorCodes.TripOrdersRequired] = "At least one order must be selected to create a trip.",
            [ErrorCodes.TripOrdersOutOfSequence] = "Orders must be collected one by one in sequence.",
            [ErrorCodes.TripMustBeCollected] = "The trip must be in Collected status before delivering to the laundry.",
            [ErrorCodes.TripOrderAlreadyAssigned] = "Order {0} is already assigned to another trip.",

            [ErrorCodes.CleanerNotFound] = "Laundry not found.",
            [ErrorCodes.CleanerDeleted] = "Laundry deleted successfully.",

            [ErrorCodes.DriverNotFound] = "Driver not found.",
            [ErrorCodes.DriverDeleted] = "Driver deleted successfully.",

            [ErrorCodes.ItemTypeNotFound] = "Item type not found.",

            [ErrorCodes.MarketingCodeNotFound] = "Marketing code not found.",
            [ErrorCodes.MarketingCodeInvalidOrInactive] = "Invalid or inactive marketing code.",
            [ErrorCodes.MarketingCodeDeleted] = "Marketing code deleted successfully.",

            [ErrorCodes.LocationDeleted] = "Location deleted successfully.",

            [ErrorCodes.PurgeComplete] = "Purge complete.",
            [ErrorCodes.PurgeFailed] = "Purge failed and was rolled back.",
        };

        private static readonly Dictionary<string, string> Arabic = new(StringComparer.OrdinalIgnoreCase)
        {
            [ErrorCodes.InternalError] = "حدث خطأ غير متوقع. يرجى المحاولة مرة أخرى.",
            [ErrorCodes.InvalidToken] = "رمز الدخول غير صالح.",

            [ErrorCodes.PhoneAlreadyExists] = "يوجد حساب مسجل بهذا الرقم بالفعل.",
            [ErrorCodes.InvalidCredentials] = "رقم الهاتف أو كلمة المرور غير صحيحة.",
            [ErrorCodes.AppleTokenMissing] = "رمز هوية Apple مفقود.",
            [ErrorCodes.AppleTokenNoSubject] = "رمز هوية Apple غير صالح.",
            [ErrorCodes.AppleTokenInvalid] = "تعذّر التحقق من تسجيل الدخول عبر Apple. يرجى المحاولة مرة أخرى.",
            [ErrorCodes.PhoneNotRegistered] = "لا يوجد حساب مسجل بهذا الرقم.",
            [ErrorCodes.AccountNotFound] = "لم يتم العثور على الحساب.",
            [ErrorCodes.UserNotFound] = "لم يتم العثور على المستخدم.",
            [ErrorCodes.PhoneAlreadyVerified] = "تم توثيق رقم الهاتف مسبقاً.",
            [ErrorCodes.OtpInvalidOrExpired] = "رمز التحقق غير صحيح أو منتهي الصلاحية.",
            [ErrorCodes.PhoneVerified] = "تم توثيق رقم الهاتف بنجاح.",
            [ErrorCodes.OtpSent] = "تم إرسال رمز التحقق عبر واتساب بنجاح.",
            [ErrorCodes.OtpVerified] = "تم التحقق من الرمز بنجاح.",
            [ErrorCodes.PasswordReset] = "تم إعادة تعيين كلمة المرور بنجاح.",
            [ErrorCodes.FcmTokenUpdated] = "تم تحديث الرمز بنجاح.",

            [ErrorCodes.OrderNotFound] = "لم يتم العثور على الطلب.",
            [ErrorCodes.OrderPendingExists] = "لديك طلب قيد الانتظار بالفعل. يرجى انتظار معالجته.",
            [ErrorCodes.OrderNotPartOfTrip] = "هذا الطلب غير مرتبط بأي رحلة.",
            [ErrorCodes.OrderDeleted] = "تم حذف الطلب بنجاح.",

            [ErrorCodes.TripNotFound] = "لم يتم العثور على الرحلة.",
            [ErrorCodes.TripCleanerAndDriverRequired] = "يجب اختيار المغسلة والسائق لإنشاء الرحلة.",
            [ErrorCodes.TripOrdersRequired] = "يجب اختيار طلب واحد على الأقل لإنشاء الرحلة.",
            [ErrorCodes.TripOrdersOutOfSequence] = "يجب استلام الطلبات واحداً تلو الآخر بالترتيب.",
            [ErrorCodes.TripMustBeCollected] = "يجب أن تكون الرحلة في حالة \"تم الاستلام\" قبل التسليم إلى المغسلة.",
            [ErrorCodes.TripOrderAlreadyAssigned] = "الطلب {0} مُسند إلى رحلة أخرى بالفعل.",

            [ErrorCodes.CleanerNotFound] = "لم يتم العثور على المغسلة.",
            [ErrorCodes.CleanerDeleted] = "تم حذف المغسلة بنجاح.",

            [ErrorCodes.DriverNotFound] = "لم يتم العثور على السائق.",
            [ErrorCodes.DriverDeleted] = "تم حذف السائق بنجاح.",

            [ErrorCodes.ItemTypeNotFound] = "لم يتم العثور على نوع الصنف.",

            [ErrorCodes.MarketingCodeNotFound] = "لم يتم العثور على الرمز التسويقي.",
            [ErrorCodes.MarketingCodeInvalidOrInactive] = "الرمز التسويقي غير صالح أو غير مفعّل.",
            [ErrorCodes.MarketingCodeDeleted] = "تم حذف الرمز التسويقي بنجاح.",

            [ErrorCodes.LocationDeleted] = "تم حذف الموقع بنجاح.",

            [ErrorCodes.PurgeComplete] = "تمت عملية التنظيف بنجاح.",
            [ErrorCodes.PurgeFailed] = "فشلت عملية التنظيف وتم التراجع عنها.",
        };

        private static readonly Dictionary<string, Dictionary<string, string>> Catalogs =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["en"] = English,
                ["ar"] = Arabic,
            };

        public bool IsSupported(string? language) =>
            !string.IsNullOrWhiteSpace(language) && Catalogs.ContainsKey(Normalize(language));

        public string Localize(string code, string? language, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                code = ErrorCodes.InternalError;
            }

            var catalog = Catalogs.TryGetValue(Normalize(language), out var requested)
                ? requested
                : English;

            // Fall back through the default catalog before surfacing the bare code, so a message
            // that exists only in English still reads sensibly to an Arabic client.
            if (!catalog.TryGetValue(code, out var template) && !English.TryGetValue(code, out template))
            {
                return code;
            }

            if (args.Length == 0)
            {
                return template;
            }

            try
            {
                return string.Format(CultureInfo.InvariantCulture, template, args);
            }
            catch (FormatException)
            {
                return template;
            }
        }

        /// <summary>Reduces "ar-JO", "AR", " ar " and similar to the bare "ar" catalog key.</summary>
        private static string Normalize(string? language)
        {
            if (string.IsNullOrWhiteSpace(language))
            {
                return "en";
            }

            var trimmed = language.Trim();
            var separator = trimmed.IndexOfAny(new[] { '-', '_' });
            return separator > 0 ? trimmed[..separator] : trimmed;
        }
    }
}

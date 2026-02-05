using System;
using System.Linq;

namespace psms.Domain.Shared.Validators
{
    /// <summary>
    /// Validator for South African ID numbers using the Luhn algorithm.
    /// SA ID format: YYMMDD SSSS C A Z
    /// - YYMMDD: Date of birth
    /// - SSSS: Gender (0000-4999 = Female, 5000-9999 = Male)
    /// - C: Citizenship (0 = SA citizen, 1 = Permanent resident)
    /// - A: Usually 8 (historically used for race classification, now obsolete)
    /// - Z: Checksum digit (Luhn algorithm)
    /// </summary>
    public static class SAIdNumberValidator
    {
        /// <summary>
        /// Validates a South African ID number
        /// </summary>
        /// <param name="idNumber">The 13-digit SA ID number to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsValid(string idNumber)
        {
            if (string.IsNullOrWhiteSpace(idNumber))
                return false;

            // Remove any spaces or dashes
            idNumber = idNumber.Replace(" ", "").Replace("-", "");

            // Must be exactly 13 digits
            if (idNumber.Length != 13)
                return false;

            // Must contain only digits
            if (!idNumber.All(char.IsDigit))
                return false;

            // Validate date of birth portion
            if (!IsValidDateOfBirth(idNumber))
                return false;

            // Validate using Luhn algorithm
            return PassesLuhnCheck(idNumber);
        }

        /// <summary>
        /// Validates and returns detailed information about an SA ID number
        /// </summary>
        public static SAIdValidationResult Validate(string idNumber)
        {
            var result = new SAIdValidationResult();

            if (string.IsNullOrWhiteSpace(idNumber))
            {
                result.IsValid = false;
                result.ErrorMessage = "ID number is required.";
                return result;
            }

            // Remove any spaces or dashes
            idNumber = idNumber.Replace(" ", "").Replace("-", "");

            if (idNumber.Length != 13)
            {
                result.IsValid = false;
                result.ErrorMessage = "ID number must be exactly 13 digits.";
                return result;
            }

            if (!idNumber.All(char.IsDigit))
            {
                result.IsValid = false;
                result.ErrorMessage = "ID number must contain only digits.";
                return result;
            }

            // Extract and validate date of birth
            var dobResult = ExtractDateOfBirth(idNumber);
            if (!dobResult.HasValue)
            {
                result.IsValid = false;
                result.ErrorMessage = "Invalid date of birth in ID number.";
                return result;
            }

            result.DateOfBirth = dobResult.Value;

            // Extract gender
            var genderDigits = int.Parse(idNumber.Substring(6, 4));
            result.IsMale = genderDigits >= 5000;

            // Extract citizenship
            var citizenshipDigit = int.Parse(idNumber.Substring(10, 1));
            result.IsSACitizen = citizenshipDigit == 0;

            // Validate Luhn checksum
            if (!PassesLuhnCheck(idNumber))
            {
                result.IsValid = false;
                result.ErrorMessage = "ID number checksum is invalid.";
                return result;
            }

            result.IsValid = true;
            return result;
        }

        /// <summary>
        /// Extracts the date of birth from an SA ID number
        /// </summary>
        public static DateTime? ExtractDateOfBirth(string idNumber)
        {
            if (string.IsNullOrWhiteSpace(idNumber) || idNumber.Length < 6)
                return null;

            try
            {
                var year = int.Parse(idNumber.Substring(0, 2));
                var month = int.Parse(idNumber.Substring(2, 2));
                var day = int.Parse(idNumber.Substring(4, 2));

                // Determine century (people born after 2000 have YY < current year - 2000)
                var currentYear = DateTime.Now.Year;
                var currentYearShort = currentYear % 100;
                var century = (year <= currentYearShort) ? 2000 : 1900;

                var fullYear = century + year;

                // Validate the date
                if (month < 1 || month > 12)
                    return null;

                if (day < 1 || day > DateTime.DaysInMonth(fullYear, month))
                    return null;

                return new DateTime(fullYear, month, day);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Determines gender from SA ID number
        /// </summary>
        public static bool? IsMale(string idNumber)
        {
            if (string.IsNullOrWhiteSpace(idNumber) || idNumber.Length < 10)
                return null;

            if (!int.TryParse(idNumber.Substring(6, 4), out var genderDigits))
                return null;

            return genderDigits >= 5000;
        }

        /// <summary>
        /// Determines SA citizenship from ID number
        /// </summary>
        public static bool? IsSACitizen(string idNumber)
        {
            if (string.IsNullOrWhiteSpace(idNumber) || idNumber.Length < 11)
                return null;

            if (!int.TryParse(idNumber.Substring(10, 1), out var citizenshipDigit))
                return null;

            return citizenshipDigit == 0;
        }

        /// <summary>
        /// Validates that the date of birth portion of the ID is valid
        /// </summary>
        private static bool IsValidDateOfBirth(string idNumber)
        {
            return ExtractDateOfBirth(idNumber).HasValue;
        }

        /// <summary>
        /// Validates the ID number using the Luhn algorithm
        /// </summary>
        private static bool PassesLuhnCheck(string idNumber)
        {
            var sum = 0;
            var alternate = false;

            // Process from right to left
            for (int i = idNumber.Length - 1; i >= 0; i--)
            {
                var digit = idNumber[i] - '0';

                if (alternate)
                {
                    digit *= 2;
                    if (digit > 9)
                    {
                        digit -= 9;
                    }
                }

                sum += digit;
                alternate = !alternate;
            }

            return sum % 10 == 0;
        }
    }

    /// <summary>
    /// Result of SA ID number validation with extracted information
    /// </summary>
    public class SAIdValidationResult
    {
        /// <summary>
        /// Whether the ID number is valid
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Error message if validation failed
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Extracted date of birth from the ID number
        /// </summary>
        public DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// True if male, false if female (based on gender digits)
        /// </summary>
        public bool IsMale { get; set; }

        /// <summary>
        /// True if SA citizen, false if permanent resident
        /// </summary>
        public bool IsSACitizen { get; set; }

        /// <summary>
        /// Calculated age based on date of birth
        /// </summary>
        public int? Age => DateOfBirth.HasValue
            ? (int)((DateTime.Today - DateOfBirth.Value).TotalDays / 365.25)
            : null;
    }
}

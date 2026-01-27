using System;
using System.Linq;
using psms.Domain.Shared.Enums;

namespace psms.Domain.Shared.ValueObjects
{
    /// <summary>
    /// Represents and validates a South African ID number with Luhn algorithm check.
    /// Format: YYMMDDSSSSCZC where:
    /// - YYMMDD: Date of birth
    /// - SSSS: Gender (0000-4999 female, 5000-9999 male)
    /// - C: Citizenship (0=SA citizen, 1=permanent resident)
    /// - Z: Previously race indicator (no longer used)
    /// - C: Checksum digit (Luhn algorithm)
    /// </summary>
    public class SouthAfricanIdNumber : IEquatable<SouthAfricanIdNumber>
    {
        /// <summary>
        /// The 13-digit South African ID number.
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Parameterless constructor for EF Core.
        /// </summary>
        private SouthAfricanIdNumber()
        {
        }

        /// <summary>
        /// Creates a new South African ID number after validation.
        /// </summary>
        public SouthAfricanIdNumber(string idNumber)
        {
            if (!IsValid(idNumber))
                throw new ArgumentException($"Invalid South African ID number: {idNumber}", nameof(idNumber));

            Value = idNumber;
        }

        /// <summary>
        /// Validates a South African ID number using Luhn algorithm.
        /// </summary>
        public static bool IsValid(string idNumber)
        {
            if (string.IsNullOrWhiteSpace(idNumber) || idNumber.Length != 13)
                return false;

            if (!idNumber.All(char.IsDigit))
                return false;

            // Validate date portion
            if (!int.TryParse(idNumber.Substring(0, 2), out int year))
                return false;
            if (!int.TryParse(idNumber.Substring(2, 2), out int month))
                return false;
            if (!int.TryParse(idNumber.Substring(4, 2), out int day))
                return false;

            if (month < 1 || month > 12 || day < 1 || day > 31)
                return false;

            // Validate using Luhn algorithm
            return ValidateLuhnChecksum(idNumber);
        }

        /// <summary>
        /// Validates the checksum using the Luhn algorithm.
        /// </summary>
        private static bool ValidateLuhnChecksum(string idNumber)
        {
            int sum = 0;
            for (int i = 0; i < 13; i++)
            {
                int digit = int.Parse(idNumber[i].ToString());

                if (i % 2 == 0)
                {
                    sum += digit;
                }
                else
                {
                    int doubled = digit * 2;
                    sum += doubled > 9 ? doubled - 9 : doubled;
                }
            }
            return sum % 10 == 0;
        }

        /// <summary>
        /// Extracts date of birth from the ID number.
        /// </summary>
        public DateTime GetDateOfBirth()
        {
            int year = int.Parse(Value.Substring(0, 2));
            int month = int.Parse(Value.Substring(2, 2));
            int day = int.Parse(Value.Substring(4, 2));

            // Determine century: if year is between 00-current year (last 2 digits), assume 2000s, else 1900s
            int currentYearTwoDigit = DateTime.Now.Year % 100;
            int fullYear = year <= currentYearTwoDigit ? 2000 + year : 1900 + year;

            return new DateTime(fullYear, month, day);
        }

        /// <summary>
        /// Extracts gender from the ID number.
        /// </summary>
        public Gender GetGender()
        {
            int genderCode = int.Parse(Value.Substring(6, 4));
            return genderCode < 5000 ? Gender.Female : Gender.Male;
        }

        /// <summary>
        /// Determines if the ID holder is a South African citizen.
        /// </summary>
        public bool IsCitizen()
        {
            return Value[10] == '0';
        }

        /// <summary>
        /// Calculates the age based on the date of birth in the ID number.
        /// </summary>
        public int GetAge()
        {
            var dateOfBirth = GetDateOfBirth();
            var today = DateTime.Today;
            var age = today.Year - dateOfBirth.Year;

            if (dateOfBirth.Date > today.AddYears(-age))
                age--;

            return age;
        }

        /// <summary>
        /// Returns the ID number in masked format (e.g., "901234*****08").
        /// </summary>
        public string GetMaskedValue()
        {
            if (string.IsNullOrEmpty(Value) || Value.Length != 13)
                return Value;

            return $"{Value.Substring(0, 6)}*****{Value.Substring(11, 2)}";
        }

        public bool Equals(SouthAfricanIdNumber other)
        {
            if (other is null) return false;
            return Value == other.Value;
        }

        public override bool Equals(object obj) => Equals(obj as SouthAfricanIdNumber);

        public override int GetHashCode() => Value?.GetHashCode() ?? 0;

        public static bool operator ==(SouthAfricanIdNumber left, SouthAfricanIdNumber right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(SouthAfricanIdNumber left, SouthAfricanIdNumber right) => !(left == right);

        public override string ToString() => Value;
    }
}

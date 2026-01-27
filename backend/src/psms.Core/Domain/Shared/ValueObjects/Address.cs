using System;

namespace psms.Domain.Shared.ValueObjects
{
    /// <summary>
    /// Represents a South African address with standard components.
    /// Configured as owned type in DbContext.
    /// </summary>
    public class Address : IEquatable<Address>
    {
        /// <summary>
        /// Street address line (e.g., "123 Main Street").
        /// </summary>
        public string StreetAddress { get; private set; }

        /// <summary>
        /// Suburb/locality name.
        /// </summary>
        public string Suburb { get; private set; }

        /// <summary>
        /// City/town name.
        /// </summary>
        public string City { get; private set; }

        /// <summary>
        /// South African province (e.g., "Gauteng", "Western Cape").
        /// </summary>
        public string Province { get; private set; }

        /// <summary>
        /// Postal/ZIP code (4-digit SA format).
        /// </summary>
        public string PostalCode { get; private set; }

        /// <summary>
        /// Country name, defaults to "South Africa".
        /// </summary>
        public string Country { get; private set; }

        /// <summary>
        /// Parameterless constructor for EF Core.
        /// </summary>
        private Address()
        {
        }

        /// <summary>
        /// Creates a new Address value object.
        /// </summary>
        public Address(
            string streetAddress,
            string suburb,
            string city,
            string province,
            string postalCode,
            string country = "South Africa")
        {
            StreetAddress = streetAddress;
            Suburb = suburb;
            City = city;
            Province = province;
            PostalCode = postalCode;
            Country = country ?? "South Africa";
        }

        /// <summary>
        /// Returns the full formatted address.
        /// </summary>
        public string GetFullAddress()
        {
            return $"{StreetAddress}, {Suburb}, {City}, {Province} {PostalCode}, {Country}";
        }

        public bool Equals(Address other)
        {
            if (other is null) return false;
            return StreetAddress == other.StreetAddress &&
                   Suburb == other.Suburb &&
                   City == other.City &&
                   Province == other.Province &&
                   PostalCode == other.PostalCode &&
                   Country == other.Country;
        }

        public override bool Equals(object obj) => Equals(obj as Address);

        public override int GetHashCode()
        {
            return HashCode.Combine(StreetAddress, Suburb, City, Province, PostalCode, Country);
        }

        public static bool operator ==(Address left, Address right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(Address left, Address right) => !(left == right);
    }
}

using AquaControl.Domain.Common;

namespace AquaControl.Domain.ValueObjects;

/// <summary>
/// Represents a physical location within the aquaculture facility.
/// This is a value object that encapsulates location information including building, room, zone, and optional GPS coordinates.
/// </summary>
/// <remarks>
/// <para>
/// Location is an immutable value object that represents where a tank or equipment is physically located.
/// It enforces business rules such as:
/// </para>
/// <list type="bullet">
/// <item><description>Building and room are required fields</description></item>
/// <item><description>Zone is optional (for facilities with zone-based organization)</description></item>
/// <item><description>GPS coordinates are optional but must be valid if provided</description></item>
/// <item><description>Latitude must be between -90 and 90 degrees</description></item>
/// <item><description>Longitude must be between -180 and 180 degrees</description></item>
/// </list>
/// <para>
/// Two Location instances are considered equal if they have the same building, room, zone,
/// latitude, and longitude (after normalization). Case-insensitive comparison is used for text fields.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create a location with building and room
/// var location = Location.Create("Building A", "Room 101");
/// 
/// // Create a location with zone
/// var locationWithZone = Location.Create("Building A", "Room 101", "Zone 1");
/// 
/// // Create a location with GPS coordinates
/// var locationWithGPS = Location.Create("Building A", "Room 101", null, 40.7128m, -74.0060m);
/// 
/// // Calculate distance between locations
/// var distance = locationWithGPS.DistanceTo(otherLocation);
/// </code>
/// </example>
public sealed class Location : ValueObject
{
    /// <summary>
    /// Gets the building name or identifier where the location is situated.
    /// </summary>
    /// <value>
    /// The building name. Always non-null and non-empty.
    /// </value>
    public string Building { get; }

    /// <summary>
    /// Gets the room number or identifier within the building.
    /// </summary>
    /// <value>
    /// The room identifier. Always non-null and non-empty.
    /// </value>
    public string Room { get; }

    /// <summary>
    /// Gets the optional zone identifier within the facility.
    /// </summary>
    /// <value>
    /// The zone identifier, or <c>null</c> if the location is not assigned to a specific zone.
    /// </value>
    public string? Zone { get; }

    /// <summary>
    /// Gets the optional latitude coordinate in decimal degrees.
    /// </summary>
    /// <value>
    /// The latitude coordinate between -90 and 90 degrees, or <c>null</c> if GPS coordinates are not available.
    /// </value>
    public decimal? Latitude { get; }

    /// <summary>
    /// Gets the optional longitude coordinate in decimal degrees.
    /// </summary>
    /// <value>
    /// The longitude coordinate between -180 and 180 degrees, or <c>null</c> if GPS coordinates are not available.
    /// </value>
    public decimal? Longitude { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Location"/> class.
    /// </summary>
    /// <param name="building">The building name. Must not be null or empty.</param>
    /// <param name="room">The room identifier. Must not be null or empty.</param>
    /// <param name="zone">The optional zone identifier. Can be null.</param>
    /// <param name="latitude">The optional latitude coordinate. Must be between -90 and 90 if provided.</param>
    /// <param name="longitude">The optional longitude coordinate. Must be between -180 and 180 if provided.</param>
    /// <remarks>
    /// This constructor is private to enforce the use of the <see cref="Create"/> factory method,
    /// which performs validation before creating instances.
    /// </remarks>
    private Location(string building, string room, string? zone, decimal? latitude, decimal? longitude)
    {
        Building = building;
        Room = room;
        Zone = zone;
        Latitude = latitude;
        Longitude = longitude;
    }

    /// <summary>
    /// Creates a new instance of <see cref="Location"/> with validation.
    /// </summary>
    /// <param name="building">The building name. Must not be null or empty.</param>
    /// <param name="room">The room identifier. Must not be null or empty.</param>
    /// <param name="zone">The optional zone identifier. Defaults to <c>null</c> if not specified.</param>
    /// <param name="latitude">The optional latitude coordinate. Must be between -90 and 90 if provided.</param>
    /// <param name="longitude">The optional longitude coordinate. Must be between -180 and 180 if provided.</param>
    /// <returns>A new <see cref="Location"/> instance with the specified values.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when:
    /// <list type="bullet">
    /// <item><description><paramref name="building"/> is null, empty, or whitespace</description></item>
    /// <item><description><paramref name="room"/> is null, empty, or whitespace</description></item>
    /// <item><description><paramref name="latitude"/> is provided but not between -90 and 90</description></item>
    /// <item><description><paramref name="longitude"/> is provided but not between -180 and 180</description></item>
    /// </list>
    /// </exception>
    /// <remarks>
    /// <para>
    /// This factory method ensures that all Location instances are created with valid values.
    /// It performs validation before creating the instance, preventing invalid location objects
    /// from being created.
    /// </para>
    /// <para>
    /// Building and room are required fields as they represent the minimum information needed
    /// to identify a location within the facility. Zone and GPS coordinates are optional and
    /// can be added later if needed.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create with required fields only
    /// var location1 = Location.Create("Building A", "Room 101");
    /// 
    /// // Create with zone
    /// var location2 = Location.Create("Building A", "Room 101", "Zone 1");
    /// 
    /// // Create with GPS coordinates
    /// var location3 = Location.Create("Building A", "Room 101", null, 40.7128m, -74.0060m);
    /// 
    /// // Invalid - will throw ArgumentException
    /// var location4 = Location.Create("", "Room 101"); // Throws exception
    /// var location5 = Location.Create("Building A", "Room 101", null, 100m, -74.0060m); // Invalid latitude
    /// </code>
    /// </example>
    public static Location Create(string building, string room, string? zone = null, 
        decimal? latitude = null, decimal? longitude = null)
    {
        if (string.IsNullOrWhiteSpace(building))
            throw new ArgumentException("Building cannot be empty", nameof(building));

        if (string.IsNullOrWhiteSpace(room))
            throw new ArgumentException("Room cannot be empty", nameof(room));

        if (latitude.HasValue && (latitude < -90 || latitude > 90))
            throw new ArgumentException("Latitude must be between -90 and 90", nameof(latitude));

        if (longitude.HasValue && (longitude < -180 || longitude > 180))
            throw new ArgumentException("Longitude must be between -180 and 180", nameof(longitude));

        return new Location(building, room, zone, latitude, longitude);
    }

    /// <summary>
    /// Gets a formatted full address string combining building, room, and optional zone.
    /// </summary>
    /// <returns>
    /// A comma-separated string in the format: "Building, Room" or "Building, Room, Zone"
    /// if zone is provided. Example: "Building A, Room 101" or "Building A, Room 101, Zone 1".
    /// </returns>
    /// <remarks>
    /// This method provides a human-readable representation of the location that includes
    /// all non-GPS components. The zone is included only if it is not null or empty.
    /// </remarks>
    /// <example>
    /// <code>
    /// var location1 = Location.Create("Building A", "Room 101");
    /// Console.WriteLine(location1.GetFullAddress()); // "Building A, Room 101"
    /// 
    /// var location2 = Location.Create("Building A", "Room 101", "Zone 1");
    /// Console.WriteLine(location2.GetFullAddress()); // "Building A, Room 101, Zone 1"
    /// </code>
    /// </example>
    public string GetFullAddress()
    {
        var parts = new List<string> { Building, Room };
        if (!string.IsNullOrWhiteSpace(Zone))
            parts.Add(Zone);
        return string.Join(", ", parts);
    }

    /// <summary>
    /// Calculates the distance in meters between this location and another location using GPS coordinates.
    /// </summary>
    /// <param name="other">The other location to calculate distance to. Must not be null.</param>
    /// <returns>
    /// The distance in meters between the two locations, or <c>null</c> if either location
    /// does not have GPS coordinates (latitude and longitude).
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method uses the Haversine formula to calculate the great-circle distance between
    /// two points on Earth's surface. The formula accounts for Earth's spherical shape and
    /// provides accurate distance calculations for locations anywhere on the planet.
    /// </para>
    /// <para>
    /// The distance is returned in meters. To convert to other units:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Kilometers: divide by 1000</description></item>
    /// <item><description>Miles: multiply by 0.000621371</description></item>
    /// <item><description>Feet: multiply by 3.28084</description></item>
    /// </list>
    /// <para>
    /// If either location is missing GPS coordinates, the method returns <c>null</c> as
    /// distance calculation is not possible without coordinates.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var location1 = Location.Create("Building A", "Room 101", null, 40.7128m, -74.0060m); // New York
    /// var location2 = Location.Create("Building B", "Room 202", null, 40.7589m, -73.9851m); // Nearby location
    /// 
    /// var distance = location1.DistanceTo(location2);
    /// if (distance.HasValue)
    /// {
    ///     Console.WriteLine($"Distance: {distance.Value:F2} meters");
    ///     Console.WriteLine($"Distance: {distance.Value / 1000:F2} kilometers");
    /// }
    /// 
    /// // If GPS coordinates are missing
    /// var location3 = Location.Create("Building C", "Room 303");
    /// var noDistance = location1.DistanceTo(location3); // Returns null
    /// </code>
    /// </example>
    public double? DistanceTo(Location other)
    {
        if (!Latitude.HasValue || !Longitude.HasValue || 
            !other.Latitude.HasValue || !other.Longitude.HasValue)
            return null;

        // Haversine formula for distance calculation
        const double R = 6371; // Earth's radius in kilometers
        
        var lat1Rad = (double)Latitude.Value * Math.PI / 180;
        var lat2Rad = (double)other.Latitude.Value * Math.PI / 180;
        var deltaLatRad = ((double)other.Latitude.Value - (double)Latitude.Value) * Math.PI / 180;
        var deltaLonRad = ((double)other.Longitude.Value - (double)Longitude.Value) * Math.PI / 180;

        var a = Math.Sin(deltaLatRad / 2) * Math.Sin(deltaLatRad / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(deltaLonRad / 2) * Math.Sin(deltaLonRad / 2);
        
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        
        return R * c * 1000; // Return distance in meters
    }

    /// <summary>
    /// Returns a string representation of the location.
    /// </summary>
    /// <returns>
    /// A formatted string showing the full address (building, room, and optional zone).
    /// This is equivalent to calling <see cref="GetFullAddress"/>.
    /// </returns>
    /// <remarks>
    /// The string representation provides a human-readable format suitable for display
    /// in user interfaces, logs, and reports. GPS coordinates are not included in the
    /// string representation but can be accessed via the <see cref="Latitude"/> and
    /// <see cref="Longitude"/> properties.
    /// </remarks>
    /// <example>
    /// <code>
    /// var location = Location.Create("Building A", "Room 101", "Zone 1");
    /// Console.WriteLine(location); // Outputs: "Building A, Room 101, Zone 1"
    /// </code>
    /// </example>
    public override string ToString() => GetFullAddress();

    /// <summary>
    /// Gets the components that define equality for this value object.
    /// </summary>
    /// <returns>
    /// An enumerable containing the normalized <see cref="Building"/>, <see cref="Room"/>,
    /// <see cref="Zone"/> (or empty string if null), <see cref="Latitude"/> (or 0 if null),
    /// and <see cref="Longitude"/> (or 0 if null).
    /// </returns>
    /// <remarks>
    /// <para>
    /// Two Location instances are considered equal if:
    /// </para>
    /// <list type="bullet">
    /// <item><description>They have the same <see cref="Building"/> (case-insensitive)</description></item>
    /// <item><description>They have the same <see cref="Room"/> (case-insensitive)</description></item>
    /// <item><description>They have the same <see cref="Zone"/> (case-insensitive, null treated as empty)</description></item>
    /// <item><description>They have the same <see cref="Latitude"/> (null treated as 0)</description></item>
    /// <item><description>They have the same <see cref="Longitude"/> (null treated as 0)</description></item>
    /// </list>
    /// <para>
    /// Text fields are normalized to uppercase for comparison, so "Building A" and "building a"
    /// are considered equal. Null values for optional fields are normalized to default values
    /// (empty string for Zone, 0 for coordinates) to ensure consistent comparison.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var location1 = Location.Create("Building A", "Room 101", "Zone 1");
    /// var location2 = Location.Create("building a", "room 101", "zone 1"); // lowercase
    /// bool areEqual = location1.Equals(location2); // true (case-insensitive comparison)
    /// 
    /// var location3 = Location.Create("Building A", "Room 101"); // no zone
    /// var location4 = Location.Create("Building A", "Room 101", null); // explicit null zone
    /// bool areEqual2 = location3.Equals(location4); // true (null zone equals no zone)
    /// 
    /// var location5 = Location.Create("Building A", "Room 101", null, 40.7128m, -74.0060m);
    /// var location6 = Location.Create("Building A", "Room 101", null, 40.7128m, -74.0060m);
    /// bool areEqual3 = location5.Equals(location6); // true (same coordinates)
    /// </code>
    /// </example>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Building.ToUpperInvariant();
        yield return Room.ToUpperInvariant();
        yield return Zone?.ToUpperInvariant() ?? string.Empty;
        yield return Latitude ?? 0;
        yield return Longitude ?? 0;
    }
}


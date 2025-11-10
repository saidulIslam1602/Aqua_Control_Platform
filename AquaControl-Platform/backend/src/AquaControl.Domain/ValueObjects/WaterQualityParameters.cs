using AquaControl.Domain.Common;

namespace AquaControl.Domain.ValueObjects;

/// <summary>
/// Represents the optimal water quality parameters for an aquaculture tank.
/// This value object encapsulates all water quality measurements including temperature, pH, dissolved oxygen, and salinity.
/// </summary>
/// <remarks>
/// All parameters are optional (nullable) to allow flexibility in configuration.
/// When min/max values are provided, they must be valid ranges (min < max).
/// This value object is immutable and provides validation methods to check if actual readings are within acceptable ranges.
/// </remarks>
public sealed class WaterQualityParameters : ValueObject
{
    /// <summary>
    /// Gets the optimal temperature value in degrees Celsius.
    /// </summary>
    /// <value>The target temperature for optimal water quality, or null if not specified.</value>
    public decimal? OptimalTemperature { get; }

    /// <summary>
    /// Gets the minimum acceptable temperature value in degrees Celsius.
    /// </summary>
    /// <value>The lower bound of acceptable temperature range, or null if not specified.</value>
    public decimal? MinTemperature { get; }

    /// <summary>
    /// Gets the maximum acceptable temperature value in degrees Celsius.
    /// </summary>
    /// <value>The upper bound of acceptable temperature range, or null if not specified.</value>
    public decimal? MaxTemperature { get; }

    /// <summary>
    /// Gets the optimal pH value.
    /// </summary>
    /// <value>The target pH level (typically 0-14), or null if not specified.</value>
    public decimal? OptimalPH { get; }

    /// <summary>
    /// Gets the minimum acceptable pH value.
    /// </summary>
    /// <value>The lower bound of acceptable pH range, or null if not specified.</value>
    public decimal? MinPH { get; }

    /// <summary>
    /// Gets the maximum acceptable pH value.
    /// </summary>
    /// <value>The upper bound of acceptable pH range, or null if not specified.</value>
    public decimal? MaxPH { get; }

    /// <summary>
    /// Gets the optimal dissolved oxygen level in milligrams per liter (mg/L).
    /// </summary>
    /// <value>The target dissolved oxygen concentration, or null if not specified.</value>
    public decimal? OptimalOxygen { get; }

    /// <summary>
    /// Gets the minimum acceptable dissolved oxygen level in milligrams per liter (mg/L).
    /// </summary>
    /// <value>The minimum required dissolved oxygen concentration, or null if not specified.</value>
    public decimal? MinOxygen { get; }

    /// <summary>
    /// Gets the optimal salinity value in parts per thousand (ppt) or practical salinity units (PSU).
    /// </summary>
    /// <value>The target salinity level, or null if not specified.</value>
    public decimal? OptimalSalinity { get; }

    /// <summary>
    /// Gets the minimum acceptable salinity value in parts per thousand (ppt) or practical salinity units (PSU).
    /// </summary>
    /// <value>The lower bound of acceptable salinity range, or null if not specified.</value>
    public decimal? MinSalinity { get; }

    /// <summary>
    /// Gets the maximum acceptable salinity value in parts per thousand (ppt) or practical salinity units (PSU).
    /// </summary>
    /// <value>The upper bound of acceptable salinity range, or null if not specified.</value>
    public decimal? MaxSalinity { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WaterQualityParameters"/> class.
    /// </summary>
    /// <param name="optimalTemperature">The optimal temperature value.</param>
    /// <param name="minTemperature">The minimum acceptable temperature value.</param>
    /// <param name="maxTemperature">The maximum acceptable temperature value.</param>
    /// <param name="optimalPH">The optimal pH value.</param>
    /// <param name="minPH">The minimum acceptable pH value.</param>
    /// <param name="maxPH">The maximum acceptable pH value.</param>
    /// <param name="optimalOxygen">The optimal dissolved oxygen level.</param>
    /// <param name="minOxygen">The minimum acceptable dissolved oxygen level.</param>
    /// <param name="optimalSalinity">The optimal salinity value.</param>
    /// <param name="minSalinity">The minimum acceptable salinity value.</param>
    /// <param name="maxSalinity">The maximum acceptable salinity value.</param>
    private WaterQualityParameters(
        decimal? optimalTemperature, decimal? minTemperature, decimal? maxTemperature,
        decimal? optimalPH, decimal? minPH, decimal? maxPH,
        decimal? optimalOxygen, decimal? minOxygen,
        decimal? optimalSalinity, decimal? minSalinity, decimal? maxSalinity)
    {
        OptimalTemperature = optimalTemperature;
        MinTemperature = minTemperature;
        MaxTemperature = maxTemperature;
        OptimalPH = optimalPH;
        MinPH = minPH;
        MaxPH = maxPH;
        OptimalOxygen = optimalOxygen;
        MinOxygen = minOxygen;
        OptimalSalinity = optimalSalinity;
        MinSalinity = minSalinity;
        MaxSalinity = maxSalinity;
    }

    /// <summary>
    /// Creates a new instance of <see cref="WaterQualityParameters"/> with the specified values.
    /// </summary>
    /// <param name="optimalTemperature">The optimal temperature value in degrees Celsius.</param>
    /// <param name="minTemperature">The minimum acceptable temperature value in degrees Celsius.</param>
    /// <param name="maxTemperature">The maximum acceptable temperature value in degrees Celsius.</param>
    /// <param name="optimalPH">The optimal pH value (typically 0-14).</param>
    /// <param name="minPH">The minimum acceptable pH value.</param>
    /// <param name="maxPH">The maximum acceptable pH value.</param>
    /// <param name="optimalOxygen">The optimal dissolved oxygen level in mg/L.</param>
    /// <param name="minOxygen">The minimum acceptable dissolved oxygen level in mg/L.</param>
    /// <param name="optimalSalinity">The optimal salinity value in ppt/PSU.</param>
    /// <param name="minSalinity">The minimum acceptable salinity value in ppt/PSU.</param>
    /// <param name="maxSalinity">The maximum acceptable salinity value in ppt/PSU.</param>
    /// <returns>A new instance of <see cref="WaterQualityParameters"/> with validated values.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when min/max value pairs are invalid (min >= max) for temperature, pH, or salinity.
    /// </exception>
    /// <remarks>
    /// All parameters are optional. Validation ensures that when both min and max values are provided,
    /// the min value is strictly less than the max value.
    /// </remarks>
    public static WaterQualityParameters Create(
        decimal? optimalTemperature = null, decimal? minTemperature = null, decimal? maxTemperature = null,
        decimal? optimalPH = null, decimal? minPH = null, decimal? maxPH = null,
        decimal? optimalOxygen = null, decimal? minOxygen = null,
        decimal? optimalSalinity = null, decimal? minSalinity = null, decimal? maxSalinity = null)
    {
        // Validation logic: Ensure min < max when both are provided
        if (minTemperature.HasValue && maxTemperature.HasValue && minTemperature >= maxTemperature)
            throw new ArgumentException("Min temperature must be less than max temperature");

        if (minPH.HasValue && maxPH.HasValue && minPH >= maxPH)
            throw new ArgumentException("Min pH must be less than max pH");

        if (minSalinity.HasValue && maxSalinity.HasValue && minSalinity >= maxSalinity)
            throw new ArgumentException("Min salinity must be less than max salinity");

        return new WaterQualityParameters(
            optimalTemperature, minTemperature, maxTemperature,
            optimalPH, minPH, maxPH,
            optimalOxygen, minOxygen,
            optimalSalinity, minSalinity, maxSalinity);
    }

    /// <summary>
    /// Determines whether the specified temperature value is within the acceptable range.
    /// </summary>
    /// <param name="temperature">The temperature value to check in degrees Celsius.</param>
    /// <returns>
    /// <c>true</c> if the temperature is within the acceptable range (between min and max, if specified);
    /// otherwise, <c>false</c>. Returns <c>true</c> if no range is specified.
    /// </returns>
    public bool IsTemperatureInRange(decimal temperature)
    {
        if (MinTemperature.HasValue && temperature < MinTemperature.Value) return false;
        if (MaxTemperature.HasValue && temperature > MaxTemperature.Value) return false;
        return true;
    }

    /// <summary>
    /// Determines whether the specified pH value is within the acceptable range.
    /// </summary>
    /// <param name="ph">The pH value to check.</param>
    /// <returns>
    /// <c>true</c> if the pH is within the acceptable range (between min and max, if specified);
    /// otherwise, <c>false</c>. Returns <c>true</c> if no range is specified.
    /// </returns>
    public bool IsPHInRange(decimal ph)
    {
        if (MinPH.HasValue && ph < MinPH.Value) return false;
        if (MaxPH.HasValue && ph > MaxPH.Value) return false;
        return true;
    }

    /// <summary>
    /// Determines whether the specified dissolved oxygen level is sufficient.
    /// </summary>
    /// <param name="oxygen">The dissolved oxygen level to check in mg/L.</param>
    /// <returns>
    /// <c>true</c> if the oxygen level meets or exceeds the minimum requirement (if specified);
    /// otherwise, <c>true</c> if no minimum is specified.
    /// </returns>
    public bool IsOxygenSufficient(decimal oxygen)
    {
        return !MinOxygen.HasValue || oxygen >= MinOxygen.Value;
    }

    /// <summary>
    /// Determines whether the specified salinity value is within the acceptable range.
    /// </summary>
    /// <param name="salinity">The salinity value to check in ppt/PSU.</param>
    /// <returns>
    /// <c>true</c> if the salinity is within the acceptable range (between min and max, if specified);
    /// otherwise, <c>false</c>. Returns <c>true</c> if no range is specified.
    /// </returns>
    public bool IsSalinityInRange(decimal salinity)
    {
        if (MinSalinity.HasValue && salinity < MinSalinity.Value) return false;
        if (MaxSalinity.HasValue && salinity > MaxSalinity.Value) return false;
        return true;
    }

    /// <summary>
    /// Gets the components that are used to determine equality for this value object.
    /// </summary>
    /// <returns>An enumerable collection of all properties that define equality for this value object.</returns>
    /// <remarks>
    /// Null values are treated as 0 for equality comparison purposes.
    /// Two instances with the same property values (including nulls) are considered equal.
    /// </remarks>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return OptimalTemperature ?? 0;
        yield return MinTemperature ?? 0;
        yield return MaxTemperature ?? 0;
        yield return OptimalPH ?? 0;
        yield return MinPH ?? 0;
        yield return MaxPH ?? 0;
        yield return OptimalOxygen ?? 0;
        yield return MinOxygen ?? 0;
        yield return OptimalSalinity ?? 0;
        yield return MinSalinity ?? 0;
        yield return MaxSalinity ?? 0;
    }
}
using System;

[System.Serializable]
public class OrbitalData
{
    public double epochJD;
    public DateTime dateUTC;
    public double semiMajorAxisAU;
    public double eccentricity;
    public double inclinationDegrees;
    public double longitudeOfAscendingNodeDegrees;
    public double argumentOfPeriapsisDegrees;
    public double meanAnomalyDegrees;
    public double trueAnomalyDegrees;

    public OrbitalData(string csvLine)
    {
        string[] values = csvLine.Split(',');

        if (values.Length >= 9)
        {
            var culture = System.Globalization.CultureInfo.InvariantCulture;

            epochJD = double.Parse(values[0].Trim(), culture);
            dateUTC = DateTime.Parse(values[1].Trim(), culture);
            semiMajorAxisAU = double.Parse(values[2].Trim(), culture);
            eccentricity = double.Parse(values[3].Trim(), culture);
            inclinationDegrees = double.Parse(values[4].Trim(), culture);
            longitudeOfAscendingNodeDegrees = double.Parse(values[5].Trim(), culture);
            argumentOfPeriapsisDegrees = double.Parse(values[6].Trim(), culture);
            meanAnomalyDegrees = double.Parse(values[7].Trim(), culture);
            trueAnomalyDegrees = double.Parse(values[8].Trim(), culture);
        }
    }

    public DateTime GetLocalTime()
    {
        return dateUTC.ToLocalTime();
    }

    public string GetFormattedDisplay()
    {
        return $"Date (Local): {GetLocalTime():yyyy-MM-dd HH:mm:ss}\n" +
               $"Semi-major Axis: {semiMajorAxisAU:F6} AU\n" +
               $"Eccentricity: {eccentricity:F6}\n" +
               $"Inclination: {inclinationDegrees:F2}°\n" +
               $"Long. of Asc. Node: {longitudeOfAscendingNodeDegrees:F2}°\n" +
               $"Arg. of Periapsis: {argumentOfPeriapsisDegrees:F2}°\n" +
               $"Mean Anomaly: {meanAnomalyDegrees:F2}°\n" +
               $"True Anomaly: {trueAnomalyDegrees:F2}°";
    }
}
using System;


//custom implementation of orbital position calculation using Kepler's laws
public static class CustomOrbitalCalculator
{
    private const double DEG_TO_RAD = Math.PI / 180.0;
    private const double AU_TO_KM = 149597870.7; //1 AU in kilometers

    //returns position in thousands of kilometers (10^3 km)

    public static Vector3Double CalculateOrbitalPosition(
        double semiMajorAxisAU,
        double eccentricity,
        double inclinationDeg,
        double longitudeOfAscendingNodeDeg,
        double argumentOfPeriapsisDeg,
        double trueAnomalyDeg)
    {
        //convert degrees to radians
        double i = inclinationDeg * DEG_TO_RAD;
        double omega = longitudeOfAscendingNodeDeg * DEG_TO_RAD;
        double w = argumentOfPeriapsisDeg * DEG_TO_RAD;
        double v = trueAnomalyDeg * DEG_TO_RAD;

        //convert semi-major axis from AU to km
        double a = semiMajorAxisAU * AU_TO_KM;

        //calculate distance from focus (sun) using true anomaly
        double r = a * (1 - eccentricity * eccentricity) / (1 + eccentricity * Math.Cos(v));

        //position in orbital plane
        double xOrbital = r * Math.Cos(v);
        double yOrbital = r * Math.Sin(v);

        //rotate by argument of periapsis
        double xPerifocal = xOrbital;
        double yPerifocal = yOrbital;

        //transform to 3D space using rotation matrices
        //first rotate by argument of periapsis (w), then inclination (i), then ascending node (omega)

        //combined rotation matrix elements
        double cosW = Math.Cos(w);
        double sinW = Math.Sin(w);
        double cosI = Math.Cos(i);
        double sinI = Math.Sin(i);
        double cosOmega = Math.Cos(omega);
        double sinOmega = Math.Sin(omega);

        //apply full transformation
        double x = (cosOmega * cosW - sinOmega * sinW * cosI) * xPerifocal +
                   (-cosOmega * sinW - sinOmega * cosW * cosI) * yPerifocal;

        double y = (sinOmega * cosW + cosOmega * sinW * cosI) * xPerifocal +
                   (-sinOmega * sinW + cosOmega * cosW * cosI) * yPerifocal;

        double z = (sinW * sinI) * xPerifocal +
                   (cosW * sinI) * yPerifocal;

        //convert to thousands of kilometers (10^3 km)
        return new Vector3Double(x / 1000.0, y / 1000.0, z / 1000.0);
    }

    //interpolate between two orbital positions
    public static Vector3Double InterpolatePosition(
        OrbitalData data1, OrbitalData data2, float t)
    {
        //for visual purposes, linear interpolation of positions works well enough

        var pos1 = CalculateOrbitalPosition(
            data1.semiMajorAxisAU,
            data1.eccentricity,
            data1.inclinationDegrees,
            data1.longitudeOfAscendingNodeDegrees,
            data1.argumentOfPeriapsisDegrees,
            data1.trueAnomalyDegrees
        );

        var pos2 = CalculateOrbitalPosition(
            data2.semiMajorAxisAU,
            data2.eccentricity,
            data2.inclinationDegrees,
            data2.longitudeOfAscendingNodeDegrees,
            data2.argumentOfPeriapsisDegrees,
            data2.trueAnomalyDegrees
        );

        //linear interpolation
        return new Vector3Double(
            pos1.x + (pos2.x - pos1.x) * t,
            pos1.y + (pos2.y - pos1.y) * t,
            pos1.z + (pos2.z - pos1.z) * t
        );
    }


    //calculate mean anomaly from true anomaly for interpolation purposes

    public static double TrueToMeanAnomaly(double trueAnomalyDeg, double eccentricity)
    {
        double v = trueAnomalyDeg * DEG_TO_RAD;
        double e = eccentricity;

        //calculate eccentric anomaly from true anomaly
        double E = Math.Atan2(Math.Sqrt(1 - e * e) * Math.Sin(v), e + Math.Cos(v));

        //calculate mean anomaly from eccentric anomaly (Kepler's equation)
        double M = E - e * Math.Sin(E);

        return M * 180.0 / Math.PI; //convert back to degrees
    }
}
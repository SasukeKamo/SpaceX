using System;

[Serializable]
public struct Vector3Double
{
    public double x;
    public double y;
    public double z;

    public Vector3Double(double x, double y, double z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public override string ToString()
    {
        return $"({x}, {y}, {z})";
    }

    public static Vector3Double zero => new Vector3Double(0, 0, 0);

    public static Vector3Double operator +(Vector3Double a, Vector3Double b)
    {
        return new Vector3Double(a.x + b.x, a.y + b.y, a.z + b.z);
    }

    public static Vector3Double operator -(Vector3Double a, Vector3Double b)
    {
        return new Vector3Double(a.x - b.x, a.y - b.y, a.z - b.z);
    }

    public static Vector3Double operator *(Vector3Double a, double scalar)
    {
        return new Vector3Double(a.x * scalar, a.y * scalar, a.z * scalar);
    }

    public static Vector3Double operator *(double scalar, Vector3Double a)
    {
        return new Vector3Double(a.x * scalar, a.y * scalar, a.z * scalar);
    }

    public double Magnitude()
    {
        return Math.Sqrt(x * x + y * y + z * z);
    }

    public Vector3Double Normalized()
    {
        double mag = Magnitude();
        if (mag > 0)
            return new Vector3Double(x / mag, y / mag, z / mag);
        return zero;
    }
}
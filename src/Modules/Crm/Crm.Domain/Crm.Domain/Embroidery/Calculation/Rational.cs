using System.Numerics;

namespace Crm.Domain.Embroidery.Calculation;

/// <summary>
/// An exact non-negative fraction for the embroidery price (EMB-001, R-07): every step keeps full precision and only
/// the final values are rounded half-up, exactly like the old calculator.
/// </summary>
public readonly record struct Rational
{
    private Rational(BigInteger numerator, BigInteger denominator)
    {
        if (denominator.IsZero) throw new DivideByZeroException();
        if (denominator.Sign < 0) (numerator, denominator) = (-numerator, -denominator);
        var gcd = BigInteger.GreatestCommonDivisor(numerator, denominator);
        if (!gcd.IsOne && !gcd.IsZero) (numerator, denominator) = (numerator / gcd, denominator / gcd);
        Numerator = numerator;
        Denominator = denominator;
    }

    public BigInteger Numerator { get; }
    public BigInteger Denominator { get; }

    public static Rational Zero => new(BigInteger.Zero, BigInteger.One);
    public static Rational One => new(BigInteger.One, BigInteger.One);

    public bool IsZero => Numerator.IsZero;
    public bool IsPositive => Numerator.Sign > 0;

    public static Rational Of(long value) => new(value, BigInteger.One);

    public static Rational Of(long numerator, long denominator) => new(numerator, denominator);

    /// <summary>A decimal is a finite fraction, so the conversion is exact.</summary>
    public static Rational Of(decimal value)
    {
        var bits = decimal.GetBits(value);
        var mantissa = new BigInteger((uint)bits[0]) | (new BigInteger((uint)bits[1]) << 32) |
            (new BigInteger((uint)bits[2]) << 64);
        var scale = (bits[3] >> 16) & 0xFF;
        if (bits[3] < 0) mantissa = -mantissa;
        return new Rational(mantissa, BigInteger.Pow(10, scale));
    }

    public static Rational operator +(Rational a, Rational b) =>
        new(a.Numerator * b.Denominator + b.Numerator * a.Denominator, a.Denominator * b.Denominator);

    public static Rational operator -(Rational a, Rational b) =>
        new(a.Numerator * b.Denominator - b.Numerator * a.Denominator, a.Denominator * b.Denominator);

    public static Rational operator *(Rational a, Rational b) =>
        new(a.Numerator * b.Numerator, a.Denominator * b.Denominator);

    public static Rational operator /(Rational a, Rational b) =>
        new(a.Numerator * b.Denominator, a.Denominator * b.Numerator);

    public static bool operator >(Rational a, Rational b) => a.Numerator * b.Denominator > b.Numerator * a.Denominator;

    public static bool operator <(Rational a, Rational b) => a.Numerator * b.Denominator < b.Numerator * a.Denominator;

    public BigInteger Floor() => BigInteger.Divide(Numerator, Denominator) -
        (Numerator.Sign < 0 && !BigInteger.Remainder(Numerator, Denominator).IsZero ? 1 : 0);

    public BigInteger Ceiling() => -(new Rational(-Numerator, Denominator).Floor());

    /// <summary>Half-up for a non-negative value: floor((2·n·10^p + d) / (2·d)) / 10^p.</summary>
    public decimal RoundHalfUp(int places = 2)
    {
        if (Numerator.Sign < 0) throw new InvalidOperationException("Only non-negative values are rounded.");
        var scale = BigInteger.Pow(10, places);
        var scaled = (2 * Numerator * scale + Denominator) / (2 * Denominator);
        return (decimal)scaled / (decimal)scale;
    }
}

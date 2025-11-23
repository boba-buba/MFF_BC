using System.Numerics;
using System.Runtime.CompilerServices;

namespace PhysicsUnitsLib {
	
	// TODO: Rename this file.
	// TODO: Rename, change, or delete the following class.
	// TODO: Add you own files, types, methods, etc. in this project.

    public struct Meter
    {
        public Meter(double value) { this.Value = value; }
		public static Meter operator +(Meter left, Meter right) => new Meter(left.Value + right.Value);
        public static Meter operator -(Meter left, Meter right) => new Meter(left.Value - right.Value);
		public static Meter operator *(Meter m, int koef) => new Meter(m.Value * koef);
		public static explicit operator double(Meter m) => m.Value; /** Trying out explicit conversion */
        public override string ToString() {  return Value.ToString() + " m"; }

        public double Value;
    }

	public struct Second
	{
		public Second(double value) { this.Value = value; }
        public static MeterPerSecond operator /(Meter m, Second t)
        {
			if (t.Value == 0)
			{
				double result = m.Value < 0 ? double.NegativeInfinity : double.PositiveInfinity;
				return new MeterPerSecond(result);
			}
            return new MeterPerSecond(m.Value / t.Value);
        }
        public override string ToString() { return Value.ToString() + " s"; }

        public double Value;
	}

	public struct MeterPerSecond
	{
		public MeterPerSecond(double value) { this.Value = value; }
		public static MeterPerSecond operator *(MeterPerSecond s, int koef)
		{
			return new MeterPerSecond(s.Value * koef);
		}
		public static Meter operator *(MeterPerSecond speed, Second time)
		{
			return new Meter(speed.Value * time.Value);
		}
		public static Meter operator *(Second time,  MeterPerSecond speed)
		{
            return new Meter(speed.Value * time.Value);
        }
		public static MeterPerSecond operator +(MeterPerSecond left, MeterPerSecond right) => new MeterPerSecond(left.Value + right.Value);
        public override string ToString() { return Value.ToString() + " m/s"; }
        public double Value;
	}

	public static class DoubleExtensions
	{
		public static Meter Meters(this double value) => new Meter(value);
		public static Second Seconds(this double value) => new Second(value);
		public static MeterPerSecond MeterPerSeconds(this double value) => new MeterPerSecond(value);
    }
	public static class IntExtensions
	{
        public static Meter Meters(this int v) => new Meter(v);
        public static Second Seconds(this int value) => new Second(value);
        public static MeterPerSecond MeterPerSeconds(this int value) => new MeterPerSecond(value);
    }
}

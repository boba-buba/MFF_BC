using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Cuni.Arithmetics.FixedPoint
{

    //D = Dot3/Dot4/Dot5/Dot8/Dot16/Dot24
    public interface IDot
    {
        public int D { get; }
    }
    struct Dot3 : IDot
    {
        public Dot3() { }
        public int D { get; } = 3;
    }

    struct Dot4 : IDot
    {
        public Dot4() { }
        public int D { get; } = 4;
    }

    struct Dot5 : IDot
    {
        public Dot5() { }
        public int D { get; } = 5;
    }

    struct Dot8 : IDot
    {
        public Dot8() { }
        public int D { get; } = 8;
    }
    struct Dot16 : IDot
    {
        public Dot16() { }
        public int D { get; } = 16;
    }

    struct Dot24 : IDot
    {
        public Dot24() { }
        public int D { get; } = 24;
    }

    public record struct Fixed<TBitWidth, TDot> : IAdditionOperators<Fixed<TBitWidth, TDot>, Fixed<TBitWidth, TDot>, Fixed<TBitWidth, TDot>>
        where TDot : IDot, new()
        where TBitWidth : struct, IBinaryNumber<TBitWidth>, IShiftOperators<TBitWidth, int, TBitWidth>
    {
        TBitWidth number;
        TDot dot = new TDot();
        public Fixed(double number)
        {
            var repr = (long)(number * (1 << dot.D));
            this.number = TBitWidth.CreateTruncating(repr);
        }
        public Fixed() => dot = new();
        public double ToDouble()
        {
            long repr = long.CreateChecked(number);
            double doubleRepr = (repr / (double)(1 << dot.D));
            return doubleRepr;
        }

        public override string ToString() => ToDouble().ToString();

        public static Fixed<TBitWidth, TDot> operator +(Fixed<TBitWidth, TDot> left, Fixed<TBitWidth, TDot> right)
        {
            left.number += right.number;
            return left;
        }
        public static Fixed<TBitWidth, TDot> operator -(Fixed<TBitWidth, TDot> left, Fixed<TBitWidth, TDot> right)
        {
            left.number -= right.number;
            return left;
        }

        public static Fixed<TBitWidth, TDot> operator *(Fixed<TBitWidth, TDot> left, Fixed<TBitWidth, TDot> right)
        {
            long longLeft = long.CreateChecked(left.number);
            long longRight = long.CreateChecked(right.number);

            long interResult = longLeft * longRight;
            interResult >>= left.dot.D;
            left.number = TBitWidth.CreateTruncating(interResult);
            return left;
        }

        public static Fixed<TBitWidth, TDot> operator /(Fixed<TBitWidth, TDot> left, Fixed<TBitWidth, TDot> right)
        {
            long longLeft = long.CreateChecked(left.number);
            longLeft <<= left.dot.D;
            long longRight = long.CreateChecked(right.number);

            long interResult = longLeft / longRight;

            left.number = TBitWidth.CreateTruncating(interResult);
            return left;
        }

        public Fixed<TBitWidth, TDotOther> To<TDotOther>() where TDotOther : struct, IDot
        {
            double value = ToDouble();
            return new Fixed<TBitWidth, TDotOther>(value);
        }
    }


    public static class ListExtension
    {
        public static TNumber SumAll<TNumber>(this List<TNumber> list) where TNumber : struct, IAdditionOperators<TNumber, TNumber, TNumber>
        {
            TNumber result = new();
            foreach (var item in list)
            {
                result += item;
            }
            return result;
        }
    }
}
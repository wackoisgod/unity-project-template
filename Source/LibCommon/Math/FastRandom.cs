using System;

namespace LibCommon.Math
{
    // based on SimSharp FastRandom
    // https://github.com/abeham/SimSharp/blob/691838d6c863b283c306df2a5c35a4c5e50158de/SimSharp/Random/FastRandom.cs
    public class FastRandom
    {
        static readonly FastRandom SeedRng = new FastRandom(Environment.TickCount);

        public struct RandomCache
        {
            public uint _x, _y, _z, _w;
        }

        const double REAL_UNIT_INT = 1.0/(int.MaxValue + 1.0);
        const double REAL_UNIT_UINT = 1.0/(uint.MaxValue + 1.0);
        const uint Y = 842502087, Z = 3579807591, W = 273326509;

        public uint _x;
        public uint _y;
        public uint _z;
        public uint _w;

        private RandomCache _cachedRandom;

        public FastRandom()
        {
            Reinitialise(SeedRng.NextInt());
        }

        public FastRandom(int seed)
        {
            Reinitialise(seed);
        }

        public void SetValues(uint x, uint y, uint z, uint w)
        {
            _x = x;
            _y = y;
            _z = z;
            _w = w;
        }

        public RandomCache GetCache()
        {
            return new RandomCache {_x = _x, _y = _y, _z = _z, _w = _w};
        }

        public void CacheValues()
        {
            _cachedRandom._x = _x;
            _cachedRandom._y = _y;
            _cachedRandom._z = _z;
            _cachedRandom._w = _w;
        }

        public void Restore()
        {
            _x = _cachedRandom._x;
            _y = _cachedRandom._y;
            _z = _cachedRandom._z;
            _w = _cachedRandom._w;
        }

        public void Reinitialise(int seed)
        {
            _x = (uint) ((seed*1431655781)
                         + (seed*1183186591)
                         + (seed*622729787)
                         + (seed*338294347));

            _y = Y;
            _z = Z;
            _w = W;
        }

        public int Next()
        {
            uint t = _x ^ (_x << 11);
            _x = _y;
            _y = _z;
            _z = _w;
            _w = (_w ^ (_w >> 19)) ^ (t ^ (t >> 8));

            uint rtn = _w & 0x7FFFFFFF;
            if (rtn == 0x7FFFFFFF)
            {
                return Next();
            }
            return (int) rtn;
        }

        public int Next(int upperBound)
        {
            if (upperBound < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(upperBound), upperBound, "upperBound must be >=0");
            }

            uint t = _x ^ (_x << 11);
            _x = _y;
            _y = _z;
            _z = _w;

            return (int) ((REAL_UNIT_INT*(int) (0x7FFFFFFF & (_w = (_w ^ (_w >> 19)) ^ (t ^ (t >> 8)))))*upperBound);
        }

        public int NextInt()
        {
            uint t = _x ^ (_x << 11);
            _x = _y;
            _y = _z;
            _z = _w;
            return (int) (0x7FFFFFFF & (_w = (_w ^ (_w >> 19)) ^ (t ^ (t >> 8))));
        }
    }
}

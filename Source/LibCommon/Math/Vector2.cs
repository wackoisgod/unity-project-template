using System;

namespace LibCommon.Math
{
    [Serializable]
    public struct Vector2
    {
        public float X;
        public float Y;

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float Length => (float) System.Math.Sqrt(LengthSquared);

        public float LengthSquared => X*X + Y*Y;
    }
}
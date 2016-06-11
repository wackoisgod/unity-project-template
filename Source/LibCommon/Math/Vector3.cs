namespace LibCommon.Math
{
    public struct Vector3
    {
        public float X;
        public float Y;
        public float Z;

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float Length => (float) System.Math.Sqrt(LengthSquared);

        public float LengthSquared => X*X + Y*Y + Z*Z;
    }
}


namespace LibCommon.Math
{
    public class Mathf
    {
        public const float E = (float) System.Math.E;
        public const float Log10E = 0.4342944819032f;
        public const float Log2E = 1.442695040888f;
        public const float Pi = (float) System.Math.PI;
        public const float PiOver2 = (float) (System.Math.PI/2.0);
        public const float PiOver4 = (float) (System.Math.PI/4.0);
        public const float TwoPi = (float) (System.Math.PI*2.0);

        public static float Lerp(float value1, float value2, float amount)
        {
            return value1 + (value2 - value1)*amount;
        }

        public static float Sqrt(float f)
        {
            return (float) System.Math.Sqrt(f);
        }
    }
}
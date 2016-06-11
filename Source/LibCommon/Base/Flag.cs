namespace LibCommon.Base
{
    public struct Flag
    {
        private readonly int _bits;

        public Flag(int bit)
        {
            _bits = bit;
        }

        public uint GetValue()
        {
            return (uint) 1 << _bits;
        }

        public bool IsValid()
        {
            return _bits != -1;
        }
    }

    public class Flags
    {
        private uint _value;

        public void Set(ref Flag flag)
        {
            if (!flag.IsValid()) return;

            _value |= flag.GetValue();
        }

        public void Clear(ref Flag flag)
        {
            if (!flag.IsValid()) return;

            _value &= ~flag.GetValue();
        }

        public void Clear()
        {
            _value = 0;
        }

        public bool Test(ref Flag flag)
        {
            if (!flag.IsValid()) return false;
            return (_value & flag.GetValue()) != 0;
        }

        public void Combine(ref Flags flags)
        {
            _value |= flags._value;
        }

        public static explicit operator Flags(uint values)
        {
            Flags f = new Flags {_value = values};

            return f;
        }
    }
}

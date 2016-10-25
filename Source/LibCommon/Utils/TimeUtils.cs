using System;

namespace LibCommon.Utils
{
  internal class TimeUtils
  {
    public static long UnixTimeNow()
    {
      TimeSpan timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
      return (long) timeSpan.TotalSeconds;
    }
  }
}

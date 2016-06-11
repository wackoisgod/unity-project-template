using System;
using System.Collections.Generic;
using System.Linq;

namespace LibCommon.Data
{
  public class DataUtils
  {
    private static Type[] _dataTypes;

    public static Type[] GetDataTypes()
    {
      return _dataTypes ?? (_dataTypes = GetSubclasses(typeof(BaseData)));
    }

    public static Type[] GetSubclasses(Type baseType)
    {
      IEnumerable<Type> subclasses = from assembly in AppDomain.CurrentDomain.GetAssemblies()
        from t in assembly.GetTypes()
        where t.IsSubclassOf(baseType)
        select t;
      return subclasses.ToArray();
    }
  }
}

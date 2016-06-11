using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LibCommon.Data;

namespace LibGameEditor.Data.Drawers
{
  [CustomDataDrawer(typeof(BaseData), true)]
  public class StandardDrawer : DataDrawer
  {
    private const uint DefaultDrawOrder = 1000;

    public override void Draw(object input, string name, PropertyInfo property, Action<object> setValueCallback)
    {
      PropertyInfo[] subProperties = input.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
      IEnumerable<PropertyInfo> sortedProperties = subProperties.OrderBy(prop =>
      {
        object[] attrs = prop.GetCustomAttributes(typeof(DisplayOrderAttribute), true);
        if (attrs.Length > 0)
        {
          return ((DisplayOrderAttribute) attrs[0]).Order;
        }
        return DefaultDrawOrder;
      });
      foreach (PropertyInfo subProperty in sortedProperties)
      {
        DrawProperty(subProperty, input);
      }
      setValueCallback(input);
    }
  }
}

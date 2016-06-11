using System;
using UnityEditor;

namespace LibGameEditor.Data.Drawers
{
  public class EnumDrawer : DataDrawer
  {
    public override void Draw(object input, string name, System.Reflection.PropertyInfo property,
      Action<object> setValueCallback)
    {
      Enum enumValue = (Enum) input;
      enumValue = EditorGUILayout.EnumPopup(name, enumValue);
      setValueCallback(enumValue);
    }
  }
}

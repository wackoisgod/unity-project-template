using System;
using System.Reflection;
using UnityEditor;

namespace LibGameEditor.Data.Drawers
{
  [CustomDataDrawer(typeof(int))]
  public class IntDrawer : DataDrawer
  {
    public override void Draw(object input, string name, PropertyInfo property, Action<object> setValueCallback)
    {
      int value = (int) input;
      value = EditorGUILayout.IntField(name, value);
      setValueCallback(value);
    }
  }
}

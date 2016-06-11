using System;
using System.Reflection;
using UnityEditor;

namespace LibGameEditor.Data.Drawers
{
  [CustomDataDrawer(typeof(string))]
  public class StringDrawer : DataDrawer
  {
    public override void Draw(object input, string name, PropertyInfo property, Action<object> setValueCallback)
    {
      string stringValue = input != null ? (string) input : "";
      stringValue = EditorGUILayout.TextField(name, stringValue);
      setValueCallback(stringValue);
    }
  }
}

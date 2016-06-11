using System;
using System.Reflection;
using UnityEditor;

namespace LibGameEditor.Data.Drawers
{
  public class DefaultDrawer : DataDrawer
  {
    public override void Draw(object input, string name, PropertyInfo property, Action<object> setValueCallback)
    {
      EditorGUILayout.LabelField(name, "No drawer implemented");
      setValueCallback(input);
    }
  }
}

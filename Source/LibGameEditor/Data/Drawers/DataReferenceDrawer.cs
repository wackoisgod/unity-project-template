using System;
using LibCommon.Data;
using UnityEditor;
using UnityEngine;

namespace LibGameEditor.Data.Drawers
{
  [CustomDataDrawer(typeof(DataReferenceAttribute))]
  public class DataReferenceDrawer : DataDrawer
  {
    public override void Draw(object input, string name, System.Reflection.PropertyInfo property,
      Action<object> setValueCallback)
    {
      DataEditorCache.DataInfo[] data = DataEditorCache.Instance.Data;
      int intValue = (int) input;
      string dataName = "Unassigned";
      for (int i = 0; i < data.Length; i++)
      {
        if (data[i].Id == intValue)
        {
          dataName = data[i].Name;
        }
      }
      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.LabelField(name);
      EditorGUILayout.LabelField(dataName);
      if (GUILayout.Button("Change", GUILayout.ExpandWidth(false)))
      {
        DataFieldAssignWindow.AssignValue((assignedData, path) => { setValueCallback(assignedData.Id); }, property.Name);
      }
      EditorGUILayout.EndHorizontal();
      setValueCallback(intValue);
    }
  }
}

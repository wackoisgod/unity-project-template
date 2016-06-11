using LibCommon.Data;
using UnityEngine;

namespace LibGameEditor.Data
{
  public class DataFieldAssignWindow : DataListWindow
  {
    private string _fieldName;

    public static void AssignValue(DataListCallback callback, string fieldName)
    {
      DataFieldAssignWindow assignWindow = GetWindow<DataFieldAssignWindow>();
      assignWindow.Callback = (data, path) =>
      {
        callback(data, path);
        assignWindow.Close();
      };
      assignWindow.Show();
      assignWindow._fieldName = fieldName;
      assignWindow.Expanded = new bool[DataUtils.GetDataTypes().Length];
    }

    protected override void OnGUI()
    {
      if (Callback == null)
      {
        Close();
        return;
      }
      GUILayout.Label("Assign Value to: " + _fieldName);
      base.OnGUI();
    }
  }
}

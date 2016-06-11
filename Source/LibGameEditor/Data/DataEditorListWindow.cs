using LibCommon.Data;
using UnityEditor;

namespace LibGameEditor.Data
{
  public class DataEditorListWindow : DataListWindow
  {
    [MenuItem("Game Data/Edit")]
    public static void Launch()
    {
      DataEditorListWindow window = GetWindow<DataEditorListWindow>();
      window.Show();
      window.Expanded = new bool[DataUtils.GetDataTypes().Length];
    }

    protected override void OnGUI()
    {
      if (Callback == null)
      {
        Callback = DataEditWindow.EditData;
      }
      base.OnGUI();
    }
  }
}

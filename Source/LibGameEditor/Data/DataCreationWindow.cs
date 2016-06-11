using System;
using System.IO;
using System.Linq;
using LibCommon.Data;
using UnityEditor;
using UnityEngine;

namespace LibGameEditor.Data
{
  public class DataCreationWindow : EditorWindow
  {
    private const string DefaultDataName = "Data";
    private string _dataPath;
    private BaseData _data;
    private DataListWindow.DataListCallback _callback;

    private string FullPath => Path.DirectorySeparatorChar + _dataPath + ".xml";

    public static void Create(Type dataType, DataListWindow.DataListCallback callback)
    {
      DataCreationWindow window = GetWindow<DataCreationWindow>();
      if (dataType.IsSubclassOf(typeof(BaseData)))
      {
        window._data = Activator.CreateInstance(dataType) as BaseData;
        int id;
        bool uniqueId = true;
        do
        {
          id = UnityEngine.Random.Range(1, int.MaxValue);
          if (DataEditorCache.Instance.Data.Any(info => info.Id == id))
          {
            uniqueId = false;
          }
        } while (!uniqueId);

        if (window._data != null)
        {
          window._data.Id = id;
          window._data.Name = DefaultDataName;
        }
        window._dataPath = "Data" + Path.DirectorySeparatorChar + dataType.Name + Path.DirectorySeparatorChar +
                          DefaultDataName;
        window._callback = callback;
      }
      window.Show();
    }

    private void OnGUI()
    {
      if (_data == null)
      {
        Close();
        return;
      }
      if (GUILayout.Button("Create Data"))
      {
        string directoryPath = Path.GetDirectoryName(FullPath);
        if (directoryPath != null && !Directory.Exists(directoryPath))
        {
          Directory.CreateDirectory(directoryPath);
        }
        Serializer.Serialize(_data.GetType(), _data, FullPath);
        AssetDatabase.Refresh();
        _callback(_data, FullPath);
        Close();
        return;
      }
      EditorGUILayout.SelectableLabel("ID: " + _data.Id);
      bool autoPath = _dataPath.EndsWith(_data.Name);
      _data.Name = EditorGUILayout.TextField("Name:", _data.Name);
      if (autoPath)
      {
        _dataPath = Path.GetDirectoryName(_dataPath) + Path.DirectorySeparatorChar + _data.Name;
      }

      _dataPath = EditorGUILayout.TextField("Path:", _dataPath);
    }
  }
}

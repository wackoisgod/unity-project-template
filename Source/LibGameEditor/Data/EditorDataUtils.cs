using System;
using System.IO;
using System.Linq;
using LibCommon.Data;
using UnityEditor;

namespace LibGameEditor.Data
{
  public class EditorDataUtil : AssetPostprocessor
  {
    [MenuItem("Game Data/Update Files")]
    public static void UpdateData()
    {
      DataInfo[] info = GetAllData();
      for (int i = 0; i < info.Length; i++)
      {
        Serializer.Serialize(info[i].Data.GetType(), info[i].Data, info[i].Path);
      }
    }

    public class DataInfo
    {
      public string Path;
      public BaseData Data;

      public DataInfo(BaseData inputData, string inputPath)
      {
        Path = inputPath;
        Data = inputData;
      }
    }

    private static DataInfo[] _dataCache;

    public static DataInfo[] GetAllData()
    {
      if (_dataCache != null) return _dataCache;

      string[] xmlFiles = Directory.GetFiles(UnityEngine.Application.dataPath, "*.xml", SearchOption.AllDirectories);
      _dataCache = (from t in DataUtils.GetDataTypes()
        from file in xmlFiles
        where Serializer.CanDeserialize(t, file)
        let data = Serializer.Deserialize(t, file) as BaseData
        select new DataInfo(data, file)).ToArray();
      return _dataCache;
    }

    public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
      string[] movedFromAssets)
    {
      _dataCache = null;
    }

    public static void ProcessAcrossAllData(Action<DataInfo> dataCallback)
    {
      DataInfo[] data = GetAllData();
      for (int i = 0; i < data.Length; i++)
      {
        dataCallback(data[i]);
      }
    }

    public static DataInfo GetData(int id)
    {
      DataInfo[] data = GetAllData();
      return data.FirstOrDefault(t => t.Data.Id == id);
    }
  }
}

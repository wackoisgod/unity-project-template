using System;
using System.IO;
using LibCommon.Data;
using LibGameClient.Data;

namespace LibGameEditor.Data
{
  public class DataCombiner
  {
    private static CombinedData _combined;

    private static string _cachedLocalPath;

    private static string LocalCombinedPath
    {
      get
      {
        if (string.IsNullOrEmpty(_cachedLocalPath))
        {
          _cachedLocalPath = ClientDataLoader.DataPath.Replace(UnityEngine.Application.dataPath, "Assets");
        }
        return _cachedLocalPath;
      }
    }

    public static void StartEdit()
    {
      if (File.Exists(ClientDataLoader.DataPath)
          && Serializer.CanDeserialize(typeof(CombinedData), ClientDataLoader.DataPath))
      {
        _combined = Serializer.Deserialize<CombinedData>(ClientDataLoader.DataPath);
      }
      else
      {
        _combined = FullBuild();
      }
    }

    public static void EndEdit()
    {
      Serializer.Serialize(typeof(CombinedData), _combined, ClientDataLoader.DataPath);
    }


    public static void RebuildCombinedData()
    {
      CombinedData output = FullBuild();
      Serializer.Serialize(typeof(CombinedData), output, ClientDataLoader.DataPath);
    }

    public static void RemoveData(int id)
    {
      if (_combined == null) return;

      for (int i = 0; i < _combined.Data.Length; i++)
      {
        if (_combined.Data[i].Id != id) continue;

        for (int j = i; j < _combined.Data.Length - 1; j++)
        {
          _combined.Data[j] = _combined.Data[j + 1];
        }
        BaseData[] data = _combined.Data;
        Array.Resize(ref data, _combined.Data.Length - 1);
        _combined.Data = data;
        break;
      }
    }

    public static void Add(BaseData data)
    {
      if (_combined == null) return;

      for (int i = 0; i < _combined.Data.Length; i++)
      {
        if (_combined.Data[i].Id != data.Id) continue;

        _combined.Data[i] = data;
        return;
      }
      BaseData[] dataArr = _combined.Data;
      Array.Resize(ref dataArr, dataArr.Length + 1);
      dataArr[dataArr.Length - 1] = data;
      _combined.Data = dataArr;
    }

    private static CombinedData FullBuild()
    {
      EditorDataUtil.DataInfo[] dataInfo = EditorDataUtil.GetAllData();
      CombinedData output = new CombinedData {Data = new BaseData[dataInfo.Length]};

      for (int i = 0; i < dataInfo.Length; i++)
      {
        output.Data[i] = dataInfo[i].Data;
      }
      return output;
    }
  }
}

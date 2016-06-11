using System;
using LibCommon.Data;

namespace LibGameEditor.Data
{
  public class DataEditorCache
  {
    public class DataInfo
    {
      public string Name;
      public string FilePath;
      public int Id;
      public string Type;
    }

    private static bool _dirty = true;
    private static DataEditorCache _instance;

    public static DataEditorCache Instance
    {
      get
      {
        if (_instance == null
            || _dirty)
        {
          _instance = Load();
          _dirty = false;
        }
        return _instance;
      }
    }

    public static void MarkDirty()
    {
      _dirty = true;
    }

    public DataInfo[] Data;

    private const string CacheLocation = "/Editor/DataCache.xml";

    public static string FullCachePath => UnityEngine.Application.dataPath + CacheLocation;

    public static string UnityCachePath => "Assets" + CacheLocation;

    public void Save()
    {
      Serializer.Serialize(this, FullCachePath);
    }

    public static DataEditorCache Load()
    {
      try
      {
        return Serializer.Deserialize<DataEditorCache>(FullCachePath);
      }
      catch (Exception)
      {
        DataEditorCache xyz = new DataEditorCache {Data = new DataInfo[0]};

        return xyz;
      }
    }

    public void Add(DataInfo info)
    {
      for (int i = 0; i < Data.Length; i++)
      {
        if (Data[i].Id == info.Id)
        {
          Data[i] = info;
          return;
        }
      }
      Array.Resize(ref Data, Data.Length + 1);
      Data[Data.Length - 1] = info;
    }

    public void Remove(string path)
    {
      for (int i = 0; i < Data.Length; i++)
      {
        if (Data[i].FilePath == path)
        {
          for (int j = i + 1; j < Data.Length; j++)
          {
            Data[j - 1] = Data[j];
          }
          Array.Resize(ref Data, Data.Length - 1);
          return;
        }
      }
    }

    public int GetId(string path)
    {
      for (int i = 0; i < Data.Length; i++)
      {
        if (Data[i].FilePath == path)
        {
          return Data[i].Id;
        }
      }
      return -1;
    }
  }
}

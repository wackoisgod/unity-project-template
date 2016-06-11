using System;
using System.Collections.Generic;
using System.Linq;
using LibCommon.Data;
using UnityEditor;

namespace LibGameEditor.Data
{
  public class DataEditorCacheCreator : AssetPostprocessor
  {
    private static bool _updatingFile;

    public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
      string[] movedFromAssets)
    {
      DataEditorCache.MarkDirty();
      for (int i = 0; i < importedAssets.Length; i++)
      {
        UnityEngine.Debug.Log(importedAssets[i]);
      }
      if (_updatingFile)
      {
        _updatingFile = false;
      }
      else
      {
        if (Array.IndexOf(importedAssets, DataEditorCache.UnityCachePath) >= 0
            || Array.IndexOf(deletedAssets, DataEditorCache.UnityCachePath) >= 0
            || Array.IndexOf(movedAssets, DataEditorCache.UnityCachePath) >= 0
            || Array.IndexOf(movedFromAssets, DataEditorCache.UnityCachePath) >= 0
            || !System.IO.File.Exists(DataEditorCache.FullCachePath))
        {
          //Full rebuild
          _updatingFile = true;
          DataEditorCache cache = new DataEditorCache();
          EditorDataUtil.DataInfo[] info = EditorDataUtil.GetAllData();
          cache.Data = new DataEditorCache.DataInfo[info.Length];
          DataCombiner.StartEdit();
          for (int i = 0; i < info.Length; i++)
          {
            DataEditorCache.DataInfo dataInfo = new DataEditorCache.DataInfo
            {
              FilePath = ShortenFilePath(info[i].Path),
              Id = info[i].Data.Id,
              Name = info[i].Data.Name,
              Type = info[i].Data.GetType().ToString()
            };
            cache.Data[i] = dataInfo;
          }
          Save(cache);
          DataCombiner.EndEdit();
          AssetDatabase.Refresh();
        }

        else
        {
          DataCombiner.StartEdit();
          DataEditorCache cache = DataEditorCache.Load();

          IEnumerable<string> xml = from asset in deletedAssets
            where asset.EndsWith("xml")
            select asset;
          foreach (string file in xml)
          {
            string shortPath = ShortenUnityPath(file);
            int id = cache.GetId(shortPath);
            DataCombiner.RemoveData(id);
            cache.Remove(shortPath);
          }

          xml = from asset in movedFromAssets
            where asset.EndsWith("xml")
            select asset;
          foreach (string file in xml)
          {
            string shortPath = ShortenUnityPath(file);
            int id = cache.GetId(shortPath);
            DataCombiner.RemoveData(id);
            cache.Remove(shortPath);
          }

          xml = from asset in importedAssets
            where asset.EndsWith("xml")
            select asset;
          foreach (string file in xml)
          {
            Type t = GetSerializedType(file);
            if (t == null) continue;

            BaseData data = Serializer.Deserialize(t, file) as BaseData;
            DataEditorCache.DataInfo dataInfo = new DataEditorCache.DataInfo {FilePath = ShortenUnityPath(file)};
            if (data == null) continue;

            dataInfo.Id = data.Id;
            dataInfo.Name = data.Name;
            dataInfo.Type = t.ToString();
            cache.Add(dataInfo);
            DataCombiner.Add(data);
          }

          xml = from asset in movedAssets
            where asset.EndsWith("xml")
            select asset;
          foreach (string file in xml)
          {
            Type t = GetSerializedType(file);
            if (t == null) continue;

            BaseData data = Serializer.Deserialize(t, file) as BaseData;
            if (data != null)
            {
              DataEditorCache.DataInfo dataInfo = new DataEditorCache.DataInfo
              {
                FilePath = ShortenUnityPath(file),
                Id = data.Id,
                Name = data.Name,
                Type = t.ToString()
              };
              cache.Add(dataInfo);
            }
            DataCombiner.Add(data);
          }

          byte[] oldBytes = null;
          if (System.IO.File.Exists(DataEditorCache.FullCachePath))
          {
            using (
              System.IO.FileStream fs = new System.IO.FileStream(DataEditorCache.FullCachePath, System.IO.FileMode.Open)
              )
            {
              oldBytes = new byte[fs.Length];
              fs.Read(oldBytes, 0, (int) fs.Length);
            }
          }
          else
          {
            _updatingFile = true;
          }
          Save(cache);
          if (oldBytes != null)
          {
            using (
              System.IO.FileStream fs = new System.IO.FileStream(DataEditorCache.FullCachePath, System.IO.FileMode.Open)
              )
            {
              byte[] newBytes = new byte[fs.Length];
              fs.Read(newBytes, 0, (int) fs.Length);

              if (oldBytes.Length != newBytes.Length)
              {
                _updatingFile = true;
              }
              for (int i = 0; i < newBytes.Length; i++)
              {
                if (oldBytes[i] != newBytes[i])
                {
                  _updatingFile = true;
                }
                if (_updatingFile)
                {
                  break;
                }
              }
            }
          }

          DataCombiner.EndEdit();
          AssetDatabase.Refresh();
        }
      }
      DataEditorCache.MarkDirty();
    }

    private static void Save(DataEditorCache cache)
    {
      cache.Save();
    }

    private static Type GetSerializedType(string xml)
    {
      foreach (Type t in DataUtils.GetDataTypes())
      {
        if (Serializer.CanDeserialize(t, xml))
        {
          return t;
        }
      }
      return null;
    }

    private static string ShortenFilePath(string fullpath)
    {
      return fullpath.Substring(UnityEngine.Application.dataPath.Length);
    }

    private static string ShortenUnityPath(string unityPath)
    {
      return unityPath.Substring(6);
    }
  }
}

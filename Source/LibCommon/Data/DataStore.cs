using System;
using System.Collections.Generic;
using System.Linq;

namespace LibCommon.Data
{
  public class DataStore
  {
    private static DataStore _instance;
    private static DataStore Instance => _instance ?? (_instance = new DataStore());

    private readonly Dictionary<int, BaseData> _data = new Dictionary<int, BaseData>();

    public static T GetData<T>(int id) where T : BaseData
    {
      BaseData output;
      if (Instance._data.TryGetValue(id, out output))
      {
        return output as T;
      }
      return null;
    }

    public static void AddData(BaseData addedData)
    {
      Instance._data.Add(addedData.Id, addedData);
    }

    public static void AddData(BaseData[] datas)
    {
      foreach (BaseData data in datas)
      {
        AddData(data);
      }
    }

    public static int[] GetAllIds(Type dataType)
    {
      var ids = from data in Instance._data.Values
        where data.GetType() == dataType
        select data.Id;
      return ids.ToArray();
    }
  }
}

namespace LibCommon.Data
{
  public class CombinedData
  {
    [Polymorphic]
    public BaseData[] Data { get; set; }
  }
}

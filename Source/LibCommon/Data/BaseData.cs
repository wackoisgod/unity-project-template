namespace LibCommon.Data
{
  public class BaseData
  {
    [DisplayOrder(1)]
    public string Name { get; set; }

    [Readonly]
    [DisplayOrder(0)]
    public int Id { get; set; }
  }
}

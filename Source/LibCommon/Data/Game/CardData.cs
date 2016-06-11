namespace LibCommon.Data.Game
{
  public class CardData : BaseData
  {
    [AssetReference(AssetReferenceAttribute.AssetReferenceType.Texture)]
    public string Icon { get; set; }

    public string DisplayName { get; set; }

    public float SpawnTime { get; set; }

    public int SpawnCount { get; set; }

    [DataReference(typeof(Creature))]
    public int Creature { get; set; }
  }
}

namespace LibCommon.Data.Game
{
  public class Creature : BaseData
  {
    [AssetReference(AssetType = AssetReferenceAttribute.AssetReferenceType.Texture, BundleOverride = "Common")]
    public string Icon { get; set; }

    [AssetReference(AssetType = AssetReferenceAttribute.AssetReferenceType.Texture, DependentBundle = "Common")]
    public string TeamIcon { get; set; }

    public string DisplayName { get; set; }
  }
}
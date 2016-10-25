namespace LibCommon.Data
{
  public class AssetReferenceAttribute : DataDrawAttribute
  {
    public enum AssetReferenceType
    {
      Texture,
      GameObject,
      AudioClip,
      Material,
      Animation
    }

    public AssetReferenceType AssetType { get; set; }
    public string BundleOverride { get; set; }
    public string DependentBundle { get; set; }

    public AssetReferenceAttribute()
    {
      BundleOverride = string.Empty;
      DependentBundle = string.Empty;
    }
  }
}
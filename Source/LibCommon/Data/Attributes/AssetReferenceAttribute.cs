using System;

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
      BundleOverride = String.Empty;
      DependentBundle = String.Empty;
    }
  }
}

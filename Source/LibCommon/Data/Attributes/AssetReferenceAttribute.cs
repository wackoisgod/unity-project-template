using System;

namespace LibCommon.Data
{
  public class BundleReferenceAttribute : Attribute
  {
    public string BundleName;

    public BundleReferenceAttribute(string name)
    {
      BundleName = name;
    }
  }

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

    public AssetReferenceType AssetType;

    public AssetReferenceAttribute(AssetReferenceType assetType)
    {
      AssetType = assetType;
    }
  }
}

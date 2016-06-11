namespace LibCommon.Assets
{
  public class AssetManifest
  {
    public class BundleInfo
    {
      public string name;
      public string hash;
      public string[] guids;
      public string[] assetNames;
      public string[] dependencies;
    }

    public int version;
    public BundleInfo[] bundles;
  }
}

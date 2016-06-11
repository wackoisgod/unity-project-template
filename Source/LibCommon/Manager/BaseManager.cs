namespace LibCommon.Manager
{
  public abstract class BaseManager
  {
    public abstract void Init();

    public virtual void Begin()
    {
    }

    public virtual void Destroy()
    {
    }

    public virtual void Update(float time, float deltaTime)
    {
    }

    public string Name { get; internal set; }
  }
}

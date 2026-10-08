public interface ISaveRegistry
{
    void Register(ISaveable saveable);
    void Unregister(ISaveable saveable);
}
 

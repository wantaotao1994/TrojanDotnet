namespace Winter
{
    public interface IProxySetting
    {
      void  SetPacProxy(string data);
        
      void  SetGlobalProxy(string data);


      void RemoveSetting();

    }
}
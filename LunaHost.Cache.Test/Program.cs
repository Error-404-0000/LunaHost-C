namespace LunaHost.Cache.Test
{
 
    public class Program
    {
        public int CacheCode { get; set; }
        public int _add {  get; set; }
        public Program Add(int x1,int x2)=> new() { _add = x1 + x2 };
        static void Main(string[] args)
        {
     
        }
    }
}

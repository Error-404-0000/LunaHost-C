namespace LunaHost.Cache.Test
{
 
    public class Program:ICacheable
    {
        public int CacheCode { get; set; }
        public int _add {  get; set; }
        public Program Add(int x1,int x2)=> new() { _add = x1 + x2 };
        static void Main(string[] args)
        {
            Cache<Program> CacheTest = new(capacity:10);
            var Program = new Program();
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine("1 + ( " + i + '*' +i+')' +'='+CacheTest.Invoke(Program.Add, 1, i*i)._add);//3
                Console.WriteLine("1 + "+i+ '='+ CacheTest.Invoke(Program.Add, 1, i)._add);//3 
            }

        }
    }
}

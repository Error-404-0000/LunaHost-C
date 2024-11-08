using LunaHost_Test;
using System;
using System.Diagnostics;



public class MyCacheableObject : ICacheable
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int CacheCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Description { get; set; }
    public double Price { get; set; }
    public int Quantity { get; set; }
    public bool IsActive { get; set; }
    public byte[] LargeData { get; set; } = new byte[1024 * 1024]; // 1 MB data for testing
}


public static class Test
{
    public static void Run()
    {
        Cache<MyCacheableObject> cache = new Cache<MyCacheableObject>(20);

        //Console.WriteLine("Running normal C# instantiation test...");
        //MeasureStandardInstantiation();
        GC.Collect();

        Console.WriteLine("\nRunning ref caching test...");
        MeasureRefCaching(cache);
        GC.Collect();

        Console.WriteLine("---------");
        MeasureStandardInstantiation();
       
        Console.ReadLine();

    }

    private static void MeasureStandardInstantiation()
    {
        Stopwatch stopwatch = new Stopwatch();
        long initialMemory = GC.GetTotalMemory(true);

        stopwatch.Start();
        for (int i = 0; i < 100000; i++)
        {
            var obj = new MyCacheableObject
            {
                Id = i,
                Name = "Test",
               
                CreatedAt = DateTime.Now,
                Description = "Sample Description",
                Price = 99.99,
                Quantity = 5,
                IsActive = true
            };

        }
        stopwatch.Stop();

        long finalMemory = GC.GetTotalMemory(true);
        Console.WriteLine("Standard Instantiation:");
        Console.WriteLine("Total memory used (bytes): " + (finalMemory - initialMemory));
        Console.WriteLine("Total time taken (ms): " + stopwatch.ElapsedMilliseconds);
    }

    private static void MeasureRefCaching(Cache<MyCacheableObject> cache)
    {
        Stopwatch stopwatch = new Stopwatch();
        long initialMemory = GC.GetTotalMemory(true);

        stopwatch.Start();
        for (int i = 0; i < 100000; i++)
        {
            MyCacheableObject obj = new MyCacheableObject
            {
                Id = 0,
                Name = "Test",
                CreatedAt = DateTime.Now,
                Description = "Sample Description",
                Price = 99.99,
                Quantity = 5,
                IsActive = true
            };

            cache.New(ref obj);

        }
        stopwatch.Stop();

        long finalMemory = GC.GetTotalMemory(true);
        Console.WriteLine("Ref Caching:");
        Console.WriteLine("Total memory used (bytes): " + (finalMemory - initialMemory));
        Console.WriteLine("Total time taken (ms): " + stopwatch.ElapsedMilliseconds);
        Console.ReadKey();
    }

}
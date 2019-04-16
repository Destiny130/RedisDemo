using RedisDemo.RedisHelp;
using RedisDemo.SimpleTest;
using System;

namespace RedisDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            string singleConnStr = Util.ReadConnectionString("RedisSingle", "127.0.0.1:6300");
            RedisHelper clusterRedis = new RedisHelper();

            BasicUse basic = new BasicUse(singleConnStr);
            basic.ExecuteBatchTest();
            basic.ExecutePrefixTest();

            Console.WriteLine("\nend");
            Console.ReadKey();
        }
    }
}

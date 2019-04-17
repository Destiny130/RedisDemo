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
            basic.ExecuteBatchTest();  //Will connect to the single redis
            basic.ExecutePrefixTest();  //Will connect to the cluster
            basic.TransactionExecuteTest();  //Will connect to the single redis

            Console.WriteLine("\nend");
            Console.ReadKey();
        }
    }
}

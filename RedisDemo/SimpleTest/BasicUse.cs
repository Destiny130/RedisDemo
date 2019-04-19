using Newtonsoft.Json;
using RedisDemo.RedisHelp;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace RedisDemo.SimpleTest
{
    public class BasicUse
    {
        private readonly string redisConnStr;

        public BasicUse(string _redisConnStr)
        {
            redisConnStr = _redisConnStr;
        }

        public void BatchTest()
        {
            Stopwatch watch = new Stopwatch();
            int count = 30;

            watch.Start();
            RedisHelper redisConn0 = new RedisHelper(0, redisConnStr);
            IDatabase db0 = redisConn0.GetDatabase();
            var batch = db0.CreateBatch();

            //List<Task<long>> batchTaskList = 
            Enumerable.Range(0, count).Select(i => batch.ListRightPushAsync("Person", JsonConvert.SerializeObject(new Person(i, i.ToString() + " first", i.ToString() + " last", i.ToString() + " address", i.ToString() + " phone")))).Count();

            batch.Execute();
            //watch.Stop();
            //Console.WriteLine($"Batch.Execute() spent: {watch.ElapsedMilliseconds:F3} milliseconds");
            //watch.Restart();
            //await Task.WhenAll(batchTaskList);  //Why this stop spent some time? Is it check something?
            //long sum = batchTaskList.Where(b => b.IsCompleted).Select(b => b.Result).Sum();
            watch.Stop();
            //Console.WriteLine($"Task.WhenAll() spent: {watch.ElapsedMilliseconds:F3} milliseconds");
            Console.WriteLine($"Batch spent: {watch.ElapsedMilliseconds:F3} milliseconds");
            //Console.WriteLine($"{String.Join(", ", batchTaskList.Where(e => e.IsCompleted).Select(e => e.Result))}");

            watch.Restart();
            RedisHelper redisConn1 = new RedisHelper(1, redisConnStr);
            Enumerable.Range(0, count).Select(i => redisConn1.ListRightPush("Person", JsonConvert.SerializeObject(new Person(i, i.ToString() + " first", i.ToString() + " last", i.ToString() + " address", i.ToString() + " phone")))).Count();

            //IDatabase db1 = redisConn1.GetDatabase();
            //Enumerable.Range(0, count).Select(i => db1.ListRightPush("Person", JsonConvert.SerializeObject(new Person(i, i.ToString() + " first", i.ToString() + " last", i.ToString() + " address", i.ToString() + " phone")))).Count();
            watch.Stop();
            Console.WriteLine($"Normal spent: {watch.ElapsedMilliseconds:F3} milliseconds");
            Console.WriteLine();
        }

        public void PrefixTest()
        {
            RedisHelper redis = new RedisHelper();  //Will connect to the cluster

            List<string> keyList = new List<string>() { "a", "b", "c", "d" };
            List<bool> resultList = new List<bool>();
            Func<string, string> func = str => str + str + str;
            Func<List<bool>, string> printFunc = list =>
            {
                string result = String.Join(", ", list);
                list.Clear();
                return result;
            };

            keyList.ForEach(key => resultList.Add(redis.StringSet(key, func(key))));
            Console.WriteLine(printFunc(resultList));

            redis.CustomKey = "{a}";
            keyList.ForEach(key => resultList.Add(redis.StringSet(key, func(key))));
            Console.WriteLine(printFunc(resultList));

            redis.CustomKey = "{{a}b}";
            keyList.ForEach(key => resultList.Add(redis.StringSet(key, func(key))));
            Console.WriteLine(printFunc(resultList));

            redis.CustomKey = "{a}{b}";
            keyList.ForEach(key => resultList.Add(redis.StringSet(key, func(key))));
            Console.WriteLine(printFunc(resultList));

            redis.CustomKey = "{{a}{b}}";
            keyList.ForEach(key => resultList.Add(redis.StringSet(key, func(key))));
            Console.WriteLine(printFunc(resultList));
            Console.WriteLine();
        }

        public void TransactionTest()
        {
            RedisHelper redis = new RedisHelper(0, redisConnStr);
            ITransaction tran = redis.CreateTransaction();
            tran.StringSetAsync("trans", "a");
            Console.WriteLine($"Try to get trans: {redis.StringGet("trans")}");
            Console.WriteLine($"Transaction: {tran.Execute()}");
            Console.WriteLine($"Try to get trans: {redis.StringGet("trans")}");
            Console.WriteLine();
        }

        public void SubscribeTest()
        {
            RedisHelper redis = new RedisHelper(0, redisConnStr);
            //List<Task> list = new List<Task>()
            //{
            //    Task.Run(() => Application.Run(new SubscribeForm(redis))),
            //    Task.Run(() => Application.Run(new SubscribeForm(new RedisHelper(0,redisConnStr))))
            //};
            //Task.WhenAll(list);
            //Console.WriteLine("SubscribeTest end");
            Application.Run(new SubscribeForm(redis));  //same connection can subscribe one channel once only
        }
    }
}

using Newtonsoft.Json;
using RedisDemo.RedisHelp;
using StackExchange.Redis;
using System;
using System.Diagnostics;
using System.Linq;

namespace RedisDemo.SimpleTest
{
    public class BasicUse
    {
        private readonly string redisConnStr;

        public BasicUse(string _redisConnStr) => redisConnStr = _redisConnStr;

        public void ExecuteBatch()
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
            IDatabase db1 = redisConn1.GetDatabase();
            Enumerable.Range(0, count).Select(i => db1.ListRightPush("Person", JsonConvert.SerializeObject(new Person(i, i.ToString() + " first", i.ToString() + " last", i.ToString() + " address", i.ToString() + " phone")))).Count();
            watch.Stop();
            Console.WriteLine($"Normal spent: {watch.ElapsedMilliseconds:F3} milliseconds");
        }
    }

    class Person
    {
        public int Id;
        public string FirstName;
        public string LastName;
        public string Address;
        public string Phone;

        public Person()
        {

        }

        public Person(int id, string firstName, string lastName, string address, string phone)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Address = address;
            Phone = phone;
        }
    }
}

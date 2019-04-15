using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Newtonsoft.Json;
using RedisDemo.RedisHelp;
using System.Threading.Tasks;

namespace RedisDemo.SimpleTest
{
    public class BasicUse
    {
        private readonly RedisHelper redisConn;

        public BasicUse(RedisHelper _redisConn)
        {
            this.redisConn = _redisConn;
        }

        public void ExecuteBatch()
        {
            Stopwatch watch = new Stopwatch();
            int count = 30;

            watch.Start();
            var db0 = redisConn.GetDatabase();
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
            var db1 = redisConn.GetDatabase(1);
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
            this.Id = id;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Address = address;
            this.Phone = phone;
        }
    }
}

using System;
using System.Linq;
using System.Diagnostics;
using StackExchange.Redis;
using Newtonsoft.Json;
using RedisDemo.RedisHelp;
using System.Configuration;

namespace RedisDemo.SimpleTest
{
    public class BasicUse
    {
        public void Execute()
        {
            Stopwatch watch = new Stopwatch();
            string config = ConfigurationManager.ConnectionStrings["RedisSingle"].ConnectionString;
            var conn = RedisConnectionHelp.GetConnectionMultiplexer(config);
            int count = 50000;

            watch.Start();
            var db0 = conn.GetDatabase();
            var batch = db0.CreateBatch();
            for (int i = 0; i < count; ++i)
            {
                batch.ListRightPushAsync("Person", JsonConvert.SerializeObject(new Person(i, i.ToString() + " first", i.ToString() + " last", i.ToString() + " address", i.ToString() + " phone")));
            }

            Enumerable.Range(0, count).Select(i => batch.ListRightPushAsync("Person", JsonConvert.SerializeObject(new Person(i, i.ToString() + " first", i.ToString() + " last", i.ToString() + " address", i.ToString() + " phone"))));

            //var t = Enumerable.Range(0, 200000).Select(i => batch.ListRightPushAsync("Person", JsonConvert.SerializeObject(new Person(i, i.ToString() + " first", i.ToString() + " last", i.ToString() + " address", i.ToString() + " phone"))));
            batch.Execute();
            watch.Stop();
            Console.WriteLine($"Batch spent: {watch.ElapsedMilliseconds:F3} milliseconds");

            watch.Restart();
            var db1 = conn.GetDatabase(1);
            for (int i = 0; i < count; ++i)
            {
                db1.ListRightPush("Person", JsonConvert.SerializeObject(new Person(i, i.ToString() + " first", i.ToString() + " last", i.ToString() + " address", i.ToString() + " phone")));
            }
            //Enumerable.Range(0, 200000).Select(i => db1.ListRightPush("Person", JsonConvert.SerializeObject(new Person(i, i.ToString() + " first", i.ToString() + " last", i.ToString() + " address", i.ToString() + " phone"))));
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

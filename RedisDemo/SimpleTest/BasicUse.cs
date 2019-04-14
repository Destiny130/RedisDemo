using System;
using System.Diagnostics;
using StackExchange.Redis;
using Newtonsoft.Json;

namespace RedisDemo.SimpleTest
{
    public class BasicUse
    {
        public void Execute()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            var conn = ConnectionMultiplexer.Connect("127.0.0.1:6300");
            var db = conn.GetDatabase();

            var ba = db.CreateBatch();

            for (int i = 100001; i < 1000000; ++i)
            {
                Person person = new Person(i, i.ToString() + " first", i.ToString() + " last", i.ToString(), i.ToString());
                ba.ListRightPushAsync("Person", JsonConvert.SerializeObject(person));
            }
            ba.Execute();
            watch.Stop();
            Console.WriteLine($"Spent: {watch.ElapsedMilliseconds:F3} milliseconds");
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

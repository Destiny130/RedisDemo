using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace RedisDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var conn = ConnectionMultiplexer.Connect("127.0.0.1:6370");
            var db = conn.GetDatabase();
            //db.StringSet("b", "abcdde");
            //db.StringSet("a", "abb");

            //var conn = ConnectionMultiplexer.Connect("127.0.0.1:6391");
            //var db = conn.GetDatabase();
            //Console.WriteLine(db.StringGet("a"));
            //Console.WriteLine(db.StringGet("b"));
            //Console.WriteLine(db.KeyDelete("a"));

            Console.WriteLine("\nend");
            Console.ReadKey();
        }
    }
}

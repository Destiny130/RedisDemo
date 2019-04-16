using StackExchange.Redis;
using System;
using System.Collections.Concurrent;

namespace RedisDemo.RedisHelp
{
    public static class RedisConnectionHelp
    {
        private static readonly string redisConnectionString = Util.ReadConnectionString("RedisCluster", "127.0.0.1:6370");
        private static readonly object locker = new object();
        private static ConnectionMultiplexer instance;
        private static readonly ConcurrentDictionary<string, ConnectionMultiplexer> connectionCache = new ConcurrentDictionary<string, ConnectionMultiplexer>();
        private static readonly string sysCustomKey = Util.ReadAppSetting("RedisKey", String.Empty);

        public static string SysCustomKey => sysCustomKey;

        public static ConnectionMultiplexer Instance
        {
            get
            {
                if (instance == null || !instance.IsConnected)
                {
                    lock (locker)
                    {
                        if (instance == null || !instance.IsConnected)
                        {
                            //if (!connectionCache.ContainsKey(redisConnectionString) || !connectionCache[redisConnectionString].IsConnected)
                            //{
                            //    connectionCache[redisConnectionString] = GetManager(redisConnectionString);
                            //    instance = connectionCache[redisConnectionString];
                            //}
                            //else
                            //{
                            //    instance = connectionCache[redisConnectionString];
                            //}
                            instance = GetManager();
                        }
                    }
                }
                return instance;
            }
        }

        public static ConnectionMultiplexer GetConnectionMultiplexer(string connectionString)
        {
            if (!connectionCache.ContainsKey(connectionString) || !connectionCache[connectionString].IsConnected)
            {
                connectionCache[connectionString] = GetManager(connectionString);
            }
            return connectionCache[connectionString];
        }

        private static ConnectionMultiplexer GetManager(string connectionString = null)
        {
            var connect = ConnectionMultiplexer.Connect(connectionString ?? redisConnectionString);

            connect.ConfigurationChanged += MuxerConfigurationChanged;
            connect.ConnectionFailed += MuxerConnectionFailed;
            connect.ConnectionRestored += MuxerConnectionRestored;
            connect.ErrorMessage += MuxerErrorMessage;
            connect.HashSlotMoved += MuxerHashSlotMoved;
            connect.InternalError += MuxerInternalError;

            return connect;
        }

        #region Event

        private static void MuxerConfigurationChanged(object sender, EndPointEventArgs e)
        {
            Console.WriteLine($"{nameof(MuxerConfigurationChanged)}: " + e.EndPoint);
        }

        private static void MuxerConnectionFailed(object sender, ConnectionFailedEventArgs e)
        {
            Console.WriteLine($"{nameof(MuxerConnectionFailed)}: " + e.EndPoint + ", " + e.FailureType + (e.Exception == null ? "" : (", " + e.Exception.Message)));
        }

        private static void MuxerConnectionRestored(object sender, ConnectionFailedEventArgs e)
        {
            Console.WriteLine($"{nameof(MuxerConnectionRestored)}: " + e.EndPoint);
        }

        private static void MuxerErrorMessage(object sender, RedisErrorEventArgs e)
        {
            Console.WriteLine($"{nameof(MuxerErrorMessage)}: " + e.Message);
        }

        private static void MuxerHashSlotMoved(object sender, HashSlotMovedEventArgs e)
        {
            Console.WriteLine($"{ nameof(MuxerHashSlotMoved)}. NewEndPoint: " + e.NewEndPoint + ", OldEndPoint: " + e.OldEndPoint);
        }

        private static void MuxerInternalError(object sender, InternalErrorEventArgs e)
        {
            Console.WriteLine($"{nameof(MuxerInternalError)}: " + e.Exception.Message);
        }

        #endregion Event
    }
}

using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using RedisDemo.RedisHelp;
using StackExchange.Redis;
using System.Threading;
using System.ComponentModel;

namespace RedisDemo.SimpleTest
{
    public class SubscribeForm : Form
    {
        readonly string dummyChannel = "dummy channel";

        Label publishLabel;
        Button publishButton;
        RichTextBox publishArea;

        Label subscribeLabel;
        Button subscribeButton;
        Button unsubscribeButton;
        RichTextBox subscribeArea;

        RedisHelper redis;
        SynchronizationContext context;  //B: commit a request
        BackgroundWorker worker = null;  //D: BackgroundWorker

        public SubscribeForm(RedisHelper _redis)
        {
            publishLabel = new Label() { Location = new Point(10, 15), Height = 10, Width = 120, Text = "Publish Content" };
            publishButton = new Button() { Location = new Point(130, 11), Height = 20, Width = 100, Text = "Publish" };
            publishButton.Click += PublishAsync;
            publishArea = new RichTextBox() { Location = new Point(10, 40), Height = 100, Width = 800, ReadOnly = true };
            //publishArea.TextChanged += PublishAreaScrollToEnd;
            publishArea.TextChanged += (sender, e) => publishArea.ScrollToCaret();

            subscribeLabel = new Label() { Location = new Point(10, 150), Height = 10, Width = 120, Text = "Subscribe Content" };
            subscribeButton = new Button() { Location = new Point(130, 146), Height = 20, Width = 100, Text = "Subscribe" };
            subscribeButton.Click += SubscribeAsync;
            unsubscribeButton = new Button() { Location = new Point(240, 146), Height = 20, Width = 100, Text = "Unsubscribe", Enabled = false };
            unsubscribeButton.Click += UnsubscribeAsync;
            subscribeArea = new RichTextBox() { Location = new Point(10, 175), Height = 200, Width = 800, ReadOnly = true };
            //subscribeArea.TextChanged += SubscribeAreaScrollToEnd;
            subscribeArea.TextChanged += (sender, e) => subscribeArea.ScrollToCaret();

            Control[] controlArr = new Control[] { publishLabel, publishButton, publishArea, subscribeLabel, subscribeButton, unsubscribeButton, subscribeArea };
            Controls.AddRange(controlArr);
            AutoSize = true;

            redis = _redis;

            //A: unsafe way
            //Control.CheckForIllegalCrossThreadCalls = false;

            //B: commit a request
            context = SynchronizationContext.Current;

            //D: BackgroundWorker
            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += (sender, e) => worker.ReportProgress(50, "Worker is running");
            worker.ProgressChanged += (sender, e) => subscribeArea.AppendText(e.UserState.ToString());
            //worker.RunWorkerCompleted += (sender, e) => subscribeArea.AppendText(e.Result.ToString());
            worker.RunWorkerCompleted += (sender, e) => subscribeArea.AppendText($"{e.Result?.ToString()}\n");
        }

        #region Events

        async void SubscribeAsync(object sender, EventArgs e)
        {
            //A: unsafe way
            //Action<RedisChannel, RedisValue> handlerA = (channel, message) => subscribeArea.AppendText(subscribePrint(channel, message, "A"));
            //await redis.SubscribeAsync(dummyChannel, handlerA);

            //B: commit a post request
            SendOrPostCallback callback = obj => subscribeArea.AppendText(obj.ToString());
            Action<RedisChannel, RedisValue> handlerB = (channel, message) => context.Post(callback, subscribePrint(channel, message, "B"));
            await redis.SubscribeAsync(dummyChannel, handlerB);

            //C1: invoke
            Action<RedisChannel, RedisValue> handlerC1 = (channel, message) =>
            {
                string output = subscribePrint(channel, message, "C1");
                if (subscribeArea.InvokeRequired)
                {
                    while (!subscribeArea.IsHandleCreated)
                    {
                        if (subscribeArea.Disposing || subscribeArea.IsDisposed)
                            return;
                    }
                    Action<string> action = text => subscribeArea.AppendText(text);
                    Invoke(action, new object[] { output });  //Will cause performance problem
                }
                else
                {
                    subscribeArea.AppendText(output);
                }
            };
            await redis.SubscribeAsync(dummyChannel, handlerC1);

            //C2: async invoke
            Action<RedisChannel, RedisValue> handlerC2 = (channel, message) =>
            {
                string output = subscribePrint(channel, message, "C2");
                if (subscribeArea.InvokeRequired)
                {
                    while (!subscribeArea.IsHandleCreated)
                    {
                        if (subscribeArea.Disposing || subscribeArea.IsDisposed)
                            return;
                    }
                    Action<string> action = text => subscribeArea.AppendText(text);
                    IAsyncResult result = subscribeArea.BeginInvoke(action, new object[] { output });
                    subscribeArea.EndInvoke(result);
                }
                else
                {
                    subscribeArea.AppendText(output);
                }
            };
            await redis.SubscribeAsync(dummyChannel, handlerC2);

            //D: BackgroundWorker
            Action<RedisChannel, RedisValue> handlerD = (channel, message) =>
            {
                worker.RunWorkerAsync(subscribePrint(channel, message, "D"));
            };
            await redis.SubscribeAsync(dummyChannel, handlerD);

            subscribeButton.Enabled = false;
            unsubscribeButton.Enabled = true;
        }

        async void UnsubscribeAsync(object sender, EventArgs e)
        {
            await redis.UnsubscribeAsync(dummyChannel);
            unsubscribeButton.Enabled = false;
            subscribeButton.Enabled = true;
        }

        async void PublishAsync(object sender, EventArgs e)
        {
            Task<long> clientCount = redis.PublishAsync(dummyChannel, "This message comes from universe");
            await clientCount;
            //publishArea.AppendText($"{clientCount.Result.ToString()} client(s) receive message, dt: {DateTime.Now:T}\n");  //Equals to HH:mm:ss
            publishArea.AppendText($"{clientCount.Result.ToString()} client(s) receive message, dt: {DateTime.Now:HH:mm:ss.fff}\n");
        }

        #endregion Events

        #region Help methods

        #endregion Help methods

        #region Delegates

        Func<RedisChannel, RedisValue, string, string> subscribePrint = (channel, message, methodEntry) => $"{channel.ToString()} channel receive message: {message.ToString()}, dt: {DateTime.Now:HH:mm:ss.fff} {methodEntry}\n";

        #endregion Delegates
    }
}

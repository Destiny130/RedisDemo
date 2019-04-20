using RedisDemo.RedisHelp;
using StackExchange.Redis;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RedisDemo.SimpleTest
{
    public class SubscribeForm : Form
    {
        readonly string dummyChannel = "dummy channel";
        readonly Label publishLabel;
        Button publishButton;
        Button cleanButton;
        RichTextBox publishArea;
        readonly Label subscribeLabel;
        Button subscribeButton;
        Button unsubscribeButton;
        RichTextBox subscribeArea;

        RedisHelper redis;
        readonly SynchronizationContext context;  //B: commit a request
        //BackgroundWorker worker = null;  //D: BackgroundWorker

        public SubscribeForm(RedisHelper _redis)
        {
            publishLabel = new Label() { Location = new Point(10, 15), Height = 10, Width = 120, Text = "Publish Content" };
            publishButton = new Button() { Location = new Point(130, 11), Height = 20, Width = 100, Text = "Publish" };
            publishButton.Click += PublishAsync;
            cleanButton = new Button() { Location = new Point(400, 11), Height = 20, Width = 100, Text = "Clean All" };
            cleanButton.Click += (sender, e) => { publishArea.Clear(); subscribeArea.Clear(); };
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

            Control[] controlArr = new Control[] { publishLabel, publishButton, cleanButton, publishArea, subscribeLabel, subscribeButton, unsubscribeButton, subscribeArea };
            Controls.AddRange(controlArr);
            AutoSize = true;
            CenterToScreen();

            redis = _redis;

            //A: unsafe way
            //Control.CheckForIllegalCrossThreadCalls = false;

            //B: commit a request
            context = SynchronizationContext.Current;

            //D: BackgroundWorker
            //worker = new BackgroundWorker();
            //worker.WorkerReportsProgress = true;
            ////worker.DoWork += (sender, e) => redis.Subscribe(dummyChannel);
            ////worker.DoWork += (sender, e) => redis.Subscribe(dummyChannel, (channel, message) => subscribeArea.AppendText(subscribePrint(channel, message, "D")));
            //worker.DoWork += (sender, e) =>
            //{
            //    //worker.ReportProgress(10, "a");
            //    //Action<int, string> test = worker.ReportProgress;
            //    //redis.Subscribe(dummyChannel, test);
            //    redis.Subscribe(dummyChannel, worker);
            //};
            //worker.ProgressChanged += (sender, e) => subscribeArea.AppendText(e.UserState.ToString());
            //worker.RunWorkerCompleted += (sender, e) =>
            //{
            //    subscribeArea.AppendText($"end? {e.Result?.ToString()}\n");
            //    //redis.Unsubscribe(dummyChannel);
            //};
        }

        #region Events

        async void SubscribeAsync(object sender, EventArgs e)
        {
            #region A: unsafe way

            //Action<RedisChannel, RedisValue> handlerA = (channel, message) => subscribeArea.AppendText(subscribePrint(channel, message, "A"));
            //await redis.SubscribeAsync(dummyChannel, handlerA);

            #endregion A: unsafe way

            #region B: commit a post request

            //SendOrPostCallback postCallback = obj => subscribeArea.AppendText(obj.ToString());
            //Action<RedisChannel, RedisValue> handlerB = (channel, message) => context.Post(postCallback, subscribePrint(channel, message, "B"));
            //await redis.SubscribeAsync(dummyChannel, handlerB);

            #endregion B: commit a post request

            #region C: invoke

            //Sync invoke
            Action<RedisChannel, RedisValue> handlerC1 = (channel, message) => invokeAction(subscribeArea, subscribePrint(channel, message, "C1"), appendTextAction, false);
            await redis.SubscribeAsync(dummyChannel, handlerC1);

            //Async invoke
            Action<RedisChannel, RedisValue> handlerC2 = (channel, message) => invokeAction(subscribeArea, subscribePrint(channel, message, "C2"), appendTextAction, true);
            await redis.SubscribeAsync(dummyChannel, handlerC2);

            #endregion C: invoke

            #region D: BackgroundWroker

            //Action<RedisChannel, RedisValue> handlerD = (channel, message) =>
            //{
            //    worker.RunWorkerAsync(subscribePrint(channel, message, "D"));
            //};
            //await redis.SubscribeAsync(dummyChannel, handlerD);
            //worker.RunWorkerAsync();

            #endregion D: BackgroundWroker

            #region E: multiply threads 

            //IEnumerable<Task> taskList = Enumerable.Range(0, 10).Select(i => Task.Run(() => redis.Subscribe(dummyChannel, handlerC2)));
            //await Task.WhenAll(taskList);

            //Parallel.For(0, 10, i =>
            //{
            //    redis.Subscribe(dummyChannel, handlerC2);
            //});

            #endregion

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
            Task<long> clientCount = redis.PublishAsync(dummyChannel, $"This message comes from universe");
            await clientCount;
            //publishArea.AppendText($"{clientCount.Result.ToString()} client(s) receive message, dt: {DateTime.Now:T}\n");  //Equals to HH:mm:ss
            publishArea.AppendText($"{clientCount.Result.ToString()} client(s) receive message, dt: {DateTime.Now:HH:mm:ss.fff}\n");
        }

        #endregion Events

        #region Delegates

        readonly Func<RedisChannel, RedisValue, string, string> subscribePrint = (channel, message, methodEntry) => $"{channel.ToString()} channel receive message: {message.ToString()}, dt: {DateTime.Now:HH:mm:ss.fff} {methodEntry}\n";

        readonly Action<TextBoxBase, string> appendTextAction = (textBoxBase, text) => textBoxBase.AppendText(text);

        readonly Action<TextBoxBase, string, Action<TextBoxBase, string>, bool> invokeAction = (textBoxBase, text, action, isAsync) =>
        {
            if (textBoxBase.InvokeRequired)
            {
                while (!textBoxBase.IsHandleCreated)
                {
                    if (textBoxBase.Disposing || textBoxBase.IsDisposed)
                    {
                        return;
                    }
                }
                if (isAsync)
                {
                    IAsyncResult result = textBoxBase.BeginInvoke(action, textBoxBase, text);
                    textBoxBase.EndInvoke(result);
                }
                else
                {
                    textBoxBase.Invoke(action, textBoxBase, text);
                }
            }
            else
            {
                textBoxBase.AppendText(text);
            }
        };

        #endregion Delegates
    }
}

using Avalonia.Controls;
using Avalonia.Threading;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Server;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Timers;



namespace 物联网.Views;

public partial class MainView : UserControl
{
    //private static System.Timers.Timer? aTimer;
    private System.Timers.Timer _timer = null!;
    private string data;
    public MainView()
    {

        InitializeComponent();


        //string getvalue = newget("https://moozy.space/value");
        //ShowData.Text = getvalue;
        ////SetTimer();
        // _timer = new System.Timers.Timer(1000); // 每秒触发一次  
        //_timer.Elapsed += TimerElapsed; // 订阅事件  
        //_timer.AutoReset = true;
        //_timer.Enabled = true; // 开始计时器  
        MQTTClientTest();
    }



    public async Task MQTTClientTest()
    {
        string broker = "d90c1610.ala.cn-hangzhou.emqxsl.cn";
        int port = 8883;
        string clientId = Guid.NewGuid().ToString();
        string topic = "mqtt/cs";
        string username = "wlwtext";
        string password = "qwe12340";
        var factory = new MqttFactory();
        var client = factory.CreateMqttClient();
        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(broker, port)
            .WithCredentials(username, password)
            .WithClientId(clientId)
            .WithCleanSession()
            .WithTls(
                o =>
                {
                    o.CertificateValidationHandler = _ => true;
                    // 设置 SSL/TLS 协议版本  
                    o.SslProtocol = SslProtocols.Tls12;

                    // 加载 CA 证书，用于验证服务器证书  
                    // 假设证书文件与你的程序在同一目录下，或者你需要提供完整的文件路径  
                    var caCertPath = "emqxsl-ca.crt"; // 确保这是正确的文件路径  
                    try
                    {
                        var caCert = new X509Certificate2(caCertPath); // 使用 X509Certificate2 而不是 X509Certificate  
                        o.Certificates = new List<X509Certificate> { caCert };
                    }
                    catch 
                    {
                        Dispatcher.UIThread.Post(() => connectwith.Text = "未获取到安全证书，请联系管理员。");
                    }
                }
            )
            .Build();
        var connectResult = await client.ConnectAsync(options);

        // 设置消息接收的事件处理程序  


        if (connectResult.ResultCode == MqttClientConnectResultCode.Success)
        {
            Dispatcher.UIThread.Post(() => connectwith.Text = "成功连接到服务器！");
            await Task.Delay(1500);
            client.ApplicationMessageReceivedAsync += e =>
            {
                Dispatcher.UIThread.Post(() => ShowData.Text =($"{Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment)}"));
                return Task.CompletedTask;
            };
        }
        else
        {
            Dispatcher.UIThread.Post(() => connectwith.Text = $"Failed to connect to MQTT broker: {connectResult.ResultCode}");
        }

        await client.SubscribeAsync(topic);

        // Callback function when a message is received
        client.ApplicationMessageReceivedAsync += e =>
        {
            Dispatcher.UIThread.Post(() => ShowData.Text =($"{Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment)}"));
            return Task.CompletedTask;
        };

    }





    public static string newget(string url)
    {
        HttpClient client = new HttpClient();
        var v = client.GetStringAsync(url).Result;
        return v;
    }

    private void TimerElapsed(object? sender, ElapsedEventArgs e)
    {
        // 在后台线程中启动异步操作  
        Task.Run(async () =>
        {
            try
            {

                //string getvalue = await NewGetAsync("https://moozy.space/value");
                //string getvalue = 
                //data = getvalue;
                await MQTTClientTest();
                // Dispatcher.UIThread.Post(() => ShowData.Text = data);
            }
            catch (Exception ex)
            {

                Dispatcher.UIThread.Post(() => ShowData.Text = ex.Message);
            }
        });
    }

    public static async Task<string> NewGetAsync(string url)
    {
        using (HttpClient client = new HttpClient())
        {
            return await client.GetStringAsync(url);
        }
    }
}

﻿using System.Net;
using System.Text;
using Newtonsoft.Json;
using SocketIOClient;
using SocketIOClient.Transport;
using testrobot;

namespace robot.SocketTool;

public class TT2PostAPI
{
    public long Group;
    public SocketIO Client;
    public string AppToken ;
    public string PlayerToken ;
    public const string Connect = "connected";
    public const string Disconnect = "disconnect";
    public const string Error = "error";
    public const string ConnectError = "connect_error";
    public const string Unsub = "unsub_clan";
    public const string Attack = "attack";
    public const string Start = "start";
    public const string SubStart = "sub_start";
    public const string End = "end";
    public const string Retire = "retire";
    public const string Cycle = "cycle_reset";
    public const string SubCycle = "sub_cycle";
    public const string Target = "target";
    public bool IsAlive;
    public bool IsSuccess;
    public Task SoketTask;
    public Action<string> OnLog;
    public Action<string> OnCannotConnect;
    public Action<string> OnConnect;
    public Action OnStart;
    public Action<AttackAPIInfo> OnAttack;
    public Action<CycleResetDate> OnCycleReset;
    public Action OnDisconnect;
    public Action OnEnd;
    public Action<string> OnEx;

    private static List<string> AllOn = new List<string>()
    {
        Connect,
        Start,
        Attack,
        End,
        Cycle,
        Disconnect,
        Error,
        ConnectError,
        Unsub,
        SubStart,
        Retire,
        SubCycle,
        Target,
    };

    public void Stop()
    {
        try
        {
            Client?.Dispose();
            
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        try
        {
            SoketTask?.Dispose();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            
        }
    }

    public void StartSocket()
    {
        Stop();
        SoketTask = SocketIoTest();
    }

    public void CheckReStart()
    {
        if(Client!=null && Client.Connected && IsSuccess)
            return;
        StartSocket();
    }

    public async Task SocketIoTest()
    {
        if(string.IsNullOrEmpty(AppToken)|| string.IsNullOrEmpty(PlayerToken))
            return;
        Client = new SocketIO("wss://tt2-public.gamehivegames.com/raid");
        Client.Options.Path = "/api";
        Client.Options.Transport = TransportProtocol.WebSocket;
        Client.Options.Reconnection = true;
        Client.Options.ReconnectionAttempts = 1;
        Client.Options.ReconnectionDelay = 20000;
        Client.Options.ExtraHeaders = new Dictionary<string, string>()
        {
            { "API-Authenticate", AppToken }
        };

        Client.On(Connect, response =>
        {
            Console.WriteLine($"{Group}: TT2 API Sussed," + response);
            IsAlive = true;
            var post = Post();
            Console.WriteLine($"{Group}:Post Result : " + post);
            IsSuccess = post.Contains("ok");
            IsSuccess = !post.Contains("Player not found");
            if (!IsSuccess)
            {
                OnCannotConnect?.Invoke(post);
            }
            else
            {
                OnConnect?.Invoke(post);
            }
        });
        Client.On(Start, response =>
        {
            OnStart?.Invoke();
        });
        Client.On(Attack, response =>
        {
            try
            {
                var res = response.ToString();
                Console.WriteLine($"{Group}:{Attack} : {res}");
                res = res.Substring(1, res.Length - 2);
                var atk = JsonConvert.DeserializeObject<AttackAPIInfo>(res);
                OnAttack?.Invoke(atk);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                OnEx?.Invoke($"{e}\n{Group}:{Attack}: {response}");
            }
            
        });
        Client.On(Cycle, response =>
        {
            try
            {
                var res = response.ToString();
                Console.WriteLine($"{Group}:{Cycle} : {res}");
                res = res.Substring(1, res.Length - 2);
                var atk = JsonConvert.DeserializeObject<CycleResetDate>(res);
                OnCycleReset?.Invoke(atk);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                OnEx?.Invoke($"{e}\n{Cycle}: {response}");
            }
            
        });
        Client.On(End, response =>
        {
            OnEnd?.Invoke();
        });
        Client.OnDisconnected += OnDisconnectFun;

        for (var i = 5; i < AllOn.Count; i++)
        {
            var onName = AllOn[i];
            Client.On(onName, response =>
            {
                var res = $"{onName}: {response}\n\n";
                OnLog?.Invoke(res);
                
            });
        }
        Console.WriteLine($"Api连接 {AppToken} || {PlayerToken}");
        await Client.ConnectAsync();
        await Task.Delay(-1);
    }

    private void OnDisconnectFun(object? sender, string e)
    {
        OnDisconnect?.Invoke();
    }

    public string Post()
    {
        string result = "";
        HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://tt2-public.gamehivegames.com/raid/subscribe");
        req.Method = "POST";
        req.ContentType = "application/json; charset=UTF-8";
        req.Accept = "application/vnd.gamehive.btb4-v1.0+json";
        req.UserAgent = "BestHTTP/2 v2.3.1";
        req.Host = "tt2.gamehivegames.com";

        StringBuilder builder = new StringBuilder();
        req.Headers.Add("Accept", "application/json");
        req.Headers.Add("Content-Type", "application/json");
        req.Headers.Add("API-Authenticate", AppToken);

        var json = "{\"player_tokens\": [\"" + PlayerToken + "\"]}";
        byte[] data = Encoding.UTF8.GetBytes(json);
        req.ContentLength = data.Length;
        using (Stream reqStream = req.GetRequestStream())
        {
            reqStream.Write(data, 0, data.Length);
            reqStream.Close();
        }

        HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

        using (Stream stream = resp.GetResponseStream())
        {
            StreamReader reader = new StreamReader(stream);
            result = reader.ReadToEnd();
        }
        //获取响应内容

        return result;
    }


}
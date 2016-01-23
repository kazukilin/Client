using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class NetworkManager
{
    // ホストのIPアドレス
    public static string host = "localhost";
    // ポート番号
    static Int32 port = 22154;

    static TcpClient client;
    static NetworkStream stream;

    // サーバから受け取るデータ
    static byte[] resBytes = new byte[256];

    static int flag = 0;

    public static Thread thread;
    static Encoding encoding = Encoding.UTF8;

    public static event EventHandler ReciveStream;
    public static event EventHandler<ReciveEventArgs> ReceiveMessage;
    public static event EventHandler OnDisconnect;

    public static bool IsConnected
    {
        get
        {
            if (client == null) return false;
            else return client.Connected;
        }
    }

    public static void Connect()
    {
        Console.WriteLine("Thread Start");
        var ts = new ThreadStart(socketConnectThread);
        thread = new Thread(ts);
        thread.Start();
    }

    static void socketConnectThread()
    {
        Console.WriteLine("Connect to " + host);
        Console.WriteLine("Port : " + port);

        try
        {
            client = new TcpClient(host, port);
            stream = client.GetStream();
            if (ReciveStream != null) ReciveStream(null, EventArgs.Empty);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            return;
        }

        while (true)
        {
            try
            {
                string resMessage;
                do
                {
                    resMessage = Read();
                    flag++;
                    if (resMessage == null)
                    {
                        Console.WriteLine("Disconnect");
                        if (OnDisconnect != null) OnDisconnect(null, EventArgs.Empty);
                        return;
                    }
                } while (stream.DataAvailable);

                if (ReceiveMessage != null) ReceiveMessage(null, new ReciveEventArgs(resMessage));

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
    //文字の受信
    public static string Read()
    {
        MemoryStream memoryStream = new MemoryStream();
        int size = stream.Read(resBytes, 0, resBytes.Length);
        if (size == 0) return null;

        memoryStream.Write(resBytes, 0, size);
        memoryStream.Close();
        return encoding.GetString(memoryStream.ToArray());
    }
    // ファイルの送信
    public static void WriteFile(string fileName)
    {
        byte[] sendBytes = File.ReadAllBytes(fileName);
        Console.WriteLine("ファイル送信中");
        stream.Write(sendBytes, 0, sendBytes.Length);
        Console.WriteLine("送信終わり");
    }

    // テキストの送信(改行)
    public static void WriteLine(string message)
    {
        if (!NetworkManager.IsConnected)
        {
            Console.WriteLine("Network is Not Connected");
            return;
        }

        byte[] bytes = encoding.GetBytes(message);
        stream.Write(bytes, 0, bytes.Length);
        Console.WriteLine("Send Completed");
    }
    //送信されてきたファイルデータを読み取り、ファイルに書き込み
    public static void ReadFile(string filename)
    {
        Console.WriteLine("ReadFileメソッド");
        MemoryStream memoryStream = new MemoryStream();
        int size = 0;
        int resize = 0;
        int flag = 0;
        int cnt = 0;
        while (true)
        {
            Console.WriteLine("受信中");
            size = stream.Read(resBytes, 0, resBytes.Length);
            if (size < 255 && cnt > 2) flag = 1;
            resize = resize + size;
            Console.WriteLine("{0}byte受信おわり", resize);
            memoryStream.Write(resBytes, 0, size);
            cnt++;
            if (flag == 1)
            {
                Console.WriteLine("return");
                break;
            }
        }
        Console.WriteLine("memoryStreamをClose");
        memoryStream.Close();
        int bytes = 256 * cnt;
        byte[] file = new byte[bytes];
        file = memoryStream.ToArray();
        Console.WriteLine("書き込み開始");
        File.WriteAllBytes(filename, file);
        Console.WriteLine("書き込み終了。");
    }
    public static void Upload(string name)
    {
        int size = 0;
        MemoryStream memoryStream = new MemoryStream();

        byte[] recieve = new byte[4096];
        Console.WriteLine("受信中");
        size = stream.Read(recieve, 0, recieve.Length);
        Console.WriteLine("受信終わり");
        memoryStream.Write(recieve, 0, size);

        memoryStream.Close();
        byte[] file = new byte[4096];
        file = memoryStream.ToArray();
        File.WriteAllBytes(name, file);
    }
    public static void Dispose()
    {
        if (stream != null) stream.Close();
        if (client != null) client.Close();
        if (thread.IsAlive)
        {
            thread.Abort();
            thread.Join();
        }
    }
}
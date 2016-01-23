using System;
using System.Threading;

class Program
{

    [STAThread]
    static void Main(string[] args)
    {
        // 接続できたら発生するイベントの登録
        NetworkManager.ReciveStream += NetworkManager_ReceiveStream;
        // メッセージを受け取った時に発生するイベントの登録
        NetworkManager.ReceiveMessage += NetworkManager_ReceiveMessage;

        // サーバに接続
        NetworkManager.Connect();

        while (true)
        {
            var cmd = Console.ReadLine();

            string dl = cmd.Substring(0, 2);
            string dlfilename = cmd.Remove(0, 2);

            string up = cmd.Substring(0, 2);
            string upfilename = cmd.Remove(0, 2);

            string meta = upfilename + ".meta";
            string midi = upfilename + ".midi";
            string data = upfilename + ".data";
            string dlp = "";

            if (cmd == "sendtxt")
            {
                Console.WriteLine("送信するテキストを入力");
                string sendtxt = Console.ReadLine();
                NetworkManager.WriteLine("sendtxt" + sendtxt);
            }
            if (cmd == "sendfile")
            {
                NetworkManager.WriteLine("sendfile");
                NetworkManager.WriteFile("hoge.jpg");
                Console.WriteLine("送信プロセス終了");
            }
            if (dl == "dl")
            {
                dlp = cmd;
                NetworkManager.WriteLine(dlp);
                Thread.Sleep(500);
                NetworkManager.Upload(dlfilename + ".midi");
                Thread.Sleep(1500);
                NetworkManager.Upload(dlfilename + ".data");
                Thread.Sleep(1500);
                NetworkManager.Upload(dlfilename + ".meta");
                Console.WriteLine("Finish Download");
            }
            if (up == "up")
            {
                NetworkManager.WriteLine("up");
                NetworkManager.WriteFile(upfilename + ".meta");
                Thread.Sleep(500);
                NetworkManager.WriteFile(upfilename + ".midi");
                Thread.Sleep(500);
                NetworkManager.WriteFile(upfilename + ".data");
                Console.WriteLine("Finish Upload");
            }
            if (cmd == "list")
            {
                NetworkManager.WriteLine("list");
                NetworkManager.Upload("List.lst");//サーバーから受信
                Console.WriteLine("Finish Send listFile");
            }
            if (cmd == "quit") break;
        }
        NetworkManager.Dispose();
    }

    static void NetworkManager_ReceiveStream(object sender, EventArgs e)
    {//接続された時に呼ばれる
        Console.WriteLine("サーバに接続");
    }

    static void NetworkManager_ReceiveMessage(object sender, ReciveEventArgs e)
    {//なにか受信した時に呼ばれる
        var message = e.message;
        int mesleng = message.Length;
        string mode = "";
        string sendmod = "";
        string sendtxt = "";

        if (mode == "") mode = e.message; //モードの判別準備

        if (message.Length > 7)
        {
            //int mesLength = message.Length;
            sendmod = message.Substring(0, 7);//modeを出すため
            sendtxt = message.Remove(0, 7);//文字列を出すため
        }

        if (sendmod == "sendtxt") //文字送受信モード
        {
            Console.WriteLine(sendtxt);
        }
        if (mode == "sendfile") //ファイル送信モード
        {
            string fonamae = Console.ReadLine();
            NetworkManager.ReadFile(fonamae);
        }
    }
}
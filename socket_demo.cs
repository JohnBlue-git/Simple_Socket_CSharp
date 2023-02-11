/*
Auther: John Blue
Time: 2023/2
Platform: VS2017
Object: A demonstration of Client and Server socket.
        In Main Class, Client and Server socket will be called and created.
        In Client Class, SendMsgToServer and ReceiveMsgFromServer will be run as threads, and they will keep sending or receiving maessage from Server.
        In Server Class, ServerCommunity will become a thread, waiting to connect with Client.
        In each socket created inside ServerCommunity, SendMsgToClient and ReceiveClient will keep sending or receiving maessage from Client.
*/

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;



namespace SocketDemo
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            // ip 192.168.50.126 or 127.0.0.1
            // port 6000

            // one Server
            Server sv = new Server();
            Server.Commence(60000, 10);

            // one Client
            Client ct = new Client();
            Client.Commence("192.168.50.126", 60000);
        }
    }
    
    
    
    class Client
    {
        // Start the Client
        public static void Commence(string ip, int port)
        {
            // setting and starting
            ClientSocket(ip, port);
        }

        //

        private static Socket socket;

        private static void ClientSocket(string ip, int port)
        {
            // Create socket
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Connect
            IPAddress myIp = IPAddress.Parse(ip);
            IPEndPoint point = new IPEndPoint(myIp, port);
            socket.Connect(point);
            Console.WriteLine("Connect Succese! " + socket.RemoteEndPoint.ToString());

            // Threading sending and receiving
            Thread sendMsg = new Thread(SendMsgToServer);
            sendMsg.IsBackground = true;
            sendMsg.Start();

            Thread ReceiveMsg = new Thread(ReceiveMsgFromServer);
            ReceiveMsg.IsBackground = true;
            ReceiveMsg.Start();
        }

        private static void ReceiveMsgFromServer()
        {
            // buffer
            byte[] buffer = new byte[1024];
            
            // keep receiving
            int rec = 0;
            while (true) {
                // receive
                rec = socket.Receive(buffer);
                
                // check null
                if (rec == 0) {
                    Console.WriteLine("Server Loss!");
                    break;
                }
                
                // show message
                Console.WriteLine("Server: " + System.Text.Encoding.UTF8.GetString(buffer, 0, rec));
            }
        }

        private static void SendMsgToServer()
        {
            // buffer
            byte[] buffer;
            string inputText;
            
            // keep sending
            while (true)
            {
                inputText = "Hello from Client";
                //inputText = Console.ReadLine();

                buffer = System.Text.Encoding.UTF8.GetBytes(inputText);
                socket.Send(buffer);
            }
        }
    }
    
    
    
    class Server
    {
        // Start Server
        public static void Commence(int myPort, int allowNum)
        {
            // setting server
            ServerEnd(myPort, allowNum);
            
            // starting server thread
            Thread th = new Thread(ServerCommunity);
            th.Start(socketListener);
        }

        //

        private static Socket socketListener;

        private static void ServerEnd(int myPort, int allowNum)
        {
            // Create socket
            //
            //AddressFamily：InterNetwork表示利用IP4協議
            //
            //SocketType.Stream 因使用TCP協議
            //
            //ProtocolType.Tcp 選用TCP協議
            //
            //https://stevenke.gitbooks.io/internetnote/content/chapter1.html
            socketListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Binding
            //
            //IPEndPoint結構
            //
            //IPAddress.Any方法得到本機的ＩＰ，同時聽網內/網外
            //
            IPAddress ip = IPAddress.Any;
            int port = myPort;
            IPEndPoint point = new IPEndPoint(ip, port);
            socketListener.Bind(point);

            // Listening
            socketListener.Listen(allowNum);
            Console.WriteLine("Listening...");
        }

        private static void ServerCommunity()
        {
            while (true)
            {
                // Wait for Client
                Socket socketSender = socketListener.Accept();

                // Client info
                Console.WriteLine(("Client IP = " + socketSender.RemoteEndPoint.ToString()) + " Connect Succese!");

                // Threading of sending and receiving
                Thread ReceiveMsg = new Thread(ReceiveClient);
                ReceiveMsg.IsBackground = true;
                ReceiveMsg.Start(socketSender);

                Thread SendToClient = new Thread(SendMsgToClient);
                SendToClient.IsBackground = true;
                SendToClient.Start(socketSender);
            }

        }

        private static void SendMsgToClient(object mySocketSender)
        {
            // object to Socket
            Socket socketSender = mySocketSender as Socket;

            // buffer
            byte[] buffer;
            string msg = "Hello from Server";

            // keep sending
            while (true)
            {
                // null check 
                if (socketSender.RemoteEndPoint == null) {
                    Console.WriteLine("socketSender.RemoteEndPoint == null");
                    break;
                }
                
                // send
                //msg = Console.ReadLine();
                buffer = Encoding.UTF8.GetBytes(msg);
                socketSender.Send(buffer);
            }
        }

        private static void ReceiveClient(object mySocketSender)
        {
            // object to Socket
            Socket socketSender = mySocketSender as Socket;

            // buffer
            byte[] buffer = new byte[1024];

            // keep receiving
            while (true)
            {
                // receive
                int rece = socketSender.Receive(buffer);
                
                // null check
                if (rece == 0) {
                    Console.WriteLine(string.Format("Client : {0} + 下線了", socketSender.RemoteEndPoint.ToString()));
                    break;
                }
                
                // show message
                Console.WriteLine(string.Format("Client : {0}", Encoding.UTF8.GetString(buffer, 0, rece)));
            }
        }
    }
}

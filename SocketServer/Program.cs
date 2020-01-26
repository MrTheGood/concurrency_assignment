/*** Fill these lines with your complete information.
 * Note: Incomplete information may result in FAIL.
 * Member 1: [First and Last name, first member]: Maarten de Goede
 * Std Number 1: [Student number of the first member] 0966770
 * Class: [what is your class, example INF2C] DINF2
 ***/


using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Text;
/* Note: If you are using .net core 2.1, install System.Text.Json (use NuGet). */
using System.Text.Json;
using System.Threading;

namespace SocketServer
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public abstract class ClientInfo
    {
        public string studentnr { get; set; }
        public string classname { get; set; }
        public int clientid { get; set; }
        public string teamname { get; set; }
        public string ip { get; set; }
        public string secret { get; set; }
        public string status { get; set; }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class Message
    {
        public const string welcome = "WELCOME";
        public const string stopCommunication = "COMC-STOP";
        public const string statusEnd = "STAT-STOP";
        public const string secret = "SECRET";
    }

    public class SequentialServer
    {
        private Socket _listener;
        private IPEndPoint _localEndPoint;
        private readonly IPAddress _ipAddress = IPAddress.Parse("127.0.0.1");
        private const int PortNumber = 11111;

        private readonly LinkedList<ClientInfo> _clients = new LinkedList<ClientInfo>();

        private bool _stopCond;
        private const int ProcessingTime = 1000;
        private const int ListeningQueueSize = 5;

        public void PrepareServer()
        {
            var bytes = new byte[1024];

            try
            {
                Console.WriteLine("[Server] is ready to start ...");
                // Establish the local endpoint
                _localEndPoint = new IPEndPoint(_ipAddress, PortNumber);
                _listener = new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                Console.Out.WriteLine("[Server] A socket is established ...");
                // associate a network address to the Server Socket. All clients must know this address
                _listener.Bind(_localEndPoint);
                // This is a non-blocking listen with max number of pending requests
                _listener.Listen(ListeningQueueSize);
                while (true)
                {
                    Console.WriteLine("Waiting connection ... ");
                    // Suspend while waiting for incoming connection 
                    var connection = _listener.Accept();
                    SendReply(connection, Message.welcome);

                    while (true)
                    {
                        var numByte = connection.Receive(bytes);
                        var data = Encoding.ASCII.GetString(bytes, 0, numByte);
                        var replyMsg = ProcessMessage(data);
                        
                        if (replyMsg.Equals(Message.stopCommunication))
                            break;
                        SendReply(connection, replyMsg);
                    }
                }
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e.Message);
            }
        }

        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once UnusedParameter.Global
        public void HandleClient(Socket con)
        {
        }

        private string ProcessMessage(string msg)
        {
            Thread.Sleep(ProcessingTime);
            Console.WriteLine("[Server] received from the client -> {0} ", msg);
            var replyMsg = "";

            try
            {
                switch (msg)
                {
                    case Message.stopCommunication:
                        replyMsg = Message.stopCommunication;
                        break;
                    default:
                        var c = JsonSerializer.Deserialize<ClientInfo>(msg);
                        _clients.AddLast(c);
                        if (c.clientid == -1)
                        {
                            _stopCond = true;
                            ExportResults();
                        }

                        c.secret = c.studentnr + Message.secret;
                        c.status = Message.statusEnd;
                        replyMsg = JsonSerializer.Serialize(c);
                        break;
                }
            }
            catch (Exception e)
            {
                Console.Out.WriteLine("[Server] processMessage {0}", e.Message);
            }

            return replyMsg;
        }

        private static void SendReply(Socket connection, string msg)
        {
            var encodedMsg = Encoding.ASCII.GetBytes(msg);
            connection.Send(encodedMsg);
        }

        private void ExportResults()
        {
            if (_stopCond)
                PrintClients();
        }

        private void PrintClients()
        {
            const string delimiter = " , ";
            Console.Out.WriteLine("[Server] This is the list of clients communicated");
            foreach (var c in _clients)
            {
                Console.WriteLine(c.classname + delimiter + c.studentnr + delimiter + c.clientid);
            }

            Console.Out.WriteLine("[Server] Number of handled clients: {0}", _clients.Count);

            _clients.Clear();
            _stopCond = false;
        }
    }


    // ReSharper disable once UnusedType.Global
    public class ConcurrentServer
    {
        // todo: implement this class
    }

    public static class ServerSimulator
    {
        public static void SequentialRun()
        {
            Console.Out.WriteLine("[Server] A sample server, sequential version ...");
            SequentialServer server = new SequentialServer();
            server.PrepareServer();
        }

        // ReSharper disable once UnusedMember.Global
        public static void ConcurrentRun()
        {
            // todo: After finishing the concurrent version of the server, implement this method to start the concurrent server
        }
    }

    internal static class Program
    {
        // Main Method 
        private static void Main()
        {
            Console.Clear();
            ServerSimulator.SequentialRun();
            // todo: uncomment this when the solution is ready.
            //ServerSimulator.concurrentRun();
        }
    }
}
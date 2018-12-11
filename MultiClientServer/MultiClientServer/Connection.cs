using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace MultiClientServer
{
    class Connection
    {
        public StreamReader Read;
        public StreamWriter Write;
        public List<Thread> threads = new List<Thread>();
        private bool ping;
        object o = new object();
        // Connection heeft 2 constructoren: deze constructor wordt gebruikt als wij CLIENT worden bij een andere SERVER
        public Connection(int port)
        {
            TcpClient client = new TcpClient("localhost", port);
            Read = new StreamReader(client.GetStream());
            Write = new StreamWriter(client.GetStream());
            Write.AutoFlush = true;

            // De server kan niet zien van welke poort wij client zijn, dit moeten we apart laten weten
            Write.WriteLine("Poort: " + Program.MijnPoort);

            // Start het reader-loopje
            Thread thread = new Thread(ReaderThread);
            threads.Add(thread);
            thread.Start();
        }

        // Deze constructor wordt gebruikt als wij SERVER zijn en een CLIENT maakt met ons verbinding
        public Connection(StreamReader read, StreamWriter write)
        {
            Read = read; Write = write;

            // Start het reader-loopje
            Thread thread = new Thread(ReaderThread);
            threads.Add(thread);
            thread.Start();         
        }

        // LET OP: Nadat er verbinding is gelegd, kun je vergeten wie er client/server is (en dat kun je aan het Connection-object dus ook niet zien!)

        // Deze loop leest wat er binnenkomt en print dit
        public void ReaderThread()
        {
            try
            {
                while (true)
                {
                    string message = Read.ReadLine();
                    Console.WriteLine(message);
                    if (message.StartsWith("ping ping"))
                    {
                        Program.Buren[Int32.Parse(message.Split()[2])].Write.WriteLine("ping pong");
                    }
                    else if (message.StartsWith("ping pong"))
                    {
                        lock (o)
                        {
                            ping = false;
                        }
                    }
                }
            }
            catch { } // Verbinding is kennelijk verbroken
        }
        public int Ping(int port)
        {
            ping = true;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            while (stop())
            {
            }
            stopwatch.Stop();
            return (int)stopwatch.ElapsedMilliseconds;
        }
        bool stop()
        {
            lock (o)
            {
                return ping;
            }
        }
        public void print()
        {
            Console.WriteLine(Program.Connecties.ToString());
        }
    }
}

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
<<<<<<< HEAD
=======
        public int eigenadres;
        public int doeladres;
>>>>>>> Thijs_poging2
        object o = new object();
        // Connection heeft 2 constructoren: deze constructor wordt gebruikt als wij CLIENT worden bij een andere SERVER
        public Connection(int port)
        {
            TcpClient client = new TcpClient("localhost", port);
            Read = new StreamReader(client.GetStream());
            Write = new StreamWriter(client.GetStream());
            Write.AutoFlush = true;

            this.eigenadres = Program.MijnPoort;
            this.doeladres = port;

            // De server kan niet zien van welke poort wij client zijn, dit moeten we apart laten weten
            Write.WriteLine("Poort: " + Program.MijnPoort);
            
            // Start het reader-loopje
            Thread thread = new Thread(ReaderThread);
            thread.Start();
        }

        // Deze constructor wordt gebruikt als wij SERVER zijn en een CLIENT maakt met ons verbinding
        public Connection(StreamReader read, StreamWriter write)
        {
            Read = read; Write = write;

            // Start het reader-loopje

            this.eigenadres = Program.MijnPoort;

            Thread thread = new Thread(ReaderThread);
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
<<<<<<< HEAD
                    Console.WriteLine(message);
=======
                    // als een bericht binnenkomt wat voor de eigen poort bedoeld is, wordt er gekeken wat de opdracht is
                    while (message.StartsWith(eigenadres.ToString()))
                    {
                        message = message.Substring(message.IndexOf(" ") + 1);
                    }
                    if (message.StartsWith(eigenadres.ToString()))
                    {
                        message = message.Substring(message.IndexOf(" ") + 1);
                        Console.WriteLine(message);
                    }
                    // Is het voor een andere poort, dan gaat hij die proberen door te sturen
                    else
                    {
                        try
                        {
                            Program.SendMessage(message);
                        }
                        catch
                        {
                            Console.WriteLine(message);
                        }
                    }
>>>>>>> Thijs_poging2
                }
            }
            catch { } // Verbinding is kennelijk verbroken
        }
<<<<<<< HEAD
=======

        public void SendMessage (String input)
        {
            Write.WriteLine(input);
        }

        public void SetDestination(int poort)
        {
            this.doeladres = poort;
        }
>>>>>>> Thijs_poging2
    }
}

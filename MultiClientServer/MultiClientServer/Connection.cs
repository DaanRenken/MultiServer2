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
        public int ping, eigenadres, doeladres, favopoort;
        object o = new object();
        private System.Timers.Timer timer;
        // Connection heeft 2 constructoren: deze constructor wordt gebruikt als wij CLIENT worden bij een andere SERVER
        public Connection(int poort)
        {
            TcpClient client = new TcpClient("localhost", poort);
            Read = new StreamReader(client.GetStream());
            Write = new StreamWriter(client.GetStream());
            Write.AutoFlush = true;

            // De server kan niet zien van welke poort wij client zijn, dit moeten we apart laten weten
            this.eigenadres = Program.MijnPoort;
            this.doeladres = poort;
            this.favopoort = poort;
            this.ping = 1;
            Write.WriteLine("Poort: " + eigenadres);

            // Start het reader-loopje
            Thread thread = new Thread(ReaderThread);
            threads.Add(thread);
            thread.Start();
        }

        // Deze constructor wordt gebruikt als wij SERVER zijn en een CLIENT maakt met ons verbinding
        public Connection(StreamReader read, StreamWriter write, int poort)
        {
            Read = read; Write = write;

            this.eigenadres = Program.MijnPoort;
            this.doeladres = poort;
            this.favopoort = poort;
            this.ping = 1;
            // Start het reader-loopje

            Thread thread = new Thread(ReaderThread);
            threads.Add(thread);
            thread.Start();
        }

        // Derde constructor, bedoeld om streamreader/streamwriter eruit te slopen
        public Connection(int doeladres, int favopoort, int ping)
        {
            this.eigenadres = Program.MijnPoort;
            this.doeladres = doeladres;
            this.favopoort = favopoort;
            this.ping = ping;
        }

        // Deze loop leest wat er binnenkomt en print dit
        public void ReaderThread()
        {
            try
            {
                while (true)
                {
                    string message = Read.ReadLine();
                    //Console.WriteLine(message);
                    // als een bericht binnenkomt wat voor de eigen poort bedoeld is, wordt er gekeken wat de opdracht is
                    if (message.StartsWith(eigenadres.ToString()))
                    {
                        message = message.Substring(message.IndexOf(" ") + 1);

                        // bij "GetDictionary" wordt de dictionary gestuurd naar iedere neighbor
                        if (message.StartsWith("GetDictionary"))
                        {
                            SendDictionary();
                        }
                        // bij "Dictionary" wordt een nieuwe connection aan de dictonary toegevoegd
                        else if (message.StartsWith("Dictionary"))
                        {
                            UpdateDictionary(message.Substring(message.IndexOf(" ") + 1));
                        }
                        // bij "Remove Connection" wordt de connectie tussen deze en de betreffende poort verwijderd
                        // vanuit RemoveConnection wordt vervolgens "Removed Connection" naar alle buren gestuurd
                        else if (message.StartsWith("Remove Connection"))
                        {
                            //Console.WriteLine(message);
                            int poort = Int32.Parse(message.Split()[2]);
                            Program.RemoveConnection(poort);
                        }
                        // bij "Removed Connection" is er een connectie verdwenen en updaten alle buren hun dictionary en sturen die naar hun buren
                        else if (message.StartsWith("Removed Connection"))
                        {
                            //Console.WriteLine(message);
                            int poort = Int32.Parse(message.Split()[2]);
                            //if (this.doeladres == this.favopoort)
                            {
                                // wanneer een verbinding wegvalt, wordt er eerst van uit gegaan dat die poort niet meer bereikbaar is
                                // als nou later blijkt dat die toch nog te bereiken is, wordt er via de dictionary rondgestuurd dat er toch nog een verbinding mogelijk is
                                if (Program.Buren[poort].favopoort != Int32.Parse(message.Split()[3]))
                                {
                                    SendDictionary(this, poort);
                                }
                                else if (Program.Buren[poort].favopoort == this.favopoort)
                                {
                                    Program.RemoveConnection(poort);
                                }
                            }
                        }
                        // als het bericht niet voor deze poort is, wordt het doorgestuurd naar degene voor wie het wel bestemd is
                        else
                        {
                            lock (o)
                            {
                                try
                                {
                                    int voor = Int32.Parse(message.Split()[0]);
                                    int naar = Program.Buren[voor].favopoort;
                                    SendMessage(naar, message);
                                }
                                catch
                                {
                                    Console.WriteLine(message);
                                }
                            }
                        }
                    }
                }
            }
            catch { } // Verbinding is kennelijk verbroken
        }

        // UpdateDictionary voegt een nieuwe connection aan de dictionary toe, mocht deze nog niet bestaan
        void UpdateDictionary(string input)
        {
            int poort = Int32.Parse(input.Split()[0]);
            int ping2 = Int32.Parse(input.Split()[1]) + 1;
            if ((!Program.Buren.ContainsKey(poort) && poort != eigenadres))
            {
                Connection connection = new Connection(poort, this.favopoort, ping2);
                Program.AddConnection(connection);
            }
            if (Program.Buren.ContainsKey(poort))
            {
                if (Program.Buren[poort].ping > ping2)
                {
                    //Console.WriteLine("start new connection" + this.doeladres + " " + this.favopoort + " " + this.ping + " " + poort);
                    Program.UpdateConnection(poort, this.favopoort, ping2);
                }
            }
        }

        // Alle SendMessage functies sturen messages, sturen ze door of ontvangen ze en writen ze
        public void SendMessage(string message)
        {
            message = doeladres + " " + message;
            if (this.doeladres == favopoort)
            {
                Write.WriteLine(message);
            }
            else
            {
                SendMessage(favopoort, message);
            }
        }
        public void SendMessage(int naarpoort, string message)
        {
            Program.Buren[naarpoort].SendMessage(message);
        }
        public void SendMessage(int naarpoort, int vanpoort, string message)
        {
            message = naarpoort + " " + message + " " + vanpoort;
            SendMessage(naarpoort, message);
        }

        // Alle SendDictionary functies sturen de dictionary van de poort door naar hun neighbors, of printen een ontvangen dictionary
        public void SendDictionary()
        {
            for (int i = 0; i < Program.Connecties.Count; i++)
            {
                {
                    SendDictionary(this, Program.Connecties[i]);
                }
            }
        }
        public void SendDictionary(Connection Neigbour, int connectie)
        {
            string output = "Dictionary " + connectie + " " + Program.Buren[connectie].ping;
            Neigbour.SendMessage(output);
        }
        public void SendDictionary(List<Connection> Neigbours, int connectie)
        {

            for (int i = 0; i < Neigbours.Count; i++)
            {
                SendDictionary(Neigbours[i], connectie);
            }
        }
    }
}

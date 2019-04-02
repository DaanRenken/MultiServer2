using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiClientServer
{
    class Node
    {
        int poort;
        int distance;
        int prefneighbor;
<<<<<<< HEAD
=======
        Connection connection;
>>>>>>> Thijs_poging2

        public Node(int inputPoort, int inputDistance, int inputNeighbor)
        {
            poort = inputPoort;
            distance = inputDistance;
            prefneighbor = inputNeighbor;
        }

        public int ReturnPoort() { return poort; }
        public int ReturnDistance() { return distance; }
        public int ReturnNeighbor() { return prefneighbor; }

        public void Update(int newPoort, int newDistance, int newNeighbor)
        {
            if ((poort == newPoort) && (newDistance < distance))
            {
                distance = newDistance;
                prefneighbor = newNeighbor;
            }
        }
<<<<<<< HEAD
=======

        public void CreateConnection(int inputPoort)
        {
            connection = new Connection(poort);
            Console.WriteLine("Connection succesfully created");
        }

        public void AcceptConnection(Connection newConnection)
        {
            connection = newConnection;
            connection.SetDestination(poort);
        }

        public void WriteMessage(String input)
        {
            connection.SendMessage(input);
        }
>>>>>>> Thijs_poging2
    }
}

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
    }
}

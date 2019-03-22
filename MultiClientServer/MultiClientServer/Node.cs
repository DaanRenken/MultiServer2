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

        int ReturnPoort() { return poort; }
        int ReturnDistance() { return distance; }
        int ReturnNeighbor() { return prefneighbor; }

        void Update(int newPoort, int newDistance, int newNeighbor)
        {
            if ((poort == newPoort) && (newDistance < distance))
            {
                distance = newDistance;
                prefneighbor = newNeighbor;
            }
        }
    }
}

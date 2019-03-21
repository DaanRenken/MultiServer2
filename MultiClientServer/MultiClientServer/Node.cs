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

        public Node (int poort, int distance, int prefneighbor)
        {

        }

        public void Update(int a, int b, int c)
        {
            if (b < distance)
            {
                distance = b;
                prefneighbor = c;
            }
        }
    }
}

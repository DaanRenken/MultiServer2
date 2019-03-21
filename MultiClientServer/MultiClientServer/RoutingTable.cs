using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiClientServer
{
    class RoutingTable
    {
        List<Node> myRoutingTable = new List<Node>();

        public RoutingTable()
        {

        }

        public void AddNode(int poort, int distance, int prefneighbor)
        {
            Node node = new Node(poort, distance, prefneighbor);
            myRoutingTable.Add(node);
        }

        public void UpdateNode(int poort, int distance, int prefneighbor)
        {
            myRoutingTable.Find(Node node.poort == poort).Update(poort, distance, prefneighbor);
        }

    }
}

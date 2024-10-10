using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageEncryptCompress
{
    public class Node
    {
        public byte color  { get; set; }
        public int Frequency { get; set; }
        public Node Left { get; set; }
        public Node Right { get; set; }


        public static int NodeComparer(object x, object y)
        {
                Node a = (Node)x;
                Node b = (Node)y;

                return a.Frequency - b.Frequency;

            //return (IComparer<Node>)new SortNode();
        }


        public Node()
        {
            Left = null;
            Right = null;
        }

        public Node(byte val , int freq)
        {
            color = val;
            Frequency = freq;
            Left = null;
            Right = null;
        }

        public Node(byte val,int freq,Node L , Node R)
        {
            color = val;
            Frequency = freq;
            Left = L;
            Right = R;
        }
    }

    class SortNode : IComparer
    {
        int IComparer.Compare(object x, object y)
        {
            Node a = (Node)x;
            Node b = (Node)y;

            return a.Frequency - b.Frequency;
        }
    }

}

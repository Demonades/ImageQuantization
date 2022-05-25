using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Linq;
using Lucene.Net.Util;

namespace ImageQuantization
{
    public class AutomaticKDetection
    {
        public List<Edge> Edges = new List<Edge>();
        int cnt = 0;
        int indexStart = 0, indexEnd = 0;

        public int Calculate()
        {
            cnt = Edges.Count;
            double mean = CalculateMean(), prevSD = 0, SD = 1, removedEdge;
            Edges.Sort((e1, e2) => e1.distance.CompareTo(e2.distance));
            indexEnd = cnt - 1;
            int k = 1;
            while (cnt > 2 && Math.Abs(SD-prevSD) > 0.0001)
            {
                //Compare between first and last element, which is furthest from the mean
                if (Math.Abs(Edges[indexStart].distance - mean) > Math.Abs(Edges[indexEnd].distance - mean)){
                    removedEdge = Edges[indexStart].distance;
                    indexStart++;
                }
                else
                {
                    removedEdge = Edges[indexEnd].distance;
                    indexEnd--;
                }
                cnt--;
                k++;
                mean = FastMean(mean, removedEdge);
                prevSD = SD;
                SD = CalculateSD(mean);
            }
            return k;
        }
        public double CalculateMean()
        {
            double m = 0;
            foreach(Edge e in Edges)
            {
                m += e.distance;
            }
            return m / cnt;
        }
        public double FastMean(double oldMean, double removedEdge)
        {
            double newMean = oldMean * (cnt+1);
            newMean -= removedEdge;
            return newMean / (cnt);
        }
        public double CalculateSD(double mean)
        {
            double up = 0, down = cnt;
            for(int i = indexStart; i <= indexEnd; i++)
            {
                Edge e = Edges[i];
                up += (Math.Abs(e.distance - mean) * Math.Abs(e.distance - mean));
            }
            return Math.Sqrt(up/down);
        }
        /*
         * if(Math.Abs(Edges[0].distance - mean) > Math.Abs(Edges[cnt-1].distance - mean))
                {
                    removedEdge = Edges[0].distance;
                    Edges.RemoveAt(0);
                }
                else
                {
                    removedEdge = Edges[cnt-1].distance;
                    Edges.RemoveAt(cnt-1);
                }
         */
    }

}

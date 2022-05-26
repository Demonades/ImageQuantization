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
            setupSD(mean);
            Edges.Sort((e1, e2) => e1.distance.CompareTo(e2.distance));
            indexEnd = cnt - 1;
            int k = 1;
            while (cnt > 2 && Math.Abs(SD - prevSD) > 0.0001)
            {
                //Compare between first and last element, which is furthest from the mean
                if (Math.Abs(Edges[indexStart].distance - mean) > Math.Abs(Edges[indexEnd].distance - mean))
                {
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
                double oldMean = mean;
                mean = FastMean(mean, removedEdge);
                prevSD = SD;
                SD = FastSD(oldMean, mean, removedEdge);
            }
            return k;
        }
        public double CalculateMean()
        {
            double m = 0;
            foreach (Edge e in Edges)
            {
                m += e.distance;
            }
            return m / cnt;
        }
        public double FastMean(double oldMean, double removedEdge)
        {
            double newMean = oldMean * (cnt + 1);
            newMean -= removedEdge;
            return newMean / (cnt);
        }
        double firstTerm = 0, secondTerm = 0;
        public void setupSD(double mean)
        {
            foreach (Edge e in Edges)
            {
                firstTerm += e.distance * e.distance;
                secondTerm += e.distance;
            }
        }

        public double FastSD(double oldMean, double mean, double removedEdge)
        {
            firstTerm -= (removedEdge * removedEdge);
            secondTerm -= removedEdge;

            double test = 2 * mean * secondTerm;
            double thirdTerm = cnt * (mean * mean);
            return Math.Sqrt((firstTerm - test + thirdTerm) / cnt);
        }
        public double CalculateSD(double mean)
        {
            double up = 0, down = cnt;
            for (int i = indexStart; i <= indexEnd; i++)
            {
                Edge e = Edges[i];
                up += (Math.Abs(e.distance - mean) * Math.Abs(e.distance - mean));
            }
            return Math.Sqrt(up / down);
        }
    }

}

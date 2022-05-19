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
    public struct Cut
    {
        public int index;
        public double distance;

        public Cut(int index, double distance)
        {
            this.index = index;
            this.distance = distance;
        }
    }
    public struct Edge
    {
        public int u, v;
        public double distance;

        public Edge(int u, int v, double distance)
        {
            this.u = u;
            this.v = v;
            this.distance = distance;
        }
    }
    public class PixelOperations
    {



        public static int N = 1000000;
        public static List<Edge> MST = new List<Edge>();
        public static List<RGBPixel> distinctColors = new List<RGBPixel>();
        public static int distinctSize = 0;
        //public static int[] parent = new int [N];
        //public static int[] size = new int[N];
        public static void master()
        {
            distinctColors = new List<RGBPixel>(GetDistinctPixels());
            distinctSize = distinctColors.Count;
            for (int i = 0; i < distinctSize; i++)
            {
                //Console.WriteLine(distinctColors[i].red + " " + distinctColors[i].green + " " + distinctColors[i].blue);
            }
            Console.WriteLine("===================================================================================");
            Console.WriteLine("===================================================================================");
            Console.WriteLine("===================================================================================");
            //Kruskal();
            Prim();
            MST.RemoveAt(0);
            double ans = 0;
            foreach(Edge e in MST)
            {
                ans += e.distance;
            }
            Console.WriteLine("Distinct Colors  = " + distinctSize);
            Console.WriteLine("MST SUM  = " + ans);
            //Cluster(3829);
            //Representatives();
            for(int i = 0; i < distinctColors.Count; i++)
            {
                //Console.WriteLine(distinctColors[i].red + " " + distinctColors[i].green + " " + distinctColors[i].blue);
            }
        }


        //FINDING DISTINCT COLORS
        public static List<RGBPixel> GetDistinctPixels()
        {
            RGBPixel[,] imageMatrix = ImageOperations.OpenImage(@"C:\Users\Lenovo\Documents\Algo\Project\[2] Image Quantization\Testcases\Testcases\Sample\Sample Test\Case4\Sample.Case4.bmp");
            List<RGBPixel> distinctColors = new List<RGBPixel>();
            int height = ImageOperations.GetHeight(imageMatrix);
            int width = ImageOperations.GetWidth(imageMatrix);

            bool[,,] isDistinct = new bool[256, 256, 256];
            for(int x = 0; x < height; x++)
            {
                for(int y = 0; y < width; y++)
                {
                    if (!(isDistinct[imageMatrix[x,y].red, imageMatrix[x, y].green, imageMatrix[x, y].blue]))
                    {
                        isDistinct[imageMatrix[x, y].red, imageMatrix[x, y].green, imageMatrix[x, y].blue] = true;
                        distinctColors.Add(imageMatrix[x, y]);
                    }
                }
            }
            return distinctColors;
        }
        //CALCULATE DISTANCE BETWEEN TWO PIXELS
        public static double CalculateDistance(RGBPixel u, RGBPixel v)
        {
            int red = (u.red - v.red) * (u.red - v.red);
            int blue = (u.blue - v.blue) * (u.blue - v.blue);
            int green = (u.green - v.green) * (u.green - v.green);
            return Math.Sqrt(red + blue + green);
        }



        public static void Prim()
        {


            // To represent set of vertices included in MST
            bool[] mstSet = new bool[distinctSize];
            PriorityQueue pq = new PriorityQueue();
            int[] vertices = new int[distinctSize];
            int[] parent = new int[distinctSize];
            for(int i = 1; i < distinctSize; i++)
            {
                pq.insert(new Cut(i, -double.MaxValue));
                mstSet[i] = false;
            }

            mstSet[0] = true;
            parent[0] = -1;
            pq.insert(new Cut(0, 0));

            for (int i = 0; i < distinctSize; i++)
            {
                // Pick the minimum key vertex from the
                // set of vertices not yet included in MST
                Cut u = pq.extractMax();

                // Add the picked vertex to the MST Set
                mstSet[u.index] = true;
                MST.Add(new Edge(parent[u.index], u.index, -1 * u.distance));
                // Update key value and parent index of
                // the adjacent vertices of the picked vertex.
                // Consider only those vertices which are not
                // yet included in MST
                for (int v = 0; v < distinctSize; v++) {
                    // graph[u][v] is non zero only for adjacent vertices of m
                    // mstSet[v] is false for vertices not yet included in MST
                    // Update the key only if graph[u][v] is smaller than key[v]
                    if (mstSet[v] == false) {
                        if(pq.changePriority(v, -CalculateDistance(distinctColors[u.index], distinctColors[v]))){
                            parent[v] = u.index;
                        }
                    }
                }
            }
        }
        public static void Cluster(int k)
        {
            MST.Sort((e1, e2) => e1.distance.CompareTo(e2.distance));
            int size = MST.Count;
            List<Edge> newEdges = new List<Edge>();

            for (int i = 0; i < k-1; i++)
            {
                newEdges.Add(MST[i]);
            }
            MST = new List<Edge>(newEdges);
        }

        public static void Representatives()
        {
            List<int>[] adjList = new List<int>[N];
            int size = distinctColors.Count;
            bool[] visited = new bool[N];
            //Initialize adjList
            for (int i = 0; i < size; i++)
            {
                adjList[i] = new List<int>();
                visited[i] = false;
            }
            //Construct graph
            foreach (Edge e in MST)
            {
                adjList[e.u].Add(e.v);
                adjList[e.v].Add(e.u);
            }
            //BFS
            for (int i = 0; i < size; i++)
            {
                if (!visited[i])
                {
                    visited[i] = true;
                    Queue<int> q = new Queue<int>();
                    q.Enqueue(i);
                    List<int> cluster = new List<int>();
                    cluster.Add(i);
                    while (q.Count != 0)
                    {
                        int cur = q.First();
                        q.Dequeue();
                        foreach (int child in adjList[i])
                        {
                            if (!visited[child])
                            {
                                visited[child] = true;
                                q.Enqueue(child);
                                cluster.Add(child);
                            }
                        }
                    }
                    ReplaceColors(cluster);
                }
            }
        }
        public static void ReplaceColors(List<int> cluster)
        {
            double avg = cluster.Average();
            RGBPixel rep;
            int totalBlue = 0, totalRed = 0, totalGreen = 0;
            for (int i = 0; i < cluster.Count; i++)
            {
                totalBlue += distinctColors[cluster[i]].blue;
                totalRed += distinctColors[cluster[i]].red;
                totalGreen += distinctColors[cluster[i]].green;
            }
            rep.blue = (byte)(totalBlue / cluster.Count);
            rep.red = (byte)(totalRed / cluster.Count);
            rep.green = (byte)(totalGreen / cluster.Count);

            for(int i = 0; i < cluster.Count; i++)
            {
                distinctColors[cluster[i]] = rep;
            }
        }
    }

}

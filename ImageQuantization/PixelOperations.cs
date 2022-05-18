using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Linq;

namespace ImageQuantization
{
    public class PixelOperations
    {


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
        public static int N = 1000000;
        public static List<Edge> edges = new List<Edge>();
        public static List<RGBPixel> distinctColors = new List<RGBPixel>();
        public static int[] parent = new int [N];
        public static int[] size = new int[N];
        public static void master()
        {
            distinctColors = new List<RGBPixel>(GetDistinctPixels());
            for (int i = 0; i < distinctColors.Count; i++)
            {
                Console.WriteLine(distinctColors[i].red + " " + distinctColors[i].green + " " + distinctColors[i].blue);
            }
            Console.WriteLine("===================================================================================");
            Console.WriteLine("===================================================================================");
            Console.WriteLine("===================================================================================");
            edges = ConstructEdges();
            Kruskal();
            Cluster(3);
            Representatives();
            for(int i = 0; i < distinctColors.Count; i++)
            {
                Console.WriteLine(distinctColors[i].red + " " + distinctColors[i].green + " " + distinctColors[i].blue);
            }
        }


        //FINDING DISTINCT COLORS
        //USING A HASHSET
        //TIME BOUNDED BY O(N^2), INSERTION INTO HASHSET IS O(1)
        public static HashSet<RGBPixel> GetDistinctPixels()
        {
            RGBPixel[,] imageMatrix = ImageOperations.OpenImage(@"C:\Users\Lenovo\Documents\Algo\Project\[2] Image Quantization\Testcases\Testcases\Sample\Sample Test\Case1\Sample.Case1.bmp");
            HashSet<RGBPixel> distinctColors = new HashSet<RGBPixel>();
            int height = ImageOperations.GetHeight(imageMatrix);
            int width = ImageOperations.GetWidth(imageMatrix);
            for(int x = 0; x < height; x++)
            {
                for(int y = 0; y < width; y++)
                {
                    distinctColors.Add(imageMatrix[x, y]);
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
        public static List<Edge> ConstructEdges()
        {
            List<Edge> edges = new List<Edge>();
            int size = distinctColors.Count;
            for (int u = 0; u < size; u++)
            {
                for (int v = 0; v < size; v++)
                {
                    if (u == v)
                        continue;
                    Edge e = new Edge(u, v, CalculateDistance(distinctColors[u], distinctColors[v]));
                    edges.Add(e);
                }
            }
            return edges;
        }

        public static void Kruskal()
        {
            edges.Sort((e1,e2) => e1.distance.CompareTo(e2.distance));
            List<Edge> newEdges = new List<Edge>();
            for(int i = 0; i < parent.Length; i++)
            {
                parent[i] = i;
                size[i] = 1;
            }
            foreach(Edge e in edges)
            {
                if(Join(e.u, e.v))
                {
                    newEdges.Add(e);
                }
            }
            edges = new List<Edge>(newEdges);
        }
        public static int Find(int node)
        {
            if (node == parent[node])
                return node;
            return parent[node] = Find(parent[node]);
        }
        public static bool Join(int u, int v)
        {
            u = Find(u);
            v = Find(v);
            if (parent[u] == parent[v])
                return false;
            if (size[u] < size[v]){
                int t = u; u = v; v = t;
            }
            parent[v] = u;
            size[u] += size[v];
            return true;
        }

        public static void Cluster(int k)
        {
            edges.Sort((e1, e2) => e1.distance.CompareTo(e2.distance));
            int size = edges.Count;
            List<Edge> newEdges = new List<Edge>();

            for (int i = 0; i < k-1; i++)
            {
                newEdges.Add(edges[i]);
            }
            edges = new List<Edge>(newEdges);
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
            foreach (Edge e in edges)
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

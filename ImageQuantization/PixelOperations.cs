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
        public List<Edge> MST = new List<Edge>();
        public List<RGBPixel> distinctColors = new List<RGBPixel>();
        public int distinctSize = 0;
        public double mstSUM;
        public RGBPixel[,] imageMatrix;
        public RGBPixel[,] newImageMatrix;
        public int[,] originalImageMapping;
        int height, width;

        public RGBPixel[,] Quantize(int k)
        {
            GetDistinctPixels();
            distinctSize = distinctColors.Count;

            Prim();
            MST.RemoveAt(0);
            Cluster(k);
            Representatives();
            Recolor();
            return newImageMatrix;
        }


        //FINDING DISTINCT COLORS
        public void GetDistinctPixels()
        {
            distinctColors = new List<RGBPixel>();
            height = ImageOperations.GetHeight(imageMatrix);
            width = ImageOperations.GetWidth(imageMatrix);
            originalImageMapping = new int[height, width];
            int n = 0;
            int[,,] isDistinct = new int[256, 256, 256];
            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 256; j++)
                {
                    for (int l = 0; l < 256; l++)
                    {
                        isDistinct[i, j, l] = -1;
                    }
                }
            }
            for (int x = 0; x < height; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    if ((isDistinct[imageMatrix[x, y].red, imageMatrix[x, y].green, imageMatrix[x, y].blue]) == -1)
                    {
                        isDistinct[imageMatrix[x, y].red, imageMatrix[x, y].green, imageMatrix[x, y].blue] = n++;
                        distinctColors.Add(imageMatrix[x, y]);
                    }
                    originalImageMapping[x, y] = isDistinct[imageMatrix[x, y].red, imageMatrix[x, y].green, imageMatrix[x, y].blue];
                }
            }
        }
        //CALCULATE DISTANCE BETWEEN TWO PIXELS
        //MEMORIZE COLOR INDEXES FOR RECOLORING
        public double CalculateDistance(RGBPixel u, RGBPixel v)
        {
            int red = (u.red - v.red) * (u.red - v.red);
            int blue = (u.blue - v.blue) * (u.blue - v.blue);
            int green = (u.green - v.green) * (u.green - v.green);
            return Math.Sqrt(red + blue + green);
        }

        //PRIM'S ALGORITHM 
        public void Prim()
        {
            bool[] mstSet = new bool[distinctSize];
            PriorityQueue pq = new PriorityQueue();
            int[] parent = new int[distinctSize];
            mstSet[0] = true;
            pq.insert(new Cut(0, 0));
            for (int i = 1; i < distinctSize; i++)
            {
                pq.insert(new Cut(i, double.MaxValue));
                mstSet[i] = false;
            }
            while (pq.size != 0)
            {
                Cut u = pq.extractMin();
                mstSet[u.index] = true;
                MST.Add(new Edge(parent[u.index], u.index, u.distance));
                for (int v = 0; v < distinctSize; v++)
                {
                    if (mstSet[v] == false)
                    {
                        if (pq.changePriority(v, CalculateDistance(distinctColors[u.index], distinctColors[v])))
                        {
                            parent[v] = u.index;
                        }
                    }
                }
            }
            for (int i = 0; i < distinctSize; i++)
            {
                if (mstSet[i] == false)
                {
                    Console.WriteLine(i);
                }
            }
        }

        //REMOVE K-1 EDGES TO FORM K CLUSTERS
        public void Cluster(int k)
        {
            MST.Sort((e1, e2) => e2.distance.CompareTo(e1.distance));
            int size = MST.Count;
            List<Edge> newEdges = new List<Edge>();

            for (int i = 0; i < k-1; i++)
            {
                MST.RemoveAt(0);
            }
        }

        //CONSTRUCT ONE GRAPH WHERE EACH COMPONENT REPRESENTS A CLUSTER
        //BFS OVER EACH COMPONENT, COLLECT AND REPLACE BY ONE REPRESENTATIVE COLOR
        public void Representatives()
        {
            List<int>[] adjList = new List<int>[1000000];
            int size = distinctColors.Count;
            bool[] visited = new bool[1000000];
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
                        foreach (int child in adjList[cur])
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
        //COMPUTE AVERAGE COLOR OF THE CLUSTER AND REPLACE IN DISTINCT COLORS LIST
        public void ReplaceColors(List<int> cluster)
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

            for (int i = 0; i < cluster.Count; i++)
            {
                distinctColors[cluster[i]] = rep;
            }
        }

        //USE THE ORIGINAL MAPPING TO RECOLOR THE IMAGE
        public void Recolor()
        {
            newImageMatrix = new RGBPixel[height, width];
            for (int x = 0; x < height; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    newImageMatrix[x, y] = distinctColors[originalImageMapping[x, y]];
                }
            }
        }
    }
}

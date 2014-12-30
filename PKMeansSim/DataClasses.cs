using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PKMeansSim
{
    public class DataSet
    {
        private List<Point> points;
        private List<Block> blocks;
        private DataGenerator dataGenerator;

        public List<Point> Points
        {
            get { return points; }
            set { points = value; }
        }

        public List<Block> Blocks
        {
            get { return blocks; }
        }

        public DataGenerator DataGenerator
        {
            get { return dataGenerator; }
            set { dataGenerator = value; }
        }


        public void DataGeneration(int pointNum)
        {
            System.Diagnostics.Debug.Assert(pointNum > 0);

            this.points = dataGenerator.GeneratePoints(pointNum);
        }

        public void DataSplit(int blockNum)
        {
            System.Diagnostics.Debug.Assert(points != null && points.Count > 0);
            System.Diagnostics.Debug.Assert(blockNum > 0 && blockNum < this.points.Count);

            int blockIdx = 0;

            BlockInitialize(blockNum);

            foreach (Point p in points)
            {
                blocks[blockIdx].Points.Add(p);
                if (blockIdx >= (blockNum - 1))
                    blockIdx = 0;
                else
                    blockIdx++;
            }
        }

        private void BlockInitialize(int blockNum)
        {
            blocks = new List<Block>();
            for (int i = 0; i < blockNum; i++)
            {
                blocks.Add(new Block());
            }
        }
    }

    public class Block
    {
        private List<Point> points;

        public List<Point> Points
        {
            get { return points; }
            set { points = value; }
        }

        public Block()
        {
            this.points = new List<Point>();
        }
    }

    public class Point
    {
        private double x;
        private double y;
        private Centroid assignCentroid;
        private double minDist;
        private Centroid minCentroid;

        public EventHandler<MapCodeEventArgs> MapCodeEvent;

        public double X
        {
            get { return x; }
            set { x = value; }
        }

        public double Y
        {
            get { return y; }
            set { y = value; }
        }

        public Centroid AssignCentroid
        {
            get { return assignCentroid; }
            set { assignCentroid = value; }
        }

        public double MinDist
        {
            get { return minDist; }
        }

        public Centroid MinCentroid
        {
            get { return minCentroid; }
        }

        public Point(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public Point(double x, double y, Centroid centroid)
        {
            this.x = x;
            this.y = y;
            this.AssignCentroid = centroid;
        }

        protected Point()
        {
        }

        public Centroid GetNearestCentroid(List<Centroid> C)
        {
            if (MapCodeEvent != null) MapCodeEvent(this, new MapCodeEventArgs(MapCode.L1Construct, null));
            if (MapCodeEvent != null) MapCodeEvent(this, new MapCodeEventArgs(MapCode.L2InitMinDis, null));
            minDist = Math.Max(Config.maxX, Config.maxY) * 2;
            if (MapCodeEvent != null) MapCodeEvent(this, new MapCodeEventArgs(MapCode.L3InitIndex, null));
            minCentroid = null;

            if (MapCodeEvent != null) MapCodeEvent(this, new MapCodeEventArgs(MapCode.L4ForStart, null));
            foreach (Centroid c in C)
            {
                if (MapCodeEvent != null) MapCodeEvent(this, new MapCodeEventArgs(MapCode.L5ComputeDist, c));
                double dist = this.GetDist(c);
                if (MapCodeEvent != null) MapCodeEvent(this, new MapCodeEventArgs(MapCode.L6DistCheck, c));
                if (dist < minDist)
                {
                    if (MapCodeEvent != null) MapCodeEvent(this, new MapCodeEventArgs(MapCode.L7SetMinDis, c));
                    minDist = dist;
                    if (MapCodeEvent != null) MapCodeEvent(this, new MapCodeEventArgs(MapCode.L8SetIndex, c));
                    minCentroid = c;
                }
            }
            if (MapCodeEvent != null) MapCodeEvent(this, new MapCodeEventArgs(MapCode.L9ForEnd, minCentroid));
            if (MapCodeEvent != null) MapCodeEvent(this, new MapCodeEventArgs(MapCode.L10IndexAsKey2, minCentroid));
            if (MapCodeEvent != null) MapCodeEvent(this, new MapCodeEventArgs(MapCode.L11SetValue2, minCentroid));
            if (MapCodeEvent != null) MapCodeEvent(this, new MapCodeEventArgs(MapCode.L12OutputKV2, minCentroid));
            return minCentroid;
        }

        public double GetDist(Point p)
        {
            double dist = 0;
            dist += (p.x - this.x)*(p.x - this.x);
            dist += (p.y - this.y)*(p.y - this.y);
            return Math.Sqrt(dist);
        }

        public void Add(Point point)
        {
            this.x += point.x;
            this.y += point.y;
        }

        internal Point Divide(int cnt)
        {
            this.x /= cnt;
            this.y /= cnt;
            return this;
        }
    }

    public class Centroid : Point
    {
        private int id;

        public int Id
        {
            get { return id; }
        }

        public Centroid(double x, double y)
            : base(x, y)
        {
            this.X = x;
            this.Y = y;
        }

        public Centroid(double x, double y, int id)
        {
            this.X = x;
            this.Y = y;
            this.id = id;
        }

        internal void SetNewCentroid(Point point)
        {
            this.X = point.X;
            this.Y = point.Y;
        }

        internal void SetNewCentroid(List<Point> points)
        {
            Point sumPoint = new Point(0, 0);
            int cnt = 0;
            foreach (Point p in points)
            {
                sumPoint.Add(p);
                cnt++;
            }
            this.SetNewCentroid(sumPoint.Divide(cnt));
        }
    }

    public class Cluster
    {
        private Centroid centroid;
        private List<Point> points;

        public Centroid Centroid
        {
            get { return centroid; }
            set { centroid = value; }
        }

        public List<Point> Points
        {
            get { return points; }
            set { points = value; }
        }

        public Cluster()
        {
            this.points = new List<Point>();
        }
    }
}

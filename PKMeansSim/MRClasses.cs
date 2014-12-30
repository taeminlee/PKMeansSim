using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace PKMeansSim
{
    class MasterNode
    {
        private DataSet dataSet;
        private List<Centroid> centroids;
        private List<DataNode> dataNodes;
        private List<Cluster> clusters;
        private List<Point> displayPoints;

        private AutoResetEvent autoResetEvent = new AutoResetEvent(true);

        private MRState state;
        public event EventHandler KMeansCompleted;
        public event EventHandler<MRState> StateChanged;
        public event EventHandler<Centroid> CentroidChanged;

        public DataSet DataSet
        {
            get { return dataSet; }
        }

        public List<Centroid> Centroids
        {
            get { return centroids; }
        }

        public List<DataNode> DataNodes
        {
            get { return dataNodes; }
        }

        public List<Cluster> Clusters
        {
            get { return clusters; }
        }

        public MRState State
        {
            get { return state; }
            set
            {
                state = value;
                if (StateChanged != null) StateChanged(this, value);
            }
        }

        public List<Point> DisplayPoints
        {
            get { return displayPoints; }
        }

        public AutoResetEvent AutoResetEvent
        {
            get { return autoResetEvent; }
        }

        public MasterNode(DataSet dataSet)
        {
            this.State = MRState.MasterNodeInit;
            this.dataSet = dataSet;
            this.dataNodes = new List<DataNode>();   
            foreach (Block b in dataSet.Blocks)
            {
                DataNode dataNode = new DataNode(b, this.centroids);
                dataNodes.Add(dataNode);
            }
            InitializeCentroidSet(Config.CentroidGenerateMethod);
            this.clusters = new List<Cluster>();
            foreach (Centroid centroid in this.centroids)
            {
                Cluster cluster = new Cluster();
                cluster.Centroid = centroid;
                this.clusters.Add(cluster);
            }
        }

        public void DataNodeNext()
        {
            foreach (DataNode dataNode in dataNodes)
            {
                dataNode.AutoResetEvent.Set();
            }
        }

        public void KMeans()
        {
            List<Cluster> beforeCluster = null;
            Task[] tasks = new Task[dataNodes.Count];
            do
            {
                this.State = MRState.StartMRJob;
                beforeCluster = CopyClusters(clusters);
                this.State = MRState.MapStart;
                for (int DNIdx = 0; DNIdx < dataNodes.Count; DNIdx++)
                {
                    DataNode dataNode = dataNodes[DNIdx];
                    dataNode.UpdatePartialCentroids(centroids);
                    tasks[DNIdx] = Task.Factory.StartNew(dataNode.Map);
                }
                this.State = MRState.MapEnd;
                Task.WaitAll(tasks);
                Reduce();
            } while (!EndConditionCheck(beforeCluster, clusters));

            if (KMeansCompleted != null) KMeansCompleted(this, null);
        }

        private List<Cluster> CopyClusters(List<Cluster> clusters)
        {
            List<Cluster> cloneClusters = new List<Cluster>();
            foreach (Cluster cluster in clusters)
            {
                Cluster clone = new Cluster();
                clone.Centroid = cluster.Centroid;
                clone.Points = new List<Point>();
                clone.Points.AddRange(cluster.Points);
                cloneClusters.Add(clone);
            }
            return cloneClusters;
        }

        private bool EndConditionCheck(List<Cluster> beforeClusters, List<Cluster> afterClusters)
        {
            System.Diagnostics.Debug.Assert(beforeClusters.Count == afterClusters.Count);

            for (int idx = 0; idx < beforeClusters.Count; idx++)
            {
                Cluster beforeCluster = beforeClusters[idx];
                Cluster afterCluster = afterClusters[idx];
                
                if (beforeCluster.Points.Count != afterCluster.Points.Count) return false;

                foreach (Point point in afterCluster.Points)
                {
                    beforeCluster.Points.Remove(point);
                }

                if (beforeCluster.Points.Count > 0) return false;
            }

            return true;
        }

        public void Reduce()
        {
            this.State = MRState.ReduceStart;
            foreach (Cluster cluster in clusters)
            {
                cluster.Points.Clear();
            }
            foreach (DataNode dataNode in dataNodes)
            {
                foreach (Cluster DNCluster in dataNode.Clusters.Values) // DNCluster = Cluster of data node
                {
                    Cluster cluster = FindClusterByCentroidID(DNCluster.Centroid.Id);
                    cluster.Points.AddRange(DNCluster.Points);
                }
            }
            this.displayPoints = new List<Point>(dataSet.Points.Count);
            foreach (Point point in dataSet.Points)
            {
                this.displayPoints.Add(new Point(point.X, point.Y, point.AssignCentroid));
            }
            this.State = MRState.ReduceCombinePoints;
            foreach (Cluster cluster in clusters)
            {
                autoResetEvent.WaitOne();
                cluster.Centroid.SetNewCentroid(cluster.Points);
                Centroid centroid = FindCentroidByCentroidID(cluster.Centroid.Id);
                centroid.SetNewCentroid(cluster.Centroid);
                if (CentroidChanged != null) CentroidChanged(this, centroid);
            }
            this.State = MRState.ReduceEnd;
        }

        private Cluster FindClusterByCentroidID(int id)
        {
            return this.clusters.Find(cluster => cluster.Centroid.Id == id);
        }

        private Centroid FindCentroidByCentroidID(int id)
        {
            return this.centroids.Find(centroid => centroid.Id == id);
        }

        public void InitializeCentroidSet(CentroidGenerateMethods method)
        {
            this.State = MRState.CentroidSetGenerate;
            switch (method)
            {
                case CentroidGenerateMethods.Random:
                    this.centroids = MakeRandomCentroidSets(this.dataSet, Config.k);
                    break;
                case CentroidGenerateMethods.KMeansPlusPlus:
                    throw new NotImplementedException();
                    //break;
                case CentroidGenerateMethods.KMeansBarBar:
                    throw new NotImplementedException();
                    //break;
                default:
                    throw new ArgumentOutOfRangeException("method");
            }
        }

        private List<Centroid> MakeRandomCentroidSets(PKMeansSim.DataSet dataSet, int clusterSize)
        {
            int itemCnt = dataSet.Points.Count;
            Random rand = new Random();
            List<Centroid> centroids = new List<Centroid>();
            for (int id = 0; id < clusterSize; id++)
            {
                int pointIdx = rand.Next(itemCnt);
                System.Diagnostics.Debug.Assert(pointIdx < dataSet.Points.Count);
                Point point = dataSet.Points[pointIdx];
                Centroid centroid = new Centroid(point.X, point.Y, id);
                centroids.Add(centroid);
            }
            return centroids;
        }
    }

    public class DataNode
    {
        private List<Point> points;
        private List<Centroid> partialCentroids;
        private Dictionary<Centroid, Cluster> clusters;

        private AutoResetEvent autoResetEvent = new AutoResetEvent(false);

        private MRState state;
        public event EventHandler<Point> PointAssignCluster;
        public event EventHandler<Centroid> CentroidChanged;
        public event EventHandler<MRState> StateChanged;

        public List<Point> Points
        {
            get { return points; }
        }

        public List<Centroid> PartialCentroids
        {
            get { return partialCentroids; }
            set { partialCentroids = value; }
        }

        public Dictionary<Centroid, Cluster> Clusters
        {
            get { return clusters; }
            set { clusters = value; }
        }

        public AutoResetEvent AutoResetEvent
        {
            get { return autoResetEvent; }
        }

        public MRState State
        {
            get { return state; }
            set
            {
                state = value;
                if (StateChanged != null) StateChanged(this, value);
            }
        }

        public DataNode(Block block, List<Centroid> centroids)
        {
            System.Diagnostics.Debug.Assert(block != null && block.Points != null && block.Points.Count > 0);

            this.points = block.Points;
            UpdatePartialCentroids(centroids);
        }

        public void UpdatePartialCentroids(List<Centroid> centroids)
        {
            if (centroids == null) return;

            foreach (Point point in points)
            {
                point.AssignCentroid = null;
            }
            this.partialCentroids = centroids.ConvertAll(original => new Centroid(original.X, original.Y, original.Id)); // Copy

            InitClusters(partialCentroids);

            this.State = MRState.DataNodeInit;
        }

        private void InitClusters(List<Centroid> centroids)
        {
            clusters = new Dictionary<Centroid, Cluster>();
            foreach (Centroid centroid in centroids)
            {
                Cluster cluster = new Cluster();
                cluster.Points = new List<Point>();
                cluster.Centroid = centroid;
                clusters.Add(centroid, cluster);
            }
        }

        public void Map()
        {
            this.State = MRState.MapStart;
            foreach (Point p in points)
            {
                autoResetEvent.WaitOne();
                Centroid centroid = p.GetNearestCentroid(PartialCentroids);
                p.AssignCentroid = centroid;
                if (PointAssignCluster != null) PointAssignCluster(this, p);
                clusters[centroid].Points.Add(p);
            }
            this.State = MRState.MapEnd;
            Combine();
        }

        public void Combine()
        {
            this.State = MRState.CombineStart;
            foreach (Centroid centroid in partialCentroids)
            {
                autoResetEvent.WaitOne();
                centroid.SetNewCentroid(clusters[centroid].Points);
                if (CentroidChanged != null) CentroidChanged(this, centroid);
            }
            this.State = MRState.CombineEnd;
        }
    }
}

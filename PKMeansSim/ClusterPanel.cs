using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace PKMeansSim
{
    public partial class ClusterPanel : UserControl
    {
        private List<Point> points;
        private List<Centroid> centroids;
        private DataNode dataNode;
        private Dictionary<Point, int> assignedPoints;

        public List<Point> Points
        {
            get { return points; }
            set
            {
                points = value;
                if (this.chart1.InvokeRequired)
                {
                    this.chart1.BeginInvoke(new Action(() => SetPoints()));
                }
                else
                {
                    if(points != null)
                    SetPoints();
                }
            }
        }

        public List<Centroid> Centroids
        {
            get { return centroids; }
            set
            {
                centroids = value;
                if (this.chart1.InvokeRequired)
                {
                    this.chart1.BeginInvoke(new Action(() => SetCentroids()));
                }
                else
                {
                    if (centroids != null)
                    SetCentroids();
                }
            }
        }

        public DataNode DataNode
        {
            get { return dataNode; }
            set { dataNode = value; }
        }

        private void SetCentroids()
        {
            Series series = this.chart1.Series["centroids"];
            series.Points.Clear();

            int id = -1;

            foreach (Centroid centroid in centroids)
            {
                id = series.Points.AddXY(centroid.X, centroid.Y);
                series.Points[id].Color = Utils.GetCentroidColor(id);
            }
        }

        public ClusterPanel()
        {
            InitializeComponent();
            SetText(MR.MasterNode);
            InitChart();
        }

        public ClusterPanel(DataNode dataNode)
        {
            InitializeComponent();
            SetText(MR.DataNode);
            InitChart();
            this.dataNode = dataNode;
        }

        public void InitChart()
        {
            System.Diagnostics.Debug.Assert(this.chart1.ChartAreas.Count > 0);
            System.Diagnostics.Debug.Assert(this.chart1.Series.Count > 0);

            ChartArea chartArea = this.chart1.ChartAreas.First();
            chartArea.AxisX.Minimum = Config.minX;
            chartArea.AxisX.Maximum = Config.maxX;
            chartArea.AxisY.Minimum = Config.minY;
            chartArea.AxisY.Maximum = Config.maxY;

            Series series = this.chart1.Series["points"];
            series.Points.Clear();

            series = this.chart1.Series["centroids"];
            series.Points.Clear();

            assignedPoints = new Dictionary<Point, int>();
        }

        private void SetText(MR masterNode)
        {
            switch (masterNode)
            {
                case MR.MasterNode:
                    this.button1.BackColor = DefaultBackColor;
                    this.button1.Text = "Data Generation";
                    this.button2.BackColor = DefaultBackColor;
                    this.button2.Text = "Reduce";
                    this.button3.BackColor = DefaultBackColor;
                    this.button3.Text = "Complete";
                    break;
                case MR.DataNode:
                    this.button1.BackColor = DefaultBackColor;
                    this.button1.Text = "Map";
                    this.button2.BackColor = DefaultBackColor;
                    this.button2.Text = "Combine";
                    this.button3.BackColor = DefaultBackColor;
                    this.button3.Text = "Complete";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("masterNode");
            }
        }

        private void SetPoints()
        {
            Series series = this.chart1.Series["points"];
            series.Points.Clear();

            int id = -1;

            foreach (Point point in points)
            {
                id = series.Points.AddXY(point.X, point.Y);
                if (point.AssignCentroid != null)
                {
                    series.Points[id].Color = Utils.GetPointColor(point.AssignCentroid.Id);
                }
                else
                {
                    series.Points[id].Color = Color.Gray;
                }
                assignedPoints[point] = id;
            }
        }

        internal void UpdatePoint(Point point)
        {
            if (this.chart1.InvokeRequired)
            {
                this.chart1.BeginInvoke(new Action(() => UpdatePoint(point)));
            }
            else
            { 
                Series series = this.chart1.Series["points"];
                if (point.AssignCentroid != null)
                { 
                    series.Points[assignedPoints[point]].Color = Utils.GetPointColor(point.AssignCentroid.Id);
                }
                else
                {
                    series.Points[assignedPoints[point]].Color = Color.Gray;
                }
            }
        }

        internal void SetLabel(int p)
        {
            switch (p)
            {
                case 1:
                    button1.BackColor = Color.LightGreen;
                    button1.ForeColor = Color.Black;
                    button2.BackColor = DefaultBackColor;
                    button2.ForeColor = DefaultForeColor;
                    button3.BackColor = DefaultBackColor;
                    button3.ForeColor = DefaultForeColor;
                    break;
                case 2:
                    button1.BackColor = DefaultBackColor;
                    button1.ForeColor = DefaultForeColor;
                    button2.BackColor = Color.LightGreen;
                    button2.ForeColor = Color.Black;
                    button3.BackColor = DefaultBackColor;
                    button3.ForeColor = DefaultForeColor;
                    break;
                case 3:
                    button1.BackColor = DefaultBackColor;
                    button1.ForeColor = DefaultForeColor;
                    button2.BackColor = DefaultBackColor;
                    button2.ForeColor = DefaultForeColor;
                    button3.BackColor = Color.LightGreen;
                    button3.ForeColor = Color.Black;
                    break;
                case 0:
                    button1.BackColor = DefaultBackColor;
                    button1.ForeColor = DefaultForeColor;
                    button2.BackColor = DefaultBackColor;
                    button2.ForeColor = DefaultForeColor;
                    button3.BackColor = DefaultBackColor;
                    button3.ForeColor = DefaultForeColor;
                    break;
            }
        }
    }
}

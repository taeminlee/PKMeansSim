using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PKMeansSim
{
    public partial class MainForm : Form
    {
        private int loopCounter;
        private Timer masterTimer = new Timer();
        private Timer dataNodeTimer = new Timer();
        private DataSet dataSet;
        private MasterNode master;
        private List<ClusterPanel> dataNodePanels = new List<ClusterPanel>();

        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            masterTimer.Interval = 100;
            masterTimer.Tick += masterTimer_Tick;
            dataNodeTimer.Interval = 10;
            dataNodeTimer.Tick += dataNodeTimer_Tick;
            GenerateDataSet();
        }

        private void GenerateDataSet()
        {
            dataSet = new DataSet();
            switch (Config.DataGenerateMethods)
            {
                case DataSetGenerateMethods.Uniform:
                    dataSet.DataGenerator = new UniformGenerator();
                    break;
                case DataSetGenerateMethods.KBlock:
                    dataSet.DataGenerator = new KBlockGenerator();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            dataSet.DataGeneration(Config.n);
            Logging(String.Format("DataSetGeneration : {0} blocks", Config.b));
            this.clusterPanel1.SetLabel(1);
            this.clusterPanel1.Points = dataSet.Points;
            this.clusterPanel1.Centroids = new List<Centroid>();
            Logging(String.Format("DataSetGeneration : {0} items", Config.n));
            dataSet.DataSplit(Config.b);
            master = new MasterNode(dataSet);
            SetDataNodeGrid();
        }

        private void Start()
        {
            masterTimer.Start();
            dataNodeTimer.Start();
            loopCounter = 0;
            //dataSet.DataSplit(Config.b);
            richTextBox1.AppendText(String.Format("MasterNode Init", Config.b) + Environment.NewLine);
            master = new MasterNode(dataSet);
            master.StateChanged += master_StateChanged;
            master.KMeansCompleted += master_KMeansCompleted;
            master.CentroidChanged += master_CentroidChanged;
            foreach (DataNode dataNode in master.DataNodes)
            {
                dataNode.PointAssignCluster += dataNode_PointAssignCluster;
                dataNode.CentroidChanged += dataNode_CentroidChanged;
                dataNode.StateChanged += dataNode_StateChanged;
            }
            this.clusterPanel1.Centroids = master.Centroids;
            SetDataNodeGrid();
            Task.Factory.StartNew(master.KMeans);

            startBtn.Enabled = false;
        }

        private void Pause()
        {
            if (masterTimer.Enabled && dataNodeTimer.Enabled)
            {
                masterTimer.Stop();
                dataNodeTimer.Stop();
                this.pauseBtn.Text = "Resume";
            }
            else
            {
                masterTimer.Start();
                dataNodeTimer.Start();
                this.pauseBtn.Text = "Pause";
            }
        }

        private void Next()
        {
            if (masterTimer.Enabled && dataNodeTimer.Enabled)
            {
                masterTimer.Stop();
                dataNodeTimer.Stop();
                masterTimer.Start();
                dataNodeTimer.Start();
            }
            master.AutoResetEvent.Set();
            master.DataNodeNext();
        }

        private void SetDataNodeGrid()
        {
            this.tableLayoutPanel1.Controls.Clear();
            this.dataNodePanels.Clear();

            foreach (DataNode dataNode in master.DataNodes)
            {
                this.dataNodePanels.Add(new ClusterPanel(dataNode));
            }

            int grid = (int)Math.Ceiling(Math.Sqrt(master.DataNodes.Count));
            tableLayoutPanel1.ColumnCount = grid;
            tableLayoutPanel1.RowCount = grid;
            this.tableLayoutPanel1.ColumnStyles.Clear();
            this.tableLayoutPanel1.RowStyles.Clear();
            for (int x = 0; x < grid; x++)
            {
                this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, (float)(1.0 / grid)));
                this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, (float)(1.0 / grid)));
            }

            int panelCnt = 0;
            for(int x = 0; x < grid; x++)
            {
                for(int y = 0; y < grid; y++)
                {
                    if (panelCnt >= master.DataNodes.Count) break;

                    DataNode dataNode = master.DataNodes[panelCnt];

                    this.tableLayoutPanel1.Controls.Add(dataNodePanels[panelCnt], x, y);
                    dataNodePanels[panelCnt].Dock = DockStyle.Fill;
                    dataNodePanels[panelCnt].Points = dataNode.Points;
                    dataNodePanels[panelCnt].Centroids = dataNode.PartialCentroids;

                    panelCnt++;
                }
            }
        }

        void masterTimer_Tick(object sender, EventArgs e)
        {
            master.AutoResetEvent.Set();
        }

        void dataNodeTimer_Tick(object sender, EventArgs e)
        {
            master.DataNodeNext();
        }

        void master_StateChanged(object sender, MRState e)
        {
            switch (e)
            {
                case MRState.MasterNodeInit:
                    this.clusterPanel1.Points = master.DataSet.Points;
                    this.clusterPanel1.SetLabel(1);
                    break;
                case MRState.StartMRJob:
                    loopCounter++;
                    this.clusterPanel1.SetLabel(0);
                    Logging(String.Format("LOOP COUNTER : {0}", loopCounter));
                    break;
                case MRState.MapStart:
                    break;
                case MRState.MapEnd:
                    break;
                case MRState.CentroidSetGenerate:
                    this.clusterPanel1.SetLabel(2);
                    this.clusterPanel1.Centroids = master.Centroids;
                    break;
                case MRState.ReduceStart:
                    this.clusterPanel1.SetLabel(2);
                    break;
                case MRState.ReduceCombinePoints:
                    this.clusterPanel1.SetLabel(2);
                    this.clusterPanel1.Points = master.DisplayPoints;
                    break;
                case MRState.ReduceEnd:
                    this.clusterPanel1.SetLabel(2);
                    break;
                case MRState.Finish:
                    this.clusterPanel1.SetLabel(3);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("e");
            }
            
            Logging(String.Format("MasterNode State Changed : {0}", e));
        }

        void master_CentroidChanged(object sender, Centroid e)
        {
            this.clusterPanel1.Centroids = master.Centroids;
        }

        void master_KMeansCompleted(object sender, EventArgs e)
        {
            this.clusterPanel1.SetLabel(3);
            Logging(String.Format("K-Means Completed"));
            masterTimer.Stop();
            SetBtns();
        }

        void SetBtns()
        {
            if (startBtn.InvokeRequired)
            {
                startBtn.BeginInvoke(new Action(() => SetBtns()));
            }
            else
            {
                startBtn.Enabled = true;
            }
        }

        void dataNode_StateChanged(object sender, MRState e)
        {
            DataNode dataNode = (DataNode)sender;
            ClusterPanel clusterPanel = dataNodePanels.Find(panel => panel.DataNode == dataNode);
            if (clusterPanel == null) return;
            switch (e)
            {
                case MRState.MasterNodeInit:
                    break;
                case MRState.StartMRJob:
                    break;
                case MRState.CentroidSetGenerate:
                    break;
                case MRState.DataNodeInit:
                    break;
                case MRState.MapStart:
                    clusterPanel.SetLabel(1);
                    clusterPanel.Points = dataNode.Points;
                    break;
                case MRState.MapEnd:
                    break;
                case MRState.CombineStart:
                    clusterPanel.SetLabel(2);
                    break;
                case MRState.CombineEnd:
                    clusterPanel.SetLabel(3);
                    break;
                case MRState.ShufflingStart:
                    break;
                case MRState.ShufflingEnd:
                    break;
                case MRState.ReduceStart:
                    break;
                case MRState.ReduceEnd:
                    break;
                case MRState.Finish:
                    break;
                case MRState.ReduceCombinePoints:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("e");
            }

            Logging(String.Format("DataNode State Changed : {0}", e));
        }

        void dataNode_CentroidChanged(object sender, Centroid e)
        {
            DataNode dataNode = (DataNode)sender;
            ClusterPanel clusterPanel = dataNodePanels.Find(panel => panel.DataNode == dataNode);
            clusterPanel.Centroids = dataNode.PartialCentroids;
        }

        void dataNode_PointAssignCluster(object sender, Point e)
        {
            DataNode dataNode = (DataNode)sender;
            ClusterPanel clusterPanel = dataNodePanels.Find(panel => panel.DataNode == dataNode);
            if (clusterPanel == null) return;
            clusterPanel.UpdatePoint(e);
        }

        private void Logging(string p)
        {
            if (this.richTextBox1.InvokeRequired)
            {
                this.richTextBox1.BeginInvoke(new Action(() => Logging(p)));
            }
            else
            {
                richTextBox1.AppendText(System.DateTime.Now.ToString("yyyy MMMM dd hh:mm:ss") + " : " + p + Environment.NewLine);
                richTextBox1.ScrollToCaret();
            }
        }

        private void optionBtn_Click(object sender, EventArgs e)
        {
            OptionForm optionForm = new OptionForm();
            optionForm.ShowDialog();
            this.clusterPanel1.InitChart();
            GenerateDataSet();
        }

        private void dataflowBtn_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms["DataFlowForm"] == null)
            {
                DataFlowForm dataFlowForm = new DataFlowForm();
                dataFlowForm.TopLevel = true;
                dataFlowForm.Show();
            }
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            masterTimer.Interval = trackBar1.Value;
            dataNodeTimer.Interval = trackBar1.Value;
        }

        private void dataGenBtn_Click_1(object sender, EventArgs e)
        {
            GenerateDataSet();
        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            Start();
        }

        private void pauseBtn_Click(object sender, EventArgs e)
        {
            if (pauseBtn.InvokeRequired)
            {
                pauseBtn.BeginInvoke(new Action(() => Pause()));
            }
            else
            {
                Pause();
            }
        }

        private void nextBtn_Click(object sender, EventArgs e)
        {
            if (nextBtn.InvokeRequired)
            {
                nextBtn.BeginInvoke(new Action(() => Next()));
            }
            else
            {
                Next();
            }
        }
    }
}

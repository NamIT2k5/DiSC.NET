using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Office.Interop.Excel;
using BasicNet;
using NetSimulation.Community;
using System.Collections.Generic;
using System.Collections;
using MatrixLibrary;
using System.Diagnostics;
using System.Linq;
using System.Data.OleDb;
using System.Data;
using NetSimulation.Lib;
using Mathutil;

//using Microsoft.Office.Core;
using Application = Microsoft.Office.Interop.Excel.Application;
using Excel = Microsoft.Office.Interop.Excel;
using Label = System.Windows.Forms.Label;

namespace NetSimulation
{
   

    public partial class MainForm : Form
    {
        private int NumberOfNetwork
        {
            get
            {
                return (int)nudNetNum.Value;
            }
        }
      
        public MainForm()
        {
            InitializeComponent();
        }

       
        private ExcelDB CreateAnalysisReport()
        {
            return new ExcelDB(new string[] { "Node count", "Interaction", "Centrality", "The number of clusters", "Modularity", "Robustness", "Network Recovery", "Module Recovery", "None-Module Recovery", "LockingRecovery"}, DateTime.Now.ToString("dd.MM.yyyy"));
        }
        private ExcelDB CreateGraphData(string GraphName)
        {
            return new ExcelDB(new string[] { "Start", "End", "Weight" }, GraphName);
        }

        
        string oldbntStartCalText = null;
        private void _baNetworkBtn_Click(object sender, EventArgs e)
        {
            _baResult.Text = "";
            _baProgress.Value = 0;
            _baProgress.Maximum = NumberOfNetwork;
            _baProgress.Minimum = 0;
            _baProgress.Step = 1;
            if (!(threadSFGenerating != null && threadSFGenerating.IsAlive))
            {
                threadSFGenerating = new Thread(GenerateScaleFreeNetwork);
                threadSFGenerating.Priority = ThreadPriority.Highest;
                threadSFGenerating.IsBackground = true;//for automatically killing this background thread when the main thread is terminated
                threadSFGenerating.Start();
                oldbntStartCalText = bntStartCal.Text;
                bntStartCal.Text = "Stop";
            }
            else
            {
                if(threadSFGenerating!=null)
                threadSFGenerating.Abort();
                threadSFGenerating = null;
                bntStartCal.Text = oldbntStartCalText;
            }
            
        }
       
     
        Thread threadSFGenerating = null;
        private void GenerateScaleFreeNetwork()
        {
            int nNet = NumberOfNetwork,
                    nNodeFrom = (int)nudNodeFrom.Value,
                    nNodeTo = (int)nudNodeTo.Value,
                    nMinLink = (int)nudMinLink.Value,
                    nMaxLink = (int)nudMaxLink.Value;
            if (nNodeFrom > nNodeTo)
                throw new Exception("The node range is invalid!");
            if(nMinLink> nMaxLink)
                throw new Exception("The link range is invalid!");


            ComplexNetGenerator GeneratingNet = new BasicNet.ComplexNetGenerator();

            ExcelDB excel = CreateAnalysisReport();
            BooleanNetwork temp = new BooleanNetwork();
            int Row=ExcelDB.DataRowStart;
            try
            {
                SetEnableCtrl(nudNetNum, false);
                User.One.MessageToUser("Start generating network data on "+DateTime.Now.ToString());
                
                int i=0;
                //foreach node
                for (int j = nNodeFrom; j <= nNodeTo; j = j >= nNodeTo ? nNodeFrom : (j + 1))
                {
                    if(i > nNet) break;
                    //foreach link
                    for (int k = nMinLink; k <= nMaxLink; k++)
                    {
                        if (++i > nNet) break;

                        BooleanNetwork sf = (BooleanNetwork)GeneratingNet.generateDirectedNetworkByPreferentialAttachment(temp, j, k);

                        float centrality = sf.DegreeCentrality;
                        double mutationNetRecovery = sf.NetworkRecovery(new Perturbation());
                        
                        double funtionRecovery = sf.NetworkRecovery(new Perturbation(Perturbation.Kind.ChangedFunction));

                        double multationRobustness = sf.NetworkRobustness(new Perturbation(Perturbation.Kind.Mutation));
                        //double lockingRobustness = sf.NetworkRobustness(new Perturbation(Perturbation.Kind.Locking));
                        
                        Dictionary<Node, int> Cluster = null;
                        double modularity = sf.modularity(ref Cluster);
                        int nNode = sf.Nodes.Count();
                        int nInteraction = sf.Arcs.Count();
                        var nCluster = from p in Cluster
                                        group p by p.Value into g
                                        select g;
                        double dModuleRecovery = 0, dNoneModuleRecovery=0;
                        sf.InOutModuleRobustness(Cluster, new Perturbation(), ref dModuleRecovery, ref dNoneModuleRecovery);

                        //"Node count", "Interaction", "Centrality", "The number of clusters", "Modularity", "Robustness", "Network Recovery","Module Recovery","None-Module Recovery", "LockingRecovery" 
                        var f = new object[] { nNode, nInteraction, centrality, nCluster.Count(), modularity, multationRobustness, mutationNetRecovery, dModuleRecovery, dNoneModuleRecovery, funtionRecovery };

                        excel.WriteRow(Row++, f);

                        SetProgressStep(_baProgress, i);
                        SetTextCtrl(labnNet, i.ToString() + "/" + nNet.ToString());
                        
                    }
                }
                SetProgressStep(_baProgress, 0);
                User.One.MessageToUser("Create a file " + excel.SaveToFile(txtReportFile.Text) + " on " + DateTime.Now.ToString());

            }
            catch (ThreadAbortException)
            {

                SetEnableCtrl(nudNetNum, true);
                User.One.MessageToUser("Aborted the network generating task");
                

            }
            catch (Exception ex)
            {
               
                MessageBox.Show(ex.StackTrace);
            }
            SetEnableCtrl(nudNetNum, true);
            SetTextCtrl(bntStartCal, oldbntStartCalText);
            
            
        }

        #region Multi-thread interacts with controls

        private delegate void SetTextDelegate(Control label, String result);
        private void SetTextCtrl(Control label, String result)
        {
            if (label.InvokeRequired)
            {
                //re-call this function in the invoke if invoke is required
                Invoke(new SetTextDelegate(SetTextCtrl), new object[] { label, result });
                return;
            }
            label.Text = result;
        }
        private delegate void SetEnableDelegate(Control ctrl, bool val);
        private void SetEnableCtrl(Control ctrl, bool val)
        {
            if (ctrl.InvokeRequired)
            {
                Invoke(new SetEnableDelegate(SetEnableCtrl), new object[] { ctrl, val });
                return;
            }
            ctrl.Enabled = val;
        }
        private delegate void SetProgressDelegate(ProgressBar pb, int step);
        private void SetProgressStep(ProgressBar pb, int step)
        {
            if (pb.InvokeRequired)
            {
                Invoke(new SetProgressDelegate(SetProgressStep), new object[] { pb, step });
                return;
            }
            pb.Value = step;
        }
        #endregion
        

       
        private void nudNode_ValueChanged(object sender, EventArgs e)
        {

            nudMaxLink.Maximum = nudMinLink.Maximum = nMaxLink;
            nudMaxLink.Minimum = nudMinLink.Minimum = nMinLink;

        }
        public decimal nMinLink
        {
            get
            {
                decimal nNode = nudNodeTo.Value;
                labMinLink.Text = "["+ComplexNetGenerator.nMinSFLink((int)nNode).ToString();
                return ComplexNetGenerator.nMinSFLink((int)nNode);
            }
        }
        public decimal nMaxLink
        {
            get
            {
                decimal nNode = nudNodeFrom.Value;

                labMaximumLink.Text = ComplexNetGenerator.nMaxSFDLink((int)nNode).ToString() + "]";
                
                txtReportFile.Text = "Network." + nudNodeFrom.Value.ToString() + "." + nudNodeTo.Value.ToString() + "." + nudMinLink.Value.ToString() + "." + nudMaxLink.Value.ToString() + ".xlsx";
                return ComplexNetGenerator.nMaxSFDLink((int)nNode);
            }
        }
        private bool IsWorking
        {
            get
            {
                return threadSFGenerating != null && threadSFGenerating.IsAlive;
            }
        }
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (IsWorking
            && User.One.AskUserYesNoQuestion("Do you want to quit this program?") == User.YesNoQuestion.No)
                e.Cancel = true;
            base.OnClosing(e);
        }
        protected override void OnLoad(EventArgs e)
        {
            
            base.OnLoad(e);
            try
            {
                LoadGraphDataFromInPutFolder(new string[] { "*.txt", "*.xml" });
                nudMaxLink.DataBindings.Add(new Binding("Maximum", this, "nMaxLink"));
                nudMinLink.DataBindings.Add(new Binding("Maximum", this, "nMaxLink"));
                nudMaxLink.DataBindings.Add(new Binding("Minimum", this, "nMinLink"));
                nudMinLink.DataBindings.Add(new Binding("Minimum", this, "nMinLink"));
            }
            catch (Exception ex)
            {
                User.One.MessageToUser(ex.Message);
            }

        }

        //private void Test()
        //{
        //    string xmlFileName = Netutil.InPutDirector + "\\Cnode.xml";
        //    //ComplexNet net = new ComplexNet();
        //    net.readFromGraphML(xmlFileName);
        //    Netutil.DumpNet(net);
        //    double netrobust = net.NetworkRobustness(new Perturbation());
        //    Node xNode=net.Nodes.Where(p => p.name=="x").Select(e =>e).ElementAt(0);
        //    Node yNode = net.Nodes.Where(p => p.name == "y").Select(e => e).ElementAt(0);
        //    Node zNode = net.Nodes.Where(p => p.name == "z").Select(e => e).ElementAt(0);

        //    double Xrobust = net.NodeRobustness(xNode, new Perturbation());
        //    double Yrobust = net.NodeRobustness(yNode, new Perturbation());
        //    double Zrobust = net.NodeRobustness(zNode, new Perturbation());
        //    double NodeNetRobust = net.NodeRobustnessForState(xNode, new float[] { 0, 0, 1 }, new Perturbation());


        

        //    Dictionary<Node, int> Cluster = new Dictionary<Node, int>();
        //    for(int i=0;i<net.Nodes.Count();i++)
        //        Cluster[net.Nodes.ElementAt(i)]=0;

        //    double XRecovery = net.GroupNodeRobustness(xNode, Cluster.Keys, new Perturbation());
        //    double YRecovery = net.GroupNodeRobustness(yNode, Cluster.Keys, new Perturbation());
        //    double ZRecovery = net.GroupNodeRobustness(zNode, Cluster.Keys, new Perturbation());
        //    double ModuleRecovery = 0, NetRovery = 0, NoneModuleRecovery=0;


        //    //net.NetRobustness
        //    net.InOutModuleRobustness(Cluster, new Perturbation(), ref ModuleRecovery, ref NoneModuleRecovery);
        //    NetRovery = net.NetworkRecovery(new Perturbation());


        //    bool ModuleCheck=((XRecovery+YRecovery+ZRecovery)/3==ModuleRecovery);
            
        //}
       
        List<BooleanNetwork> graphs = new List<BooleanNetwork>();
        private void bntbrowse_Click(object sender, EventArgs e)
        {

            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = Directory.GetCurrentDirectory();
            openFileDialog1.Filter = "Text (*.txt)|*.txt|Excel 2007(*.xlsx)|*.xlsx|Excel 2003(*.xls)|*.xlsx |All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    //Determine the order of the data (star node is always zero index)


                    BooleanNetwork gr = BooleanNetwork.ReadSignalingNetworkFile(openFileDialog1.FileName);
                    Debug.WriteLine("Arcs are:");
                    Netutil.DumpInteraction(gr.Arcs.ToArray());
                    Debug.WriteLine("Edges are:");
                    Netutil.DumpInteraction(gr.Edges.ToArray());

                    if (gr.Arcs.Count() <= 0)
                    {
                        MessageBox.Show("Have no data in the file \"" + openFileDialog1.FileName + "\"");
                        return;
                    }
                    lbgraphs.DataSource = null;
                    graphs.Add(gr);
                    lbgraphs.DataSource = graphs;
                    lbgraphs.DisplayMember = "Name";
                    lbgraphs.ValueMember = "ObjectID";
                    
                    txtFileName.Text = openFileDialog1.FileName;
                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }

        }
        private void LoadGraphDataFromInPutFolder(string []extensions)
        {
            lbgraphs.DataSource = null;
            graphs.Clear();
            graphs=GraphData.LoadInputFiles(extensions);
            lbgraphs.DataSource = graphs;
            lbgraphs.DisplayMember = "Name";
            lbgraphs.ValueMember = "ObjectID";
        }
        private void bntCalculate_Click(object sender, EventArgs e)
        {
            if (lbgraphs.Items.Count == 0)
            {
                User.One.MessageToUser("Have no network to calculate!\nPlease load data network from file!");
                return;
            }
            if (lbgraphs.SelectedIndex == -1)
            {
                User.One.MessageToUser("You have to select a network for calculation");
                return;
            }
            
            BooleanNetwork net = graphs[lbgraphs.SelectedIndex];
            //ComplexNet net = new ComplexNet(graph);

            Debug.WriteLine(" n multi arcs edge =");
            Netutil.DumpInteraction(net.EdgeWithMultipleOppositeArcs.ToArray());
            float centrality = net.DegreeCentrality;
            Dictionary<Node, int> Cluster = null;
            double modularity = net.modularity(ref Cluster);
            var nCluster = from p in Cluster
                           group p by p.Value into g
                           select g;
            double networkRecovery = 0, moduleRecovery=0, noneModuleRecovery=0;
            net.InOutModuleRobustness(Cluster, new Perturbation(Perturbation.Kind.Mutation), ref moduleRecovery, ref noneModuleRecovery);
            networkRecovery = net.NetworkRecovery(new Perturbation());

            double sigrobust = net.NetworkRobustness(new Perturbation(Perturbation.Kind.Mutation));
            
            User.One.MessageToUser("-> Network '" + net.Name + string.Format("' Mutation robustness ={0}\n\t Centrality ={1}\n\t Modularity={2}\n\t The number of clusters={3}\n\t Module recovery={4}\n\t Non-Module recovery={5}\n\t Network recovery={6}", sigrobust, centrality, modularity, nCluster.Count(), moduleRecovery, noneModuleRecovery, networkRecovery));
            User.One.MessageToUser("\nCluster at "+ Netutil.WriteClusterToTextFile(modularity, Cluster, "Modules."+net.Name+".txt"));
        }

        private void bntrefresh_Click(object sender, EventArgs e)
        {
            LoadGraphDataFromInPutFolder(new string[] { "*.txt", "*.xls" });
           
        }

        

        private void bntSave_Click(object sender, EventArgs e)
        {
            int nNode = (int)nudNodeFrom.Value,
                   nMinLink = (int)nudMinLink.Value,
                   nMaxLink = (int)nudMaxLink.Value;
            object filename = "";
            if (User.One.AskUserAnValue(string.Format("You'll create a random network with {0} nodes and {1} links",nNode,nMinLink), "Please enter a file name (text file)", typeof(string),
                "net." + nudNodeFrom.Value.ToString() + "." + nudMinLink.Value.ToString() + "." + nudMaxLink.Value.ToString() + ".txt", ref filename)
                == User.YesNoQuestion.No)
                return;
            ComplexNetGenerator generator = new ComplexNetGenerator();
            BooleanNetwork temp = new BooleanNetwork();
             User.One.MessageToUser(string.Format("Start creating a network with {0} nodes and {1} links",nNode,nMinLink));
             BooleanNetwork net = (BooleanNetwork)generator.generateDirectedNetworkByPreferentialAttachment(temp, nNode, nMinLink);
            User.One.MessageToUser(filename.ToString() + " is created at " + Netutil.WriteGraphToTextFile(net, filename as string));

        }

       
       
       
    }
}

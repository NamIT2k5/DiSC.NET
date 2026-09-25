using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NetSimulation.Community;
using NetSimulation.Lib;
using System.Diagnostics;
using Mathutil;
using BasicNet;
using System.IO;
namespace NetSimulation
{
    
    static class Program
    {

        public static MainForm mainform = null;
        static WinUser user = null;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args != null && args.Length > 0 && args[0] == "--test")
            {
                Console.WriteLine("Running DiSC Network Simulation (1000 Networks)...");
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                BasicNet.Examination.SignalingStudy.DiSCNetworkSimulation(1000, 100, 200, 2, 6, "disc.ba.rel.txt");
                stopwatch.Stop();
                Console.WriteLine($"Simulation finished in {stopwatch.ElapsedMilliseconds} ms.");
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            mainform = new MainForm();
            user = new WinUser();//create after creating MainForm
            Application.Run(mainform);
        }
        private static void AlgorithmTest()
        {
            
            

           
            
        }
        private static void GenerateCombination()
        {
            List<Pair<Set<string>, Set<string>>> Laws = new List<Pair<Set<string>, Set<string>>>();
            Laws.Add(new Pair<Set<string>,Set<string>>(new Set<string>("A"),new Set<string>("B","C")));
            Laws.Add(new Pair<Set<string>, Set<string>>(new Set<string>("B"), new Set<string>("A")));
            Laws.Add(new Pair<Set<string>, Set<string>>(new Set<string>("C"), new Set<string>("D")));
            Laws.Add(new Pair<Set<string>, Set<string>>(new Set<string>("E"), new Set<string>("F")));
            Laws.Add(new Pair<Set<string>, Set<string>>(new Set<string>("E"), new Set<string>("K")));
            Laws.Add(new Pair<Set<string>, Set<string>>(new Set<string>("K"), new Set<string>("E")));
            Debug.WriteLine("Closure...");
            Set<string> att = new Set<string>("K","C");
            att.ClosureWith(Laws);
            Netutil.DumpList<string>(att);

            Debug.WriteLine("Keys...");
            IEnumerable<Set<string>> Keys= Set<string>.FindSmallestKeySet(Laws);
            foreach (Set<string> key in Keys)
            {
                Netutil.DumpList<string>(key);
            }
            //ComplexNetGenerator gen = new ComplexNetGenerator();
            //ComplexNet net = gen.generateComplexScaleFreeDzung(10, 20);
            //Debug.WriteLine(Netutil.DumpNet(net));
            //bool val = net.IsValid();
            
            //IEnumerable<Node> pNode = net.Nodes.Where(p => p.name == "1");
            //Node node = pNode.ElementAt(0);
            //node.isLock = true;
            

            //Debug.WriteLine("----");
            //Debug.WriteLine(Netutil.DumpNet(net));
            //node.isLock = false;

            
            //Perturbation SignalePerturb = new Perturbation();
            //bool sigRobust = net.IsRobustNode(node, net.InitStandardUnlockedState(), SignalePerturb);
            //double sigNotRobust = net.RobustnessOfNode(node, SignalePerturb);

            
            //Perturbation RemovedPerturb = new Perturbation(Perturbation.Kind.RemovedPerturbation);
            //bool remRobust = net.IsRobustNode(node, net.InitStandardUnlockedState(), RemovedPerturb);
            //double remNotRobust = net.RobustnessOfNode(node, RemovedPerturb);

        }
    }
}

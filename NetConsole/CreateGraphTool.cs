using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using BasicNet;
namespace NetConsole
{
    public class CreateGraphTool
    {
        #region Reduced module undirected graph with the dash showing the same module and the solid show the same module and the same disease
        /// <summary>
        /// Create a REDUCED undirected  graph in the text file that represents a map of modules disease genes
        /// </summary>
        /// <param name="filename">The file source which is a text file with three column (Disease name, ModuleID, DiseaseID)</param>
        /// <returns>A grap text file with format (start, end, weight, subtype (module ID), type (ID of In disease or Out disease)</returns>
        public static BasicNetwork CreateModuleReducedGraphFromFile(string filename)
        {
            HashSet<DiseaseRow> dataSrc = readModuleFile(filename);
            BasicNet.BasicNetwork Net= new BasicNetwork();
            String gene = null;
            String module = null;
            String disease = null;
            Interaction interaction = null;
            foreach (DiseaseRow row in dataSrc)
            {
                gene = row.Gene;
                module = row.Module;
                disease = row.Disease;
                Node start = null, end = null;
                start = Net.AddNode(gene);

                //1. Find gene in the same module AND disease to make a link
                var sameDiMo = from p in dataSrc
                               where p.Gene != gene && //diff gene
                               p.Disease == disease && // the same disease
                                   p.Module == module // the same module
                               select p;

                if (sameDiMo != null && sameDiMo.Count() > 0)
                {
                    foreach (DiseaseRow r in sameDiMo)
                    {
                        //NEGATIVE link connecting between gene in the same disease and in the same module
                        int InteractionType = module.GetHashCode();
                        if (!Net.hasUndirectedConnection(start, r.Gene, InteractionType))
                        {
                            end = Net.AddNode(r.Gene);
                            interaction = new Interaction(start, end, InteractionType, "InDisease.Solid");
                            Net.AddArc(interaction);

                        }
                    }
                }
                
                //2. Find gene in the same module to make a link
                var sameMobutDi = from p in dataSrc
                                    where p.Gene != gene && //diff gene
                                    p.Disease != disease && // diff disease
                                        p.Module == module // the same module
                                    select p;
                if (sameMobutDi != null && sameMobutDi.Count() > 0)
                {
                    foreach (DiseaseRow r in sameMobutDi)
                    {
                        //POSITIVE link connecting between gene in the same disease but in diffrent module
                        int InteractionType = module.GetHashCode();
                        if (!Net.hasUndirectedConnection(start, r.Gene, InteractionType))
                        {
                            end = Net.AddNode(r.Gene);
                            interaction = new Interaction(start, end, InteractionType, "OutDisease.Dash");
                            Net.AddArc(interaction);

                        }
                    }
                }
                
            }
            Netutil.WriteGraphToTextFile(Net, filename + ".graph.txt");
            return Net;

        }
        #endregion

        #region Reduced disease undirected graph with the dash showing the same disease and the solid show the same diseae and the same module (duplicates is removal)
        /// <summary>
        /// Create a REDUCE undirected graph in the text file that represents a map of disease genes
        /// </summary>
        /// <param name="filename">The file source which is a text file with three column (Disease name, ModuleID, DiseaseID)</param>
        /// <returns>A grap text file with format (start, end, weight, subtype (disease ID), type (ID of In module or Out module)</returns>
        public static BasicNetwork CreateDiseaseReducedGraphFromFile(string filename)
        {
            HashSet<DiseaseRow> dataSrc = readModuleFile(filename);
            BasicNet.BasicNetwork Net = new BasicNetwork();
            String gene = null;
            String module = null;
            String disease = null;
            Interaction interaction = null;
            foreach (DiseaseRow row in dataSrc)
            {
                gene = row.Gene;
                module = row.Module;
                disease = row.Disease;
                Node start = null, end = null;
                start = Net.AddNode(gene);

                //1. Find gene in the same module AND disease to make a link
                var sameDiMo = from p in dataSrc
                               where p.Gene != gene && //diff gene
                               p.Disease == disease && // the same disease
                                   p.Module == module // the same module
                               select p;

                if (sameDiMo != null && sameDiMo.Count() > 0)
                {
                    foreach (DiseaseRow r in sameDiMo)
                    {
                        //NEGATIVE link connecting between gene in the same disease and in the same module
                        int InteractionType = disease.GetHashCode();
                        if (!Net.hasUndirectedConnection(start, r.Gene, InteractionType))
                        {
                            end = Net.AddNode(r.Gene);
                            interaction = new Interaction(start, end, InteractionType, "InModule.Solid");
                            Net.AddArc(interaction);

                        }
                    }
                }

                //3. Find gene in the same disease to make a link
                var sameDibutMo = from p in dataSrc
                                  where p.Gene != gene && //diff gene
                                  p.Disease == disease && //the same disease
                                      p.Module != module // dif module
                                  select p;
                if (sameDibutMo != null && sameDibutMo.Count() > 0)
                {
                    foreach (DiseaseRow r in sameDibutMo)
                    {
                        //POSITIVE link connecting between gene in the same disease but in diffrent module
                        int InteractionType = disease.GetHashCode();
                        if (!Net.hasUndirectedConnection(start, r.Gene, InteractionType))
                        {
                            end = Net.AddNode(r.Gene);
                            interaction = new Interaction(start, end, InteractionType, "OutModule.Dash");
                            Net.AddArc(interaction);

                        }
                    }
                }


            }
            Netutil.WriteGraphToTextFile(Net, filename + ".graph.txt");
            return Net;

        }
        #endregion

        #region Full disease undirected graph with the dash showing the same disease and the solid show the same diseae and the same module (NO REMOVAL of DUPLICATE)
        /// <summary>
        /// Create a FULL graph in the text file that represents a map of disease genes
        /// </summary>
        /// <param name="filename">The file source which is a text file with three column (Gene, ModuleID, DiseaseID)</param>
        /// <returns>A grap text file with format (start, end, weight, subtype (disease ID), type (ID of In module or Out module)</returns>
        public static BasicNetwork CreateDiseaseFullGraphFromFile(string filename)
        {
            HashSet<DiseaseRow> dataSrc = readModuleFile(filename);
            BasicNet.BasicNetwork Net = new BasicNetwork();
            String gene = null;
            String module = null;
            String disease = null;
            Interaction interaction = null;
            IEnumerable<Node> ptemp=null;
            foreach (DiseaseRow row in dataSrc)
            {
                gene = row.Gene;
                module = row.Module;
                disease = row.Disease;
                Node start = null, end = null;
                start = Net.AddNode(gene);

                //1. Find gene in the same module AND disease to make a link
                var sameDiMo = from p in dataSrc
                               where p.Gene != gene && //diff gene
                               p.Disease == disease && // the same disease
                                   p.Module == module // the same module
                               select p;

                if (sameDiMo != null && sameDiMo.Count() > 0)
                {
                    foreach (DiseaseRow r in sameDiMo)
                    {
                        //NEGATIVE link connecting between gene in the same disease and in the same module
                        int InteractionType = disease.GetHashCode();
                        ptemp = start.GetNeighbour(InteractionType);
                        if (ptemp.Where(p => p.name==r.Gene).Count()==0)
                        {
                            end = Net.AddNode(r.Gene);
                            interaction = new Interaction(start, end, InteractionType, "InModule.Solid");
                            Net.AddArc(interaction);

                        }
                    }
                }

                //3. Find gene in the same disease to make a link
                var sameDibutMo = from p in dataSrc
                                  where p.Gene != gene && //diff gene
                                  p.Disease == disease && //the same disease
                                      p.Module != module // dif module
                                  select p;
                if (sameDibutMo != null && sameDibutMo.Count() > 0)
                {
                    foreach (DiseaseRow r in sameDibutMo)
                    {
                        //POSITIVE link connecting between gene in the same disease but in diffrent module
                        int InteractionType = disease.GetHashCode();
                        ptemp = start.GetNeighbour(InteractionType);
                        if (ptemp.Where(p => p.name == r.Gene).Count() == 0)
                        {
                            end = Net.AddNode(r.Gene);
                            interaction = new Interaction(start, end, InteractionType, "OutModule.Dash");
                            Net.AddArc(interaction);

                        }
                    }
                }


            }
            Netutil.WriteGraphToTextFile(Net, filename + ".graph.txt");
            return Net;

        }
        #endregion

        #region Reduced disease undirected graph with the dash showing the same disease and the solid show the same diseae and the same module (duplicates is removal)
       
        private static void AddEdgeToUndirectedCircle(BasicNetwork Net, Node startNode, Node newNode, int edgeType, string edgeName)
        {
            IEnumerable<Node> circleNode = Net.BreadthFirstTraversal(startNode, edgeType, false);
            if (circleNode.Count() <= 1)
            {
                Interaction interaction = new Interaction(startNode, newNode, edgeType, edgeName);
                Net.AddArc(interaction);
            }
            else if (circleNode.Count() == 2) // start creating a new undirected single circle
            {
                 Node endCircle = null, beginCircle = null;

                 if (Net.hasDirectedConnection(startNode, circleNode.ElementAt(circleNode.Count() - 1).name, edgeType))
                 {
                     beginCircle = startNode;
                     endCircle = circleNode.ElementAt(circleNode.Count() - 1);
                 }
                 else
                 {
                     beginCircle = circleNode.ElementAt(circleNode.Count() - 1);
                     endCircle = startNode;
                 }
               

                Interaction interaction = new Interaction(endCircle, newNode, edgeType, edgeName);
                Net.AddArc(interaction);

                interaction = new Interaction(newNode, beginCircle, edgeType, edgeName);
                Net.AddArc(interaction);
            }
            else // exist a undirected single circle already
            {
                Node endCircle = null, beginCircle = null;

                if (Net.hasDirectedConnection(startNode, circleNode.ElementAt(circleNode.Count() - 1).name, edgeType))
                {
                    beginCircle = startNode;
                    endCircle = circleNode.ElementAt(circleNode.Count() - 1);
                }
                else
                {
                    beginCircle = circleNode.ElementAt(circleNode.Count() - 1);
                    endCircle = startNode;
                }

                IEnumerable<Interaction> linkBetween2 = Net.GetArcsBetween2Node(beginCircle, endCircle, edgeType);
                Net.RemoveArc(edgeType, linkBetween2.ToArray());

                Interaction interaction = new Interaction(endCircle, newNode, edgeType, edgeName);
                Net.AddArc(interaction);

                interaction = new Interaction(newNode, beginCircle, edgeType, edgeName);
                Net.AddArc(interaction);
            }
        }
        /// <summary>
        /// Create a REDUCE undirected graph in the text file that represents a map of disease genes
        /// </summary>
        /// <param name="filename">The file source which is a text file with three column (Disease name, ModuleID, DiseaseID)</param>
        /// <returns>A grap text file with format (start, end, weight, subtype (disease ID), type (ID of In module or Out module)</returns>
        public static BasicNetwork CreateDiseaseCircileReducedGraphFromFile(string filename)
        {
            HashSet<DiseaseRow> dataSrc = readModuleFile(filename);
            BasicNet.BasicNetwork Net = new BasicNetwork();
            String gene = null;
            String module = null;
            String disease = null;

            foreach (DiseaseRow row in dataSrc)
            {
                gene = row.Gene;
                module = row.Module;
                disease = row.Disease;
                Node start = null, end = null;
                start = Net.AddNode(gene);

                //1. Find gene in the same module AND disease to make a link
                var sameDiMo = from p in dataSrc
                               where p.Gene != gene && //diff gene
                               p.Disease == disease && // the same disease
                                   p.Module == module // the same module
                               select p;

                if (sameDiMo != null && sameDiMo.Count() > 0)
                {
                    foreach (DiseaseRow r in sameDiMo)
                    {
                        //NEGATIVE link connecting between gene in the same disease and in the same module
                        int InteractionType = disease.GetHashCode();
                        if (!Net.hasUndirectedConnection(start, r.Gene, InteractionType))
                        {
                            end = Net.AddNode(r.Gene);
                            AddEdgeToUndirectedCircle(Net, start, end, InteractionType, "InModule.Solid");
                        }
                    }
                }

                //3. Find gene in the same disease to make a link
                var sameDibutMo = from p in dataSrc
                                  where p.Gene != gene && //diff gene
                                  p.Disease == disease && //the same disease
                                      p.Module != module // dif module
                                  select p;
                if (sameDibutMo != null && sameDibutMo.Count() > 0)
                {
                    foreach (DiseaseRow r in sameDibutMo)
                    {
                        //POSITIVE link connecting between gene in the same disease but in diffrent module
                        int InteractionType = disease.GetHashCode();
                        if (!Net.hasUndirectedConnection(start, r.Gene, InteractionType))
                        {
                            end = Net.AddNode(r.Gene);
                            AddEdgeToUndirectedCircle(Net, start, end, InteractionType, "OutModule.Dash");

                        }
                    }
                }


            }
            Netutil.WriteGraphToTextFile(Net, filename + ".graph.txt");
            return Net;

        }
        #endregion
        
        public struct DiseaseRow
        {
            public string Gene;
            public string Module;
            public string Disease;
            public DiseaseRow(string gene, string module, string disease)
            {
                this.Gene = gene;
                this.Module = module;
                this.Disease = disease;
            }
        }
        /// <summary>
        /// Read a module text file with 3 columns in order: (Gene, ModuleID, DiseaseID)
        /// </summary>
        /// <param name="filename">The file name</param>
        /// <returns>Rows</returns>
        public static HashSet<DiseaseRow> readModuleFile(string filename)
        {
            filename = Netutil.InPutDirector + "\\" + filename;
            HashSet<DiseaseRow> dataSrc = new HashSet<DiseaseRow>();
            int geneIdx = 0, moduleIdx = 1, diseaseIdx = 2;
            StreamReader file = new StreamReader(filename);
            string line;
            string[] token = null;
            String gene = null;
            String module = null;
            String disease = null;
            try
            {
                
                while ((line = file.ReadLine()) != null)
                {
                    token = line.Split(new char[] { '\t' });

                    if (token == null) continue;
                    gene = token[geneIdx].Trim();
                    module = token[moduleIdx].Trim();
                    disease = token[diseaseIdx].Trim();
                    dataSrc.Add(new DiseaseRow(gene, module, disease));
                  
                }
                file.Close();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception while reading the graph for invalid data format:");
                Debug.WriteLine(e.Message);

            }
            return dataSrc;
        }
    }
}

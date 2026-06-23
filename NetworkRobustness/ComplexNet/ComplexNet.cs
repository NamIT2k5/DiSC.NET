using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BasicNet;
using NetSimulation.Community;
using System.Xml;
using System.Xml.XPath;
using System.Diagnostics;

namespace BasicNet
{
    public class ComplexNet:BooleanNetwork
    {
       
        //public void Import(GraphData source)
        //{
        //    Dictionary<string, Dictionary<string, double>> g = source;
        //    foreach (string start in g.Keys)
        //    {
                
        //        foreach (KeyValuePair<string, double> end in g[start])
        //        {
        //            Node startnode = new Node(start), endnode = new Node(end.Key);
        //            AddArc(new Interaction(startnode, endnode, end.Value));
        //            Nodes.Add(startnode);
        //            Nodes.Add(endnode);
        //        }
        //    }
        //    this.Name = source.Name;
        //}
       
        public void Import(GraphData source)
        {
            Dictionary<string, Dictionary<string, float>> g = source;
            foreach (string start in g.Keys)
            {

                foreach (KeyValuePair<string, float> end in g[start])
                {
                    Node nodesrc = null;
                    Node nodetar = null;

                    if (Nodes.Count() > 0)
                    {
                        var set = from nodeid in Nodes
                                  where nodeid.name == start
                                  select nodeid;

                        if (set != null && set.Count() > 0)
                            nodesrc = set.ElementAt(0);
                        else
                        {
                            nodesrc = new Node(start,Node.DefaultFunctionType);
                            this.AddNode(nodesrc);
                        }

                        set = from nodeid in Nodes
                              where nodeid.name == end.Key
                              select nodeid;

                        if (set != null && set.Count() > 0)
                            nodetar = set.ElementAt(0);
                        else
                        {
                            nodetar = new Node(end.Key, Node.DefaultFunctionType);
                            AddNode(nodetar);
                        }

                    }
                    else
                    {
                        nodesrc = new Node(start, Node.DefaultFunctionType);
                        nodetar = new Node(end.Key, Node.DefaultFunctionType);
                        AddNode(nodesrc);
                        AddNode(nodetar);

                    }

                    Interaction intract = new Interaction(nodesrc, nodetar,Interaction.DefaultValue, end.Value);
                    AddArc(intract);
                }
            }
            this.Name = source.Name;
        }
        public ComplexNet(GraphData source):this()
        {
            Import(source);
        }
       
        
        public ComplexNet()
        {
            
        }

        public void saveAsGraphML(StreamWriter writer)
        {
            writer.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            writer.WriteLine("<graphml xmlns=\"http://graphml.graphdrawing.org/xmlns\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://graphml.graphdrawing.org/xmlns http://graphml.graphdrawing.org/xmlns/1.0/graphml.xsd\">");
            writer.WriteLine("\t<graph name=\""+this.Name+"\" edgetype=\"undirected\">");
            
            foreach (Node node in Nodes)
            {
                writer.WriteLine("\t\t<node name=\"" + node.name + "\" state=\""+node.State+"\" function=\""+node.Type+"\" />");
            }

            foreach (Interaction edge in Arcs)
            {
                writer.WriteLine("\t\t<edge type=\"" + edge.Type + "\" weight=\""+edge.weight+"\" start=\"" + edge.startNode.name + "\" end=\"" + edge.endNode.name + "\" />");
            }

            writer.WriteLine("\t</graph>");
            writer.WriteLine("</graphml>");
        }
        public void readFromGraphML(string xmlfile)
        {
            XmlTextReader reader = new XmlTextReader(xmlfile);
            using (reader)
            {
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element: // The node is an element.
                            Debug.Write("<" + reader.Name);
                            Debug.WriteLine(">");
                            readXmlElement(reader);
                            break;
                        case XmlNodeType.Text: //Display the text in each element.
                            Debug.WriteLine(reader.Value);
                            break;
                        case XmlNodeType.EndElement: //Display the end of the element.
                            Debug.Write("</" + reader.Name);
                            Debug.WriteLine(">");
                            break;
                    }
                }
            }


        }
        void readXmlElement(XmlTextReader reader)
        {
            if(reader.Name=="node")
            {
                string name=null;
                float state=0.0f;
                FunctionType type= FunctionType.AND;
                while (reader.MoveToNextAttribute()) // Read the attributes.
                {
                    switch (reader.Name)
                    { 
                        case "name":
                            name=reader.Value;
                            break;
                        case "state":
                            state=Convert.ToSingle(reader.Value);
                            break;
                        case "function":
                            type = reader.Value.ToUpper().Equals("AND") ? FunctionType.AND : FunctionType.OR;
                            break;
                    }
                }
                Node n = new Node(name, type);
                n.ResetState(state);
                this.AddNode(n);
            }
            if (reader.Name == "edge")
            {
                string start=null, end = null;
                float weight = 0.0f;
                InteractionType type = InteractionType.NULL;

                while (reader.MoveToNextAttribute()) // Read the attributes.
                {
                    switch (reader.Name)
                    {
                        case "type":
                            type = reader.Value.ToUpper().Equals("NEGATIVE") ? InteractionType.NEGATIVE : (reader.Value.ToUpper().Equals("POSITIVE") ? InteractionType.POSITIVE : InteractionType.NULL);
                            break;
                        case "weight":
                            weight = Convert.ToSingle(reader.Value);
                            break;
                        case "start":
                            start = reader.Value;
                            break;
                        case "end":
                            end = reader.Value;
                            break;
                    }
                }
                Node nstart = null;
                Node nend = null;

                IEnumerable<Node> pNode = (from p in Nodes where p.name == start select p);
                if (pNode.Count() == 0)
                {
                    nstart = new Node(start, Node.ArbitraryFunctionType);
                    this.AddNode(nstart);
                }else
                    nstart = pNode.ElementAt(0);

                pNode = (from p in Nodes where p.name == end select p);
                if (pNode.Count() == 0)
                {
                    nend = new Node(end, Node.ArbitraryFunctionType);
                    this.AddNode(nend);
                }else
                    nend = pNode.ElementAt(0);

                this.AddArc(new Interaction(nstart, nend, type, weight, weight==0?Interaction.DirectionType.undirected:Interaction.DirectionType.directed));

            }

        }
    }
}

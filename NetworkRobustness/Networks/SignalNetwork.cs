using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using NetSimulation;

namespace ComplexNetGeneratorLib
{
    public class SignalNetwork: BasicNetwork
    {
        Random ran = new Random((int)DateTime.Now.Ticks);

        public void LoadFromFile(string filename)
        {
            try
            {
                StreamReader file = new StreamReader(filename);
                using (file)
                {
                    string line;
                    string[] token = null;
                    
                    while ((line = file.ReadLine()) != null)
                    {
                        //token = line.Split(new char[] { ' ', ';', '\t' });
                        token = line.Split(new char[] {';', '\t' });
                        if (token == null) continue;

                        string sourceName = token[0];
                        string targetName = token[1];
                        InteractionType interactionType = token.Length>2? (token[2].Trim()=="1" ? InteractionType.POSITIVE:InteractionType.NEGATIVE):InteractionType.NULL;
                        Node source =null;
                        Node target =null;

                        if (Nodes.Count > 0)
                        {
                            var set = from nodeid in Nodes
                                      where nodeid.name == sourceName
                                      select nodeid;

                            if (set != null && set.Count() > 0)
                                source = set.ElementAt(0);
                            else
                            {
                                source = new Node(sourceName);
                                Nodes.Add(source);
                            }

                            set = from nodeid in Nodes
                                  where nodeid.name == targetName
                                  select nodeid;

                            if (set != null && set.Count() > 0)
                                target = set.ElementAt(0);
                            else
                            {
                                target = new Node(targetName);
                                Nodes.Add(target);
                            }

                        }
                        else
                        {
                            source = new Node(sourceName);
                            target = new Node(targetName);
                            Nodes.Add(source);
                            Nodes.Add(target);

                        }

                        Interaction intract = new Interaction(source, target, interactionType);
                        Interactions.Add(intract);
                        

                    }
                    file.Close();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception while reading the graph:");
                Debug.WriteLine(e.Message);
            }
        }
    }
}

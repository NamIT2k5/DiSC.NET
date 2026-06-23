using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BasicNet;
using NetSimulation.Lib;
using System.Data;

namespace NetworkRobustness.Geonetwork
{
    public class GeoNet: BooleanNetwork
    {
        protected float mR = 0;//The width and height of the cell
        protected Grid mGrid = null;// The grid whose each cell has size of RxR
        protected DataTable mGpsDT = null;// the GPS coordinator whose structure StartTime, StartX, StartY, EndTime, EndX, EndY
        protected DataTable mGridDT = null;// the Gid coordinator whose structure StartTime, Start, EndTime, End, Weight
        public GeoNet(float R, string sourceTxtFile)
        {
            this.mR = R;
            Readnetwork(sourceTxtFile);
        }
        /// <summary>
        /// Read network from text file 
        /// </summary>
        /// <param name="sourceTxtFile">The name of file with 6 columns separated by tab 
        /// whose structure is StartTime, StartX, StartY, EndTime, EndX, EndY and without header</param>
        protected void Readnetwork(string sourceTxtFile)
        {
            mGpsDT = TextDB.ConvertToDataTable(sourceTxtFile, new DataColumn[] {
                    new DataColumn("StartTime", typeof(DateTime)), 
                    new DataColumn("StartX",typeof(float)), 
                    new DataColumn("StartY",typeof(float)),
                    new DataColumn("EndTime",typeof(DateTime)),
                    new DataColumn("EndX",typeof(float)),
                    new DataColumn("EndY",typeof(float)) });

            //float Xo = 20.85128f;
            float Xo = Math.Min((from row in mGpsDT.AsEnumerable()
                                 select row.Field<float>("StartX")).Min(),
                                         (from row in mGpsDT.AsEnumerable()
                                          select row.Field<float>("EndX")).Min());
            //float Yo = 105.76232f;
            float Yo = Math.Min((from row in mGpsDT.AsEnumerable()
                                 select row.Field<float>("StartY")).Min(),
                                         (from row in mGpsDT.AsEnumerable()
                                          select row.Field<float>("EndY")).Min());
            //float Xm = 21.21377f;
            float Xm = Math.Max((from row in mGpsDT.AsEnumerable()
                                 select row.Field<float>("StartX")).Max(),
                                         (from row in mGpsDT.AsEnumerable()
                                          select row.Field<float>("EndX")).Max());

            //float Ym = 106.03915f;
            float Ym = Math.Max((from row in mGpsDT.AsEnumerable()
                                 select row.Field<float>("StartY")).Max(),
                                         (from row in mGpsDT.AsEnumerable()
                                          select row.Field<float>("EndY")).Max());
            mGrid = new Grid(Xo, Yo, Xm, Ym, mR);

            mGridDT = ConvertGPStoGrid(mGpsDT);
            var sumGridTab = from e in mGridDT.AsEnumerable()
                             group e by new { Start = e.Field<int>("Start"), End = e.Field<int>("End") } into g
                             select new
                             {
                                 Start = g.Key.Start,
                                 End = g.Key.End,
                                 Weight = g.Sum(r => r.Field<int>("Weight"))
                             };
            foreach (var e in sumGridTab)
            {
                Node nodeSta = AddNode(e.Start.ToString());
                Node nodeEnd = AddNode(e.End.ToString());
                Interaction edg = new Interaction(nodeSta, nodeEnd, 1,"", e.Weight);
                AddNodeAndArc(edg);
            }
        }
        /// <summary>
        /// Convert GPS datatable to grid datatable whose columns are StartTime, Start, EndTime, End, Weight
        /// </summary>
        /// <param name="gpsTable">GPS datatable whose structure is StartTime, StartX, StartY, EndTime, EndX, EndY</param>
        /// <returns>The datatable whose structure is StartTime, Start, EndTime, End, Weight</returns>
        protected DataTable ConvertGPStoGrid(DataTable gpsTable)
        {
            DataColumn[] gridCols = new DataColumn[] { new DataColumn("StartTime",typeof(DateTime)), new DataColumn("Start",typeof(int)), 
                new DataColumn("EndTime",typeof(DateTime)), new DataColumn("End",typeof(int)), new DataColumn("Weight", typeof(int)) };

            DataTable gridTable = TextDB.CreateDataTable(gridCols);
            foreach (DataRow r in gpsTable.Rows)
            {
                DataRow dr = gridTable.NewRow();

                dr[0] = r[0];
                dr[1] = mGrid.ConvertGPStoGrid(Convert.ToSingle(r[1]), Convert.ToSingle(r[2]));
                dr[2] = r[3];
                dr[3] = mGrid.ConvertGPStoGrid(Convert.ToSingle(r[4]), Convert.ToSingle(r[5]));
                dr[4] = 1;


                gridTable.Rows.Add(dr);
            }
            return gridTable;

        }
        protected void ConvertGridtoGPS(string NodeIDasCellID, ref float X, ref float Y, ref float Size)
        {

            mGrid.ConvertGridtoGPS(Convert.ToInt32(NodeIDasCellID), ref X, ref Y, ref Size);
        }
        public void NetStudy(string outFile)
        {

            float X = 0, Y = 0, size = 0;
            Dictionary<Node, float> ranking = null;
            object choice=5;
            //if (User.One.AskUserAnValue("Select ranking type", "0 => Taxipassenger; 1 => Closeness; 2 => HC; 3 => PageRank; 4 => Betweeness; 5=> Modules", typeof(int), 0, ref choice) == User.YesNoQuestion.No)
            //    return;
            switch(Convert.ToInt32(choice))
            {
                case 0:
                    ranking = this.TaxiPassengerRank();
                    break;
                case 1:
                    Dictionary<string, double> closeness = this.ClosenessCentrality();
                    ranking = new Dictionary<Node, float>();
                    foreach (var e in closeness)
                        ranking.Add(this[e.Key], Convert.ToSingle(e.Value));

                    break;
                case 2:
                    Dictionary<string, double> hc = this.HierarchicalClosenessCentrality();
                    ranking = new Dictionary<Node, float>();
                    foreach (var e in hc)
                        ranking.Add(this[e.Key], Convert.ToSingle(e.Value));
                    break;
                case 3:
                    ranking = this.PageRankCentralityInLink();
                    break;
                case 4:
                    ranking = this.BetweenessCentrality();
                    break;
                case 5:
                    Dictionary<Node, int> pCluster=null;
                    double mo = this.modularity(ref pCluster);
                    var pCls = from p in pCluster orderby p.Value descending select p;
            
                    
                    for (int i = 0; i < pCls.Count(); i++)
                    {
                        // The place where passengers go in
                        KeyValuePair<Node, int> e = pCls.ElementAt(i);
                        ConvertGridtoGPS(e.Key.name, ref X, ref Y, ref size);
                        TextDB.WriteTextFile(new string[] {
                            X.ToString(),
                            Y.ToString(),
                            (X+size).ToString(),
                            (Y+size).ToString(),
                            e.Value.ToString(),
                            e.Value.ToString(),
                             this[e.Key.name].InWeight.ToString(),
                             this[e.Key.name].OutWeight.ToString()
                                }, outFile);

                        
                    }
                    return;
                    
               
            }
            
            

            var order = from p in ranking orderby p.Value descending select p;
            int nPoint = Convert.ToInt32(Math.Ceiling(order.Count()*0.05));
            
            for (int i = 0; i < nPoint; i++)
            {
                // The place where passengers go in
                KeyValuePair<Node, float> e = order.ElementAt(i);
                ConvertGridtoGPS(e.Key.name, ref X, ref Y, ref size);
                TextDB.WriteTextFile(new string[] {
                    X.ToString(),
                    Y.ToString(),
                    (X+size).ToString(),
                    (Y+size).ToString(),
                    ranking[e.Key].ToString(),
                    "in",
                    this[e.Key.name].InWeight.ToString(),
                    this[e.Key.name].OutWeight.ToString()
                        }, outFile);

                // The place where passengers go out
                e = order.ElementAt(order.Count() - 1 - i);

                ConvertGridtoGPS(e.Key.name, ref X, ref Y, ref size);
                TextDB.WriteTextFile(new string[] {
                    X.ToString(),
                    Y.ToString(),
                    (X+size).ToString(),
                    (Y+size).ToString(),
                    (-ranking[e.Key]).ToString(),
                    "out",
                    this[e.Key.name].InWeight.ToString(),
                    this[e.Key.name].OutWeight.ToString()
                        }, outFile);
            }

        }
    }
}

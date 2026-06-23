using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mathutil;
using MathNet.Numerics.LinearAlgebra;
using NetSimulation.Lib;

namespace BasicNet
{
    /// <summary>
    /// https://github.com/gephi/gephi/blob/master/modules/StatisticsPlugin/src/main/java/org/gephi/statistics/plugin/Modularity.java
    /// </summary>
    public class Clustering
    {
        /// <summary>
        /// Convert cluster to normal form
        /// </summary>
        /// <param name="pClustering">The cluster to convert</param>
        /// <returns>The dictionary of ModuleID, and its nodes inside </returns>
        public static Dictionary<int, List<Node>> ConvertCluster(Dictionary<Node, int> pClustering)
        {
            Dictionary<int, List<Node>> clusterSummarized = new Dictionary<int, List<Node>>();

            foreach (var cls in pClustering)
            {
                if (!clusterSummarized.ContainsKey(cls.Value))
                    clusterSummarized[cls.Value] = new List<Node>();

                clusterSummarized[cls.Value].Add(cls.Key);

            }
            return clusterSummarized;
        }
        /// <summary>
        /// Select nodes existing on the Network so that their name in the cluster
        /// </summary>
        /// <param name="Net">The network to select from</param>
        /// <param name="unmanagedCluster">The cluster with the list of node name</param>
        /// <returns></returns>
        public static Dictionary<Node, int> SelectNodeFromCluster(BasicNetwork Net, Dictionary<Node, int> unmanagedCluster)
        {
            Dictionary<Node, int> kq = new Dictionary<Node, int>();
            foreach (var unode in unmanagedCluster)
            {
                kq.Add(Net[unode.Key.name], unode.Value);
            }
            return kq;
        }
        /// <summary>
        /// Return normalized entropy of a cluster
        /// </summary>
        /// <param name="pClustering">The cluster to calculate entropy</param>
        /// <returns>Entropy value</returns>
        public static double EntropyOfCluster(Dictionary<Node, int> pClustering)
        {
            int n = pClustering.Keys.Count;
            double entropy = - (from p in pClustering group p by p.Value into g select new { groupID = g.Key, p = (double)g.Count() / n }).Sum(t => t.p * Math.Log(t.p, 2));
            
            //Normalizing entropy
            return entropy / Math.Log(n, 2);
        }
        /// <summary>
        /// Calculation the difference between two clusterings on the SAME NETWORK. Used method "Variation of information"
        /// </summary>
        /// <param name="pClustering1">The clusters of the first clustering </param>
        /// <param name="pClustering2">The clusters of the first clustering</param>
        /// <returns>The difference between two clusterings: Zero => similar; Closer to Zero -> More similar</returns>
        public static double DifferenceClusteringByVariationOfInformation(Dictionary<Node, int> pClustering1, Dictionary<Node, int> pClustering2)
        {
            //Netutil.DumpCluster(pClustering1);
            //Netutil.DumpCluster(pClustering2);
            int n = pClustering1.Keys.Count;
            Dictionary<int, List<Node>> moduleClustering1 = ConvertCluster(pClustering1);
            Dictionary<int, List<Node>> moduleClustering2 = ConvertCluster(pClustering2);
            //HC = 0, no uncertainty (1 cluster) HC =1: two clusters
            double HC1 = -moduleClustering1.Sum(t => (double)t.Value.Count / n * Math.Log((double)t.Value.Count / n, 2));
            double HC2 = -moduleClustering2.Sum(t => (double)t.Value.Count / n * Math.Log((double)t.Value.Count / n, 2));
            
            double ICC = 0;
            double Pij = 0, Pi = 0, Pj = 0;

            foreach (int k1 in moduleClustering1.Keys)
                foreach (int l2 in moduleClustering2.Keys)
                {
                    var nodeSharing = from n1 in moduleClustering1[k1] join n2 in moduleClustering2[l2] on n1.id equals n2.id select n1;
                    Pij = (double)nodeSharing.Count() / n;
                    if (Pij == 0) continue;
                    Pi = (double)moduleClustering1[k1].Count / n;
                    Pj = (double)moduleClustering2[l2].Count / n;
                    ICC += Pij * Math.Log(Pij / (Pi * Pj), 2);
                }
            return (HC1 + HC2 - 2 * ICC)/Math.Log(n,2);
        }
        public static int ClusterCount(Dictionary<Node, int> pClustering)
        {
            return (from e in pClustering group e by e.Value into g select g).Count();
        }
        public static List<Pair<float,double>> RobustnessOfClusteringMethod(BasicNetwork Net, bool forArc, int nPerturbedSample=100)
        {
            List<Pair<float, double>> Result = new List<Pair<float, double>>();
            BasicNetwork perturbedNet = null;
            Dictionary<Node, int> pCluster1 = null, pCluster2 = null;
            
            Net.modularity(ref pCluster1, forArc);//Clustering the original network by the method

            int N = forArc?Net.Arcs.Count():Net.Edges.Count();
            
            
            for (float i = 0; i <= 1; i += 0.1f)
            {
                double difference = 0;
                for (int j = 0; j < nPerturbedSample; j++)
                {
                    perturbedNet = Net.ShufflePreservingDegree((int)(i * N), forArc);
                    perturbedNet.modularity(ref pCluster2, forArc);//Clustering the perturbed network by the method
                    difference += DifferenceClusteringByVariationOfInformation(pCluster1, pCluster2);
                }
                difference /= nPerturbedSample;
                Result.Add(new Pair<float, double>(i, difference));
            }
            
            return Result;
        }
        #region Benchmark genaration
        // it computes the integral of a power law
        double integral(double a, double b) 
        {
	
	        if (Math.Abs(a+1.0)>1e-10)
		        return (1.0/(a+1.0)*Math.Pow(b, a+1.0));
	        else
		        return (Math.Log(b));

        }

        // it returns the average degree of a power law
        public double average_degree(double dmax, double dmin, double gamma) {

	        return (1.0/(integral(gamma, dmax)-integral(gamma, dmin)))*(integral(gamma+1, dmax)-integral(gamma+1, dmin));

        }
        //bisection method to find the inferior limit, in order to have the expected average degree
        private double solve_dmin(double dmax, double dmed, double gamma) 
        {
	
	        double dmin_l=1;
	        double dmin_r=dmax;
	        double average_k1=average_degree(dmin_r, dmin_l, gamma);
	        double average_k2=dmin_r;
	
	
	        if ((average_k1-dmed>0) || (average_k2-dmed<0)) 
            {
		
		        User.One.SendErrorToUser(new Exception("\n***********************\nERROR: the average degree is out of range:"));
		        if (average_k1-dmed>0) {
			        User.One.MessageToUser("\nyou should increase the average degree (bigger than "+average_k1.ToString()+")\n (or decrease the maximum degree...)\n"); 
		        }
		
		        if (average_k2-dmed<0) {
			        User.One.MessageToUser("\nyou should decrease the average degree (smaller than "+average_k2.ToString()+")\n (or increase the maximum degree...)\n");
		        }
		        return -1;	
	        }
	
		
	        while (Math.Abs(average_k1-dmed)>1e-7) 
            {
		
		        double temp=average_degree(dmax, ((dmin_r+dmin_l)/2.0), gamma);
		        if ((temp-dmed)*(average_k2-dmed)>0) {
			
			        average_k2=temp;
			        dmin_r=((dmin_r+dmin_l)/2.0);
		
		        }
		        else 
                {
			        average_k1=temp;
			        dmin_l=((dmin_r+dmin_l)/2.0);
		        }
	        }
	
	        return dmin_l;
        }
        // it computes the correct (i.e.
		// rete) average of a power law
        double integer_average(int n, int min, double tau) 
        {
	        double a=0;
	        for (double h=min; h<n+1; h++)
		        a+= Math.Pow((1.0/h),tau);
	
	        double pf=0;
	        for(double i=min; i<n+1; i++)
		        pf+=1/a*Math.Pow((1.0/(i)),tau)*i;
	        return pf;
        }
        // this function sets "cumulative" as the cumulative function of (1/x)^tau, with range= [min, n]
        //to draw a number: 
        //int nn=lower_bound(cumulative.begin(), cumulative.end(), ran4())-cumulative.begin()+min_degree;


        int powerlaw (int n, int min, double tau, List<double> cumulative) 
        {
	        cumulative.Clear();
	        double a=0;			
	        for (double h=min; h<n+1; h++)
		        a+= Math.Pow((1.0/h),tau);
	        double pf=0;
	        for(double i=min; i<n+1; i++) {
	
		        pf+=1/a*Math.Pow((1.0/(i)),tau);
		        cumulative.Add(pf);
	        }
	        return 0;	
        }
        const int R2_IM1 =2147483563;
        const int R2_IM2 =2147483399;
        const double R2_AM =1.0/R2_IM1;
        const int R2_IMM1 =R2_IM1-1;
        const int R2_IA1 =40014;
        const int R2_IA2 =40692;
        const int R2_IQ1 =53668;
        const int R2_IQ2 =52774;
        const int R2_IR1 =12211;
        const int R2_IR2 =3791;
        const int R2_NTAB =32;
        const int R2_NDIV =(1+R2_IMM1/R2_NTAB);
        const double R2_EPS =1.2e-7;
        const double R2_RNMX = (1.0 - R2_EPS);

        //static long idum2=123456789;
        //static long iy=0;
        //static long []iv=new long[R2_NTAB];
        
        //double ran2(long idum) {
        //    long j;
        //    long k;
	        
        //    double temp;

        //    if(idum<=0 || iy!=0){
        //        if(-(idum)<1) idum=1*(idum);
        //        else idum=-(idum);
        //        idum2=idum;
        //        for(j=R2_NTAB+7;j>=0;j--){
        //            k=(idum)/R2_IQ1;
        //            idum=R2_IA1*(idum-k*R2_IQ1)-k*R2_IR1;
        //            if(idum<0) idum+=R2_IM1;
        //            if(j<R2_NTAB) iv[j]=idum;
        //        }
        //        iy=iv[0];
        //    }
        //    k=(idum)/R2_IQ1;
        //    idum=R2_IA1*(idum-k*R2_IQ1)-k*R2_IR1;
        //    if(idum<0) idum+=R2_IM1;
        //    k=(idum2)/R2_IQ2;
        //    idum2=R2_IA2*(idum2-k*R2_IQ2)-k*R2_IR2;
        //    if (idum2 < 0) idum2 += R2_IM2;
        //    j=iy/R2_NDIV;
        //    iy=iv[j]-idum2;
        //    iv[j]=idum;
        //    if(iy<1) iy+=R2_IMM1;
        //    if((temp=R2_AM*iy)>R2_RNMX) return R2_RNMX;
        //    else return temp;
        //}
        //static long seed_ = 1;
        //double ran4(bool t, long s) 
        //{
	
        //    double r=0;
	        
        //    if(t)
        //        r=ran2(seed_);
        //    else
        //        seed_=s;
        //    return r;
        //}


        //double ran4() {
	
        //    return ran4(true, 0);
        //}
        //int irand(int n) 
        //{

        //    return (int)(ran4()*(n+1));
	
        //}
        int deque_int_sum(List<int>  a) 
        {
	        int s=0;
	        for(int i=0; i<a.Count; i++)
		        s+=a[i];
	        return s;
        }

        int build_bipartite_network(List<List<int> >  member_matrix, List<int>  member_numbers, List<int> num_seq) 
        {

		
	
	        // this function builds a bipartite network with num_seq and member_numbers which are the degree sequences. in member matrix links of the communities are stored
	        // this means member_matrix has num_seq.size() rows and each row has num_seq[i] elements
	
	
	
	        List<HashSet<int> > en_in=new List<HashSet<int>>();			// this is the Ein of the subgraph
	        List<HashSet<int> > en_out=new List<HashSet<int>>();		// this is the Eout of the subgraph
	
	
	        {
		        //HashSet<int> first=new HashSet<int>();
		        for(int i=0; i<member_numbers.Count; i++) {
                    en_in.Add(new HashSet<int>());
		        }
	        }
	
	        {
		        //HashSet<int> first=new HashSet<int>();
		        for(int i=0; i<num_seq.Count; i++) {
                    en_out.Add(new HashSet<int>());
		        }
	        }



            List<KeyValuePair<int, int>> degree_node_out = new List<KeyValuePair<int, int>>();
            List<KeyValuePair<int, int>> degree_node_in = new List<KeyValuePair<int, int>>();
	
	        for(int i=0; i<num_seq.Count; i++)
		        //degree_node_out.insert(make_pair(num_seq[i], i));
                degree_node_out.Add(new KeyValuePair<int, int>(num_seq[i], i));
	
	        for(int i=0; i<member_numbers.Count; i++)
                degree_node_in.Add(new KeyValuePair<int, int>(member_numbers[i], i));
	
	
	        //sort(degree_node_in.begin(), degree_node_in.end());
            //var sortdegree_node_in=from x in degree_node_in orderby x.First, x.Second select x;
            degree_node_in.Sort((l, r) => l.Value.CompareTo(r.Value));



            int itlast = degree_node_in.Count(); //degree_node_in.end();
	       
	        //while (itlast != degree_node_in.begin()) {
	        while (itlast >0) {
		
		        itlast--;
		
		        //multimap <int, int>::iterator itit= degree_node_out.end();
		        int itit= degree_node_out.Count;
                List<KeyValuePair<int, int>> erasenda = new List<KeyValuePair<int, int>>();
                
                for (int i = 0; i < degree_node_in.ElementAt(itlast).Key; i++)
                {
			
			        //if(itit!=degree_node_out.begin()) {
                    if(itit>0) {
				
				        itit--;
				
                        //en_in[itlast->second].insert(itit->second);
                        //en_out[itit->second].insert(itlast->second);

                        en_in[degree_node_in.ElementAt(itlast).Value].Add(degree_node_out.ElementAt(itit).Value);
                        en_out[degree_node_out.ElementAt(itit).Value].Add(degree_node_in.ElementAt(itlast).Value);
					
				        erasenda.Add(degree_node_out[itit]);
				
			        }
			
			        else
				        return -1;
		
		        }
		
		
		        //cout<<"degree node out before"<<endl;
		        //prints(degree_node_out);
		
		        for (int i=0; i<erasenda.Count; i++) {
			
			
			        if(erasenda[i].Key>1)
                        degree_node_out.Add(new KeyValuePair<int, int>(erasenda[i].Key - 1, erasenda[i].Value));
	
			
			        degree_node_out.Remove(erasenda[i]);

			
		
		        }
		
		        //cout<<"degree node out after"<<endl;
		        //prints(degree_node_out);
		
	        }
	
	
	        // this is to randomize the subgraph -------------------------------------------------------------------

	
	        for(int node_a=0; node_a<num_seq.Count; node_a++) for(int krm=0; krm<en_out[node_a].Count; krm++) {
				
				
		        int random_mate=NumericMath.RandomCraft.Next(member_numbers.Count);
		
				
		        //if (en_out[node_a].find(random_mate)==en_out[node_a].end()) {
                if (!en_out[node_a].Contains(random_mate)) {
			
			        List <int> external_nodes=new List<int>();
			        //for (set<int>::iterator it_est=en_out[node_a].begin(); it_est!=en_out[node_a].end(); it_est++)
                    for (HashSet<int>.Enumerator it_est=en_out[node_a].GetEnumerator(); it_est.MoveNext();)
				        //external_nodes.push_back(*it_est);
                        external_nodes.Add(it_est.Current);
						
										
			        int	old_node=external_nodes[NumericMath.RandomCraft.Next(external_nodes.Count)];
					
			
			        List <int> not_common=new List<int>();
			        //for (set<int>::iterator it_est=en_in[random_mate].begin(); it_est!=en_in[random_mate].end(); it_est++)
                    for (HashSet<int>.Enumerator it_est=en_in[random_mate].GetEnumerator(); it_est.MoveNext();)
				        //if (en_in[old_node].find(*it_est)==en_in[old_node].end())
                        if (!en_in[old_node].Contains(it_est.Current))
					        not_common.Add(it_est.Current);
					
			
			        if (not_common.Count==0)
				        break;

                    int node_h = not_common[NumericMath.RandomCraft.Next(not_common.Count)];
			
			
			        en_out[node_a].Add(random_mate);
			        en_out[node_a].Remove(old_node);
			
			        en_in[old_node].Add(node_h);
			        en_in[old_node].Remove(node_a);
			
			        en_in[random_mate].Add(node_a);
			        en_in[random_mate].Remove(node_h);
			
			        en_out[node_h].Remove(random_mate);
			        en_out[node_h].Add(old_node);
			

		        }
	
	
	        }

	
	
	        member_matrix.Clear();
	        //List <int> first2=new List<int>();
	
	        for (int i=0; i<en_out.Count; i++) {

                member_matrix.Add(new List<int>());
		        //for (set<int>::iterator its=en_out[i].begin(); its!=en_out[i].end(); its++)
                for (HashSet<int>.Enumerator its=en_out[i].GetEnumerator(); its.MoveNext();)
			        member_matrix[i].Add(its.Current);
		
			
	        }
	
	
	        return 0;


        }
        int compute_internal_degree_per_node(int d, int m, List<int> a) 
        {
	        // d is the internal degree
	        // m is the number of memebership 
	        a.Clear();
	        int d_i= d/m;
	        for (int i=0; i<m; i++)
		        a.Add(d_i);
		
	        for(int i=0; i<d%m; i++)
		        a[i]++;
	        return 0;
        }
        int change_community_size(List<int> seq) 
        {

	
			
	        if (seq.Count<=2)
		        return -1;
	
	        int min1=0;
	        int min2=0;
	
	        for (int i=0; i<seq.Count; i++)		
		        if (seq[i]<=seq[min1])
			        min1=i;
	
	        if (min1==0)
		        min2=1;
	
	        for (int i=0; i<seq.Count; i++)		
		        if (seq[i]<=seq[min2] && seq[i]>seq[min1])
			        min2=i;
	

	
	        seq[min1]+=seq[min2];
	
	        int c=seq[0];
	        seq[0]=seq[min2];
	        seq[min2]=c;
	        //seq.pop_front();
            seq.RemoveAt(0);
	
	
	        return 0;
        }

        int internal_degree_and_membership (double mixing_parameter, int overlapping_nodes, int max_mem_num, int num_nodes, List<List<int> > member_matrix, 
        bool excess, bool defect,  List<int>  degree_seq_in, List<int>  degree_seq_out, List<int> num_seq, List<int> internal_degree_seq_in, List<int> internal_degree_seq_out, bool fixed_range, int nmin, int nmax, double tau2) 
        {
	
	        if(num_nodes< overlapping_nodes) {
		
		        User.One.SendErrorToUser(new Exception("\n***********************\nERROR: there are more overlapping nodes than nodes in the whole network! Please, decrease the former ones or increase the latter ones"));
		        return -1;
	        }
	
	        // 
	        member_matrix.Clear();
	        internal_degree_seq_in.Clear();
	
	        List<double> cumulative=new List<double>();
	
	        // it assigns the internal degree to each node -------------------------------------------------------------------------
	        int max_degree_actual=0;		// maximum internal degree

	        for (int i=0; i<degree_seq_in.Count; i++) {
		
		        double interno=(1-mixing_parameter)*degree_seq_in[i];
		        int int_interno=(int)interno;


                if (NumericMath.RandomCraft.NextDouble() < (interno - int_interno))
			        int_interno++;
		
		        if (excess) {
			
			        while (   (  (double)(int_interno)/degree_seq_in[i] < (1-mixing_parameter) )  &&   (int_interno<degree_seq_in[i])   )
				        int_interno++;
		        }
		
		        if (defect) {
			
			        while (   (  (double)int_interno/degree_seq_in[i] > (1-mixing_parameter) )  &&   (int_interno>0)   )
				        int_interno--;
				
		
		        }

		
		
		
		        internal_degree_seq_in.Add(int_interno);
		
		
		        if (int_interno>max_degree_actual)
			        max_degree_actual=int_interno;
		
			
	        }
	
	        for (int i=0; i<degree_seq_out.Count; i++) {
		
		        double interno=(1-mixing_parameter)*degree_seq_out[i];
		        int int_interno=(int)interno;


                if (NumericMath.RandomCraft.NextDouble() < (interno - int_interno))
			        int_interno++;
		
		        if (excess) {
			
			        while (   (  (double)int_interno/degree_seq_out[i] < (1-mixing_parameter) )  &&   (int_interno<degree_seq_out[i])   )
				        int_interno++;
				
		
		        }
		
		
		        if (defect) {
			
			        while (   (  (double)int_interno/degree_seq_out[i] > (1-mixing_parameter) )  &&   (int_interno>0)   )
				        int_interno--;
				
		
		        }

		
		        internal_degree_seq_out.Add(int_interno);
			
	        }
	
	        // it assigns the community size sequence -----------------------------------------------------------------------------
	
	        powerlaw(nmax, nmin, tau2, cumulative);
	
	
	        if (num_seq.Count==0) {
		
		        int _num_=0;
		        if (!fixed_range && (max_degree_actual+1)>nmin) {
		
			        _num_=max_degree_actual+1;			// this helps the assignment of the memberships (it assures that at least one module is big enough to host each node)
			        num_seq.Add(max_degree_actual+1);
		
		        }
		
		
		        while (true) {
			
			
			        //int nn=lower_bound(cumulative.begin(), cumulative.end(), ran4())-cumulative.begin()+nmin;
                    int nn = Netutil.lower_bound<double>(cumulative, NumericMath.RandomCraft.NextDouble());//cumulative.IndexOf((from x in cumulative where x>= NumericMath.RandomCraft.NextDouble() select x).FirstOrDefault())+nmin;
			
			        if (nn+_num_<=num_nodes + overlapping_nodes * (max_mem_num-1) ) {
				
				        num_seq.Add(nn);				
				        _num_+=nn;
			
			        }
			        else
				        break;
			
			
		        }
		
		        //num_seq[min_element(num_seq.begin(), num_seq.end()) - num_seq.begin()]+=num_nodes + overlapping_nodes * (max_mem_num-1) - _num_;
                num_seq[num_seq.IndexOf(num_seq.Min())]+=num_nodes + overlapping_nodes * (max_mem_num-1) - _num_;
		
	        }
	
	
	        //cout<<"num_seq"<<endl;
	        //prints(num_seq);
	
	        int ncom=num_seq.Count;
	
	        //cout<<"\n----------------------------------------------------------"<<endl;


	        List<int> member_numbers=new List<int>();
	        for(int i=0; i<overlapping_nodes; i++)
		        member_numbers.Add(max_mem_num);
	        for(int i=overlapping_nodes; i<degree_seq_in.Count; i++)
		        member_numbers.Add(1);
	
	        //prints(member_numbers);
	        //prints(num_seq);
	
	        if(build_bipartite_network(member_matrix, member_numbers, num_seq)==-1) {
		
		        User.One.MessageToUser("it seems that the overlapping nodes need more communities that those I provided. Please increase the number of communities or decrease the number of overlapping nodes/n");
		        return -1;			
	
	        }

	
	
	        //printm(member_matrix);
	
	        //cout<<"degree_seq_in"<<endl;
	        //prints(degree_seq_in);
	
	        //cout<<"internal_degree_seq_in"<<endl;
	        //prints(internal_degree_seq_in);

	        List<int> available=new List<int>();
	        for (int i=0; i<num_nodes; i++)
		        available.Add(0);
	
	        for (int i=0; i<member_matrix.Count; i++) {
		        for (int j=0; j<member_matrix[i].Count; j++)
			        available[member_matrix[i][j]]+=member_matrix[i].Count-1;
	        }
	
	        //cout<<"available"<<endl;
	        //prints(available);
	
	
	        List<int> available_nodes=new List<int>();
	        for (int i=0; i<num_nodes; i++)
		        available_nodes.Add(i);
	
	
	        List<int> map_nodes=new List<int>();				// in the position i there is the new name of the node i
	        for (int i=0; i<num_nodes; i++)
		        map_nodes.Add(0);

	
	        for (int i=degree_seq_in.Count-1; i>=0; i--) {
		
		        int  degree_here=internal_degree_seq_in[i];
                int try_this = NumericMath.RandomCraft.Next(available_nodes.Count);
		
		        int kr=0;
		        while (internal_degree_seq_in[i] > available[available_nodes[try_this]]) {
		
			        kr++;
                    try_this = NumericMath.RandomCraft.Next(available_nodes.Count);
			        if(kr==3*num_nodes) {
			
				        if(change_community_size(num_seq)==-1) {
					
					        User.One.SendErrorToUser(new Exception("\n***********************\nERROR: this program needs more than one community to work fine\n"));
					        return -1;
				
				        }
				
				        User.One.MessageToUser("it took too long to decide the memberships; I will try to change the community sizes\n");

				        User.One.MessageToUser("new community sizes\n");
				        for (int j=0; j<num_seq.Count; j++)
					        User.One.MessageToUser(num_seq[j].ToString()+" ");
                        User.One.MessageToUser("\n\n");
				
				        return (internal_degree_and_membership(mixing_parameter, overlapping_nodes, max_mem_num, num_nodes, member_matrix, excess, defect, degree_seq_in, degree_seq_out, num_seq, internal_degree_seq_in, internal_degree_seq_out, fixed_range, nmin, nmax, tau2));
			
			
			        }
			
			
		        }
		
		
		
		        map_nodes[available_nodes[try_this]]=i;
		
		        available_nodes[try_this]=available_nodes[available_nodes.Count-1];
		        //available_nodes.pop_back();
                available_nodes.RemoveAt(available_nodes.Count-1);
		
	            
	
	        }
	
	
	        for (int i=0; i<member_matrix.Count; i++) {
		        for (int j=0; j<member_matrix[i].Count; j++)
			        member_matrix[i][j]=map_nodes[member_matrix[i][j]];	
	        }
	
	
	
	        for (int i=0; i<member_matrix.Count; i++)
		        //sort(member_matrix[i].begin(), member_matrix[i].end());
                member_matrix[i].Sort();

		
	        return 0;

        }
        static int CompareIntPair(Pair<int, int> a, Pair<int, int> b)
        {
            return a.First.CompareTo(b.First);
        }
        int build_subgraph(List<HashSet<int> > Ein, List<HashSet<int> > Eout, List<int> nodes, List<int> d_in, List<int> d_out) 
        {
	
	
	       
	
	        if(d_in.Count<3) {
		
		        //User.One.MessageToUser("it seems that some communities should have only 2 nodes! This does not make much sense (in my opinion) Please change some parameters!\n");
		        return -1;
	
	        }
	
	       
	
	
	        // labels will be placed in the end
	        List<HashSet<int> > en_in=new List<HashSet<int>>();			// this is the Ein of the subgraph
	        List<HashSet<int> > en_out=new List<HashSet<int>>();		// this is the Eout of the subgraph
	
	
	        {
		        //HashSet<int> first=new HashSet<int>();
		        for(int i=0; i<nodes.Count; i++) {
                    en_in.Add(new HashSet<int>());
                    en_out.Add(new HashSet<int>());
		        }
	        }
	
	
	
	        //multimap <int, int> degree_node_out;
            List<Pair<int, int>> degree_node_out=new  List<Pair<int,int>>();
	        List<Pair<int, int> > degree_node_in=new List<Pair<int,int>>();
	
	        for(int i=0; i<d_out.Count; i++)
		        degree_node_out.Add(new Pair<int,int>(d_out[i], i));
		
		
	        List<int> fakes=new List<int>();
	        for(int i=0; i<d_in.Count; i++)
		        //fakes.push_back(i);
                fakes.Add(i);
	
	        Netutil.Shuffle<int>(fakes);
	

	        //List<int> antifakes=new List<int>(fakes.Count);
            int[] antifakes = new int [fakes.Count];
	        for(int i=0; i<d_in.Count; i++)
		        antifakes[fakes[i]]=i;
	
	
	
	        for(int i=0; i<d_in.Count; i++)
		        degree_node_in.Add(new Pair<int,int>(d_in[i], fakes[i]));
	

	        //sort(degree_node_in.begin(), degree_node_in.end());
            degree_node_in.Sort(CompareIntPair);
	

	        for(int i=0; i<d_in.Count; i++)
		        degree_node_in[i]=new Pair<int,int>(degree_node_in[i].First,antifakes[degree_node_in[i].Second]);
            

	
	        //deque<pair<int, int> >::iterator itlast = degree_node_in.end();
            int itlast = degree_node_in.Count;
	
	        /*
	        for (int i=0; i<degree_node_in.size(); i++)
		        cout<<degree_node_in[i].first<<" "<<degree_node_in[i].second<<endl;
	        //*/
	
	        List<int> self_loop=new List<int>();
	
	        int inserted=0;
	
	        while (itlast != 0) {
		
		        itlast--;
		
		
		        //multimap <int, int>::iterator itit= degree_node_out.end();
                int itit= degree_node_out.Count;
		        //deque <multimap<int, int>::iterator> erasenda;
                List <Pair<int,int>> erasenda=new List<Pair<int,int>>();
		
		        for (int i=0; i<degree_node_in[itlast].First; i++) {
			
			        //if(itit!=degree_node_out.begin()) {
                    if(itit!=0) {
				
				        itit--;
				
				        //if (itit->second!=itlast->second) {
					    if (degree_node_out[itit].Second!=degree_node_in[itlast].Second) {
					        en_in[degree_node_in[itlast].Second].Add(degree_node_out[itit].Second);
					        en_out[degree_node_out[itit].Second].Add(degree_node_in[itlast].Second);
					        inserted++;
					
				        }
				        else
					        self_loop.Add(degree_node_in[itlast].Second);
				

				        //erasenda.push_back(itit);
                        erasenda.Add(degree_node_out[itit]);
				
			        }
			
			        else
				        break;
		
		        }
		
		
		        //cout<<"degree node out before"<<endl;
		        //prints(degree_node_out);
		
		        for (int i=0; i<erasenda.Count; i++) {
			
			
			        if(erasenda[i].First>1)
				        degree_node_out.Add(new Pair<int,int>(erasenda[i].First - 1, erasenda[i].Second));
	
			
			        degree_node_out.Remove(erasenda[i]);

			
		
		        }
		
		        //cout<<"degree node out after"<<endl;
		        //prints(degree_node_out);
		
	        }
	
	        //cout<<inserted<<"<------ inserted"<<endl; 
	
	        //cout<<"left "<<degree_node_out.size()<<endl;
	        //cout<<"self loops "<<self_loop.size()<<endl;
	        int not_done=0;
	
	        for(int i=0; i<self_loop.Count; i++) {
	
		        int node=self_loop[i];

		        int stopper=d_in.Count*d_in.Count;
		        int stop=0;
		
		        //cout<<"node "<<nodes[node]<<endl;
		
		        bool breaker=false;

		        while (stop++ < stopper) {
			
			
			        while(true) {


                        int random_mate = NumericMath.RandomCraft.Next(d_in.Count);
				        if(random_mate==node || en_in[node].Contains(random_mate))
					        break;
				
				        List <int> not_common=new List<int>();
				        for (HashSet<int>.Enumerator it_est=en_out[random_mate].GetEnumerator(); it_est.MoveNext();)
					        if (!en_out[node].Contains(it_est.Current))
						        not_common.Add(it_est.Current);
					
			
				        if (not_common.Count==0)
					        break;

                        int random_neigh = not_common[NumericMath.RandomCraft.Next(not_common.Count)];

				
				        en_out[node].Add(random_neigh);
				        en_in[node].Add(random_mate);
				
				
				        en_in[random_neigh].Add(node);
				        en_in[random_neigh].Remove(random_mate);
				
				        en_out[random_mate].Add(node);
				        en_out[random_mate].Remove(random_neigh);
				
				        breaker=true;
				        break;
			
			
			        }
			
			        if(breaker)
				        break;
			
		
		        }
		
		        if(!breaker)
			        not_done++;
		
		
	        }
	
	        //cout<<"not done "<<not_done<<endl;
	
	        // this is to randomize the subgraph -------------------------------------------------------------------

	
	        for(int node_a=0; node_a<d_in.Count; node_a++) for(int krm=0; krm<en_out[node_a].Count; krm++) {
				
				
		        //int random_mate=irand(d_in.Count-1);
                int random_mate = Mathutil.NumericMath.RandomCraft.Next(d_in.Count);
		        while (random_mate==node_a)
			        //random_mate=irand(d_in.Count-1);
                    random_mate = Mathutil.NumericMath.RandomCraft.Next(d_in.Count);
				
		
		

		
		        if (!en_out[node_a].Contains(random_mate)) {
			
			        List <int> external_nodes=new List<int>();
			        for (HashSet<int>.Enumerator it_est=en_out[node_a].GetEnumerator(); it_est.MoveNext();)
				        external_nodes.Add(it_est.Current);


                    int old_node = external_nodes[NumericMath.RandomCraft.Next(external_nodes.Count)];
					
			
			        List <int> not_common=new List<int>();
			        for (HashSet<int>.Enumerator it_est=en_in[random_mate].GetEnumerator(); it_est.MoveNext();)
				        if ((old_node!=(it_est.Current)) && (!en_in[old_node].Contains(it_est.Current)))
					        not_common.Add(it_est.Current);
					
			
			        if (not_common.Count==0)
				        break;

                    int node_h = not_common[NumericMath.RandomCraft.Next(not_common.Count)];
			
			
			        en_out[node_a].Add(random_mate);
			        en_out[node_a].Remove(old_node);
			
			        en_in[old_node].Add(node_h);
			        en_in[old_node].Remove(node_a);
			
			        en_in[random_mate].Add(node_a);
			        en_in[random_mate].Remove(node_h);
			
			        en_out[node_h].Remove(random_mate);
			        en_out[node_h].Add(old_node);
			

		        }
	
	
	        }

	
	
	// now I try to insert the new links into the already done network. If some multiple links come out, I try to rewire them
	
	List < Pair<int, int> > multiple_edge=new List<Pair<int,int>>();
	for (int i=0; i<en_in.Count; i++) {
		
		for(HashSet<int>.Enumerator its=en_in[i].GetEnumerator(); its.MoveNext();) {
		
			//bool already = !(Ein[nodes[i]].insert(nodes[*its]).second) ;		// true is the insertion didn't take place
            bool already = Ein[nodes[i]].Contains(nodes[its.Current]);		// true is the insertion didn't take place
            if(!already)
                Ein[nodes[i]].Add(nodes[its.Current]);	

			if (already)
				multiple_edge.Add(new Pair<int,int>(nodes[i], nodes[its.Current]));			
			else
				Eout[nodes[its.Current]].Add(nodes[i]);
		}
	
	
	}
	
	
	//cout<<"multiple "<<multiple_edge.size()<<endl;
	
	for (int i=0; i<multiple_edge.Count; i++) {
		
		
		int a = multiple_edge[i].First;
		int b = multiple_edge[i].Second;
		
	
		// now, I'll try to rewire this multiple link among the nodes stored in nodes.
		int stopper_ml=0;
		
		while (true) {
					
			stopper_ml++;

            int random_mate = nodes[NumericMath.RandomCraft.Next(d_in.Count)];
			while (random_mate==a || random_mate==b)
                random_mate = nodes[NumericMath.RandomCraft.Next(d_in.Count)];
			
			if(!Ein[a].Contains(random_mate)) {
				
				List <int> not_common=new List<int>();
				for (HashSet<int>.Enumerator it_est=Eout[random_mate].GetEnumerator(); it_est.MoveNext();)
					//if ((b!=(*it_est)) && (Eout[b].find(*it_est)==Eout[b].end()) && (binary_search(nodes.begin(), nodes.end(), *it_est)))
                    if ((b != (it_est.Current)) && (!Eout[b].Contains(it_est.Current)) && (nodes.BinarySearch(it_est.Current) >= 0))
						not_common.Add(it_est.Current);
				
				if(not_common.Count>0) {

                    int node_h = not_common[NumericMath.RandomCraft.Next(not_common.Count)];
					
					
					
					Eout[random_mate].Add(a);
					Eout[random_mate].Remove(node_h);
					
					Ein[node_h].Remove(random_mate);
					Ein[node_h].Add(b);
					
					Eout[b].Add(node_h);
					Ein[a].Add(random_mate);
					
					break;

				
			
				}
			
			}
			
			if(stopper_ml==2*Ein.Count) {
	
				User.One.MessageToUser("sorry, I need to change the degree distribution a little bit (one less link)\n");
				break;
	
			}
			
			
			
		}
	
	
	}
	
	

	
	
	return 0;

}



        int build_subgraphs(List<HashSet<int> > Ein, List<HashSet<int> > Eout, List<List<int> > member_matrix, List<List<int> > member_list, List<List<int> > link_list_in, List<List<int> > link_list_out, 
	List<int> internal_degree_seq_in, List<int> degree_seq_in, List<int> internal_degree_seq_out, List<int> degree_seq_out, bool excess, bool defect) {
	
	
	
	Ein.Clear();
    Eout.Clear();
    member_list.Clear();
    link_list_in.Clear();
    link_list_out.Clear();
	
	int num_nodes=degree_seq_in.Count();
	
	
	
	{

        //List<int> first=new List<int>();
		for (int i=0; i<num_nodes; i++)
			member_list.Add(new List<int>());
	
	}
	
	
	
	for (int i=0; i<member_matrix.Count; i++)
        for (int j = 0; j < member_matrix[i].Count; j++)
			member_list[member_matrix[i][j]].Add(i);
	
		
	for (int i=0; i<member_list.Count; i++) {

        List<int> liin=new List<int>();
        List<int> liout=new List<int>();

		
		for (int j=0; j<member_list[i].Count; j++) {
			
			compute_internal_degree_per_node(internal_degree_seq_in[i], member_list[i].Count, liin);
			liin.Add(degree_seq_in[i] - internal_degree_seq_in[i]);
            compute_internal_degree_per_node(internal_degree_seq_out[i], member_list[i].Count, liout);
            liout.Add(degree_seq_out[i] - internal_degree_seq_out[i]);

		
		}

        link_list_in.Add(liin);
        link_list_out.Add(liout);
		
	}
	
	
	/*
	cout<<"link list in out ************************"<<endl;
	printm(link_list_in);
	printm(link_list_out);
	cout<<"link list in out ************************"<<endl;
	*/
	
	// ------------------------ this is done to check if the sums of the internal degrees (in and out) are equal. if not, the program will change it in such a way to assure that. 
	
	
			
	for (int i=0; i<member_matrix.Count; i++) {
	
		
		int internal_cluster_in=0;
		int internal_cluster_out=0;
		
		
		for (int j=0; j<member_matrix[i].Count; j++) {
			
			//int right_index= lower_bound(member_list[member_matrix[i][j]].begin(), member_list[member_matrix[i][j]].end(), i) - member_list[member_matrix[i][j]].begin();
            int right_index = Netutil.lower_bound<int>(member_list[member_matrix[i][j]], i);
			internal_cluster_in+=link_list_in[member_matrix[i][j]][right_index];
			internal_cluster_out+=link_list_out[member_matrix[i][j]][right_index];
		}
		
		//cout<<"internal_cluster difference "<<internal_cluster_in - internal_cluster_out<<" for nodes: "<<member_matrix[i].size()<<endl;
		
		
		int initial_diff= Math.Abs(internal_cluster_in - internal_cluster_out);
		for(int diffloop=0; diffloop<3*initial_diff; diffloop++) {
			
			
			if((internal_cluster_in - internal_cluster_out)==0)
				break;
			
						
			
				
			// if this does not work in a reasonable time the degree sequence will be changed
				
			for (int j=0; j<member_matrix[i].Count; j++) {



                int random_mate = member_matrix[i][NumericMath.RandomCraft.Next(member_matrix[i].Count)];
				//int right_index= lower_bound(member_list[random_mate].begin(), member_list[random_mate].end(), i) - member_list[random_mate].begin();
                //int right_index = member_list[random_mate].IndexOf((from x in member_list[random_mate] where x >= i select x).FirstOrDefault());
                int right_index = Netutil.lower_bound<int>(member_list[random_mate], i);//member_list[random_mate].IndexOf(Netutil.lower_bound<int>(member_list[random_mate], i));
				
				if(internal_cluster_in>internal_cluster_out) {

                    if ((link_list_out[random_mate][right_index] < member_matrix[i].Count - 1) && (link_list_out[random_mate][link_list_out[random_mate].Count - 1] > 0))
                    {
					
						link_list_out[random_mate][right_index]++;
                        link_list_out[random_mate][link_list_out[random_mate].Count - 1]--;
						internal_cluster_out++;
						
						break;
					}
				}
				
				else if (link_list_out[random_mate][right_index] > 0) {
					
					link_list_out[random_mate][right_index]--;
                    link_list_out[random_mate][link_list_out[random_mate].Count - 1]++;
					internal_cluster_out--;

					break;
				
				}
			
			}			
					
		}


		//cout<<"internal_cluster difference after "<<internal_cluster_in - internal_cluster_out<<endl;
		
		for(int diffloop=0; diffloop<3*initial_diff; diffloop++) {
			
			
			if((internal_cluster_in - internal_cluster_out)==0)
				break;
			
						
			
				
			// if this does not work in a reasonable time the degree sequence will be changed

            for (int j = 0; j < member_matrix[i].Count; j++)
            {



                int random_mate = member_matrix[i][NumericMath.RandomCraft.Next(member_matrix[i].Count)];
				//int right_index= lower_bound(member_list[random_mate].begin(), member_list[random_mate].end(), i) - member_list[random_mate].begin();
                int right_index = Netutil.lower_bound<int>(member_list[random_mate], i);
				
				if(internal_cluster_in>internal_cluster_out) {
					
					if ((link_list_out[random_mate][right_index]<member_matrix[i].Count-1)) {
					
						link_list_out[random_mate][right_index]++;
						internal_cluster_out++;
						
						break;
					}
				}
				
				else {
					
					link_list_out[random_mate][right_index]--;
					internal_cluster_out--;

					break;
				
				}
			
			}			
					
		}
		
		
		
		//cout<<"internal_cluster difference after after "<<internal_cluster_in - internal_cluster_out<<endl;
	
	}
	
	
	// ------------------------ this is done to check if the sums of the internal degrees (in and out) are equal. if not, the program will change it in such a way to assure that. 
	
	
		
	{
	
		//HashSet<int> first=new HashSet<int>();
		for(int i=0; i<num_nodes; i++) {
			Ein.Add(new HashSet<int>());
            Eout.Add(new HashSet<int>());
			
		}
	
	}
	
	for (int i=0; i<member_matrix.Count; i++) {
		
		
		List<int> internal_degree_in=new List<int>();
        List<int> internal_degree_out=new List<int>();

		for (int j=0; j<member_matrix[i].Count; j++) {
		
			//int right_index= lower_bound(member_list[member_matrix[i][j]].begin(), member_list[member_matrix[i][j]].end(), i) - member_list[member_matrix[i][j]].begin();
            int right_index = Netutil.lower_bound<int>(member_list[member_matrix[i][j]], i);

			internal_degree_in.Add(link_list_in[member_matrix[i][j]][right_index]);
            internal_degree_out.Add(link_list_out[member_matrix[i][j]][right_index]);

		}		
		

		
		if(build_subgraph(Ein, Eout, member_matrix[i], internal_degree_in, internal_degree_out)==-1)
			return -1;
	
	
	}




	return 0;
	
}

        bool they_are_mate(int a, int b, List<List<int> > member_list) 
        {


	        for(int i=0; i<member_list[a].Count; i++) {
		
		        //if(binary_search(member_list[b].begin(), member_list[b].end(), member_list[a][i]))
                if (member_list[b].BinarySearch(member_list[a][i])>=0)
			        return true;
	
	        }

	        return false;

        }

        int compute_var_mate(List<HashSet<int> >  en_in,  List<List<int> >  member_list) 
        {



	    int var_mate=0;
	    for(int i=0; i<en_in.Count; i++) for(HashSet<int>.Enumerator itss= en_in[i].GetEnumerator(); itss.MoveNext();) if(they_are_mate(i, itss.Current, member_list)) {
		    var_mate++;
	    }



	return var_mate;
}


        int connect_all_the_parts(List<HashSet<int> > Ein, List<HashSet<int> > Eout, List<List<int> > member_list, List<List<int> > link_list_in, List<List<int> > link_list_out) 
        {

	
	List<int> d_in=new List<int>();
	for(int i=0; i<link_list_in.Count; i++)
		d_in.Add(link_list_in[i][link_list_in[i].Count-1]);
	
	
	List<int> d_out=new List<int>();
	for(int i=0; i<link_list_out.Count; i++)
		d_out.Add(link_list_out[i][link_list_out[i].Count-1]);
		
	/*
	prints(d_in);
	prints(d_out);
	*/

	
	List<HashSet<int> > en_in=new List<HashSet<int>>();			// this is the Ein of the subgraph
	List<HashSet<int> > en_out=new List<HashSet<int>>();		// this is the Eout of the subgraph
	
	
	{
		//HashSet<int> first=new HashSet<int>();
		for(int i=0; i<member_list.Count; i++) {
			en_in.Add(new HashSet<int>());
            en_out.Add(new HashSet<int>());
		}
	}
	
	
	
	List<Pair<int, int>> degree_node_out=new List<Pair<int,int>>();
	List<Pair<int, int> > degree_node_in=new List<Pair<int,int>>();
	
	for(int i=0; i<d_out.Count; i++)
		degree_node_out.Add(new Pair<int,int>(d_out[i], i));
	
	List<int> fakes=new List<int>();
	for(int i=0; i<d_in.Count; i++)
		fakes.Add(i);
	
	Netutil.Shuffle<int>(fakes);
	
	//prints(fakes);
	//List<int> antifakes=new List<int>(fakes.Count);
    int[] antifakes = new int[fakes.Count];
	for(int i=0; i<d_in.Count; i++)
		antifakes[fakes[i]]=i;
	
	
	
	for(int i=0; i<d_in.Count; i++)
		degree_node_in.Add(new Pair<int,int>(d_in[i], fakes[i]));
	
	//printm(degree_node_in);

	//sort(degree_node_in.begin(), degree_node_in.end());
    degree_node_in.Sort((l, r) => l.Second.CompareTo(r.Second));
	
	//printm(degree_node_in);

	for(int i=0; i<d_in.Count; i++)
		degree_node_in[i]=new Pair<int,int>(degree_node_in[i].First,antifakes[degree_node_in[i].Second]);
	
	/*
	prints(d_in);
	printm(degree_node_in);
	*/
	
	//deque<pair<int, int> >::iterator itlast = degree_node_in.end();
            int itlast = degree_node_in.Count;
	
	List<int> self_loop=new List<int>();
	
	
	//cout<<"difference in connect_all_parts "<<deque_int_sum(d_in) - deque_int_sum(d_out)<<endl;
	//while (itlast != degree_node_in.begin()) {
            while (itlast != 0) {
		
		itlast--;
		
		
		//multimap <int, int>::iterator itit= degree_node_out.end();
		//deque <multimap<int, int>::iterator> erasenda;
        int itit= degree_node_out.Count;
		List <Pair<int, int>> erasenda=new List<Pair<int,int>>();
		
		
		for (int i=0; i<degree_node_in[itlast].First; i++) {
			
			//if(itit!=degree_node_out.begin()) {
			if(itit!=0) {
				itit--;
				
				if (degree_node_out[itit].Second!=degree_node_in[itlast].Second) {
					
					en_in[degree_node_in[itlast].Second].Add(degree_node_out[itit].Second);
					en_out[degree_node_out[itit].Second].Add(degree_node_in[itlast].Second);
				
				}
				else
					self_loop.Add(degree_node_in[itlast].Second);
				

				erasenda.Add(degree_node_out[itit]);
				
			}
			
			else
				break;
		
		}
		
		
		for (int i=0; i<erasenda.Count; i++) {
			
			
			if(erasenda[i].First>1)
				degree_node_out.Add(new Pair<int,int>(erasenda[i].First - 1, erasenda[i].Second));
	
			
			degree_node_out.Remove(erasenda[i]);

			
		
		}

		
		
	}
	
	//cout<<"left "<<degree_node_out.size()<<endl;
	//cout<<"self loops "<<self_loop.size()<<endl;
		
	for(int i=0; i<self_loop.Count; i++) {
	
		int node=self_loop[i];

		int stopper=d_in.Count*d_in.Count;
		int stop=0;
		
		//cout<<"node "<<node<<endl;
		
		bool breaker=false;

		while (stop++ < stopper) {
		
			//cout<<stop<<" "<<node<<endl;
			
			while(true) {


                int random_mate = NumericMath.RandomCraft.Next(d_in.Count);
				if(random_mate==node || en_in[node].Contains(random_mate))
					break;
				
				List <int> not_common=new List<int>();
				for (HashSet<int>.Enumerator it_est=en_out[random_mate].GetEnumerator(); it_est.MoveNext();)
					if (!en_out[node].Contains(it_est.Current))
						not_common.Add(it_est.Current);
					
				if (not_common.Count==0)
					break;



                int random_neigh = not_common[NumericMath.RandomCraft.Next(not_common.Count)];

				
				en_out[node].Add(random_neigh);
				en_in[node].Add(random_mate);
				
				
				en_in[random_neigh].Add(node);
				en_in[random_neigh].Remove(random_mate);
				
				en_out[random_mate].Add(node);
				en_out[random_mate].Remove(random_neigh);
				
				breaker=true;
				break;
			
			
			}
			
			if(breaker)
				break;
			
		
		}
		
		
		


	}

	
	// this is to randomize the subgraph -------------------------------------------------------------------

	
	for(int node_a=0; node_a<d_in.Count; node_a++) for(int krm=0; krm<en_out[node_a].Count; krm++) {


        int random_mate = NumericMath.RandomCraft.Next(d_in.Count);
		while (random_mate==node_a)
            random_mate = NumericMath.RandomCraft.Next(d_in.Count);
				
		
		

		
		if (!en_out[node_a].Contains(random_mate)) {
			
			List <int> external_nodes=new List<int>();
			for (HashSet<int>.Enumerator it_est=en_out[node_a].GetEnumerator(); it_est.MoveNext();)
				external_nodes.Add(it_est.Current);


            int old_node = external_nodes[NumericMath.RandomCraft.Next(external_nodes.Count)];
					
			
			List <int> not_common=new List<int>();
			for (HashSet<int>.Enumerator it_est=en_in[random_mate].GetEnumerator(); it_est.MoveNext();)
				if ((old_node!=(it_est.Current)) && (!en_in[old_node].Contains(it_est.Current)))
					not_common.Add(it_est.Current);
					
			
			if (not_common.Count==0)
				break;

            int node_h = not_common[NumericMath.RandomCraft.Next(not_common.Count)];
			
			
			en_out[node_a].Add(random_mate);
			en_out[node_a].Remove(old_node);
			
			en_in[old_node].Add(node_h);
			en_in[old_node].Remove(node_a);
			
			en_in[random_mate].Add(node_a);
			en_in[random_mate].Remove(node_h);
			
			en_out[node_h].Remove(random_mate);
			en_out[node_h].Add(old_node);
			

		}
	
	
	}

	
	
	// now there is a rewiring process to avoid "mate nodes" (nodes with al least one membership in common) to link each other
	
	int var_mate= compute_var_mate(en_in, member_list);	
	//cout<<"var mate = "<<var_mate<<endl;
	
	int stopper_mate=0;
	int mate_trooper=10;
	
	while(var_mate>0) {
	
		
		//cout<<"var mate = "<<var_mate<<endl;

		
		int best_var_mate=var_mate;
	
		// ************************************************  rewiring
		
		
		for(int a=0; a<d_in.Count; a++) for(HashSet<int>.Enumerator its= en_in[a].GetEnumerator(); its.MoveNext(); ) if(they_are_mate(a, its.Current, member_list)) {
				
			
			
			int b=its.Current;
			int stopper_m=0;
			
			while (true) {
						
				stopper_m++;

                int random_mate = NumericMath.RandomCraft.Next(d_in.Count);
				while (random_mate==a || random_mate==b)
                    random_mate = NumericMath.RandomCraft.Next(d_in.Count);
				
				
				if(!(they_are_mate(a, random_mate, member_list)) && (!en_in[a].Contains(random_mate))) {
					
					List <int> not_common=new List<int>();
					for (HashSet<int>.Enumerator it_est=en_out[random_mate].GetEnumerator(); it_est.MoveNext();)
						if ((b!=(it_est.Current)) && (!en_out[b].Contains(it_est.Current)))
							not_common.Add(it_est.Current);
					
					if(not_common.Count>0) {

                        int node_h = not_common[NumericMath.RandomCraft.Next(not_common.Count)];
						
						
						en_out[random_mate].Remove(node_h);
						en_out[random_mate].Add(a);
						
						en_in[node_h].Remove(random_mate);
						en_in[node_h].Add(b);
						
						en_out[b].Remove(a);
						en_out[b].Add(node_h);
						
						en_in[a].Add(random_mate);
						en_in[a].Remove(b);
						
											
						
						if(!they_are_mate(b, node_h, member_list))
							var_mate--;
						
						
						if(they_are_mate(random_mate, node_h, member_list))
							var_mate--;
						
						break;

					
				
					}
				
				}
				
				if(stopper_m==en_in[a].Count)
					break;
				
				
				
			}
				
			
			break;		// this break is done because if you erased some link you have to stop this loop (en[i] changed)
	
	
		}

		// ************************************************  rewiring
		
		
				

		if(var_mate==best_var_mate) {
			
			stopper_mate++;
			
			if(stopper_mate==mate_trooper)
				break;

		}
		else
			stopper_mate=0;
		
		
		
		//cout<<"var mate = "<<var_mate<<endl;

	
	}
	
	
	
	//cout<<"var mate = "<<var_mate<<endl;

	for (int i=0; i<en_in.Count; i++) {
		
		for(HashSet<int>.Enumerator its=en_in[i].GetEnumerator(); its.MoveNext(); ) {
		
			Ein[i].Add(its.Current);
			Eout[its.Current].Add(i);
			
		
		}
	
	
	}
	
	
	
	return 0;

}
        int internal_kin(List<HashSet<int> > Ein, List<List<int> > member_list, int i) 
        {
	
	        int var_mate2=0;
	        for(HashSet<int>.Enumerator itss= Ein[i].GetEnumerator(); itss.MoveNext(); ) if(they_are_mate(i, itss.Current, member_list)) 
		        var_mate2++;	

	        return var_mate2;
	
        }


        int erase_links(List<HashSet<int> > Ein, List<HashSet<int> > Eout, List<List<int> > member_list, bool excess, bool defect, double mixing_parameter) 
        {

	
	            int num_nodes= member_list.Count;
	
	            int eras_add_times=0;
	
	            if (excess) {
		
		            for (int i=0; i<num_nodes; i++) {
			
			
			            while ( (Ein[i].Count>1) &&  (double)(internal_kin(Ein, member_list, i))/Ein[i].Count < 1 - mixing_parameter) {
			
			            //---------------------------------------------------------------------------------
				
				
				            User.One.MessageToUser("degree sequence changed to respect the option -sup ... "+(++eras_add_times).ToString()+"\n");
				
				            List<int> deqar=new List<int>();
				            for (HashSet<int>.Enumerator it_est=Ein[i].GetEnumerator(); it_est.MoveNext(); )
					            if (!they_are_mate(i, it_est.Current, member_list))
						            deqar.Add(it_est.Current);
				
				
				            if(deqar.Count==Ein[i].Count) {	// this shouldn't happen...
				
					            User.One.SendErrorToUser(new Exception("sorry, something went wrong: there is a node which does not respect the constraints. (option -sup)\n"));
					            return -1;
				
				            }

                            int random_mate = deqar[NumericMath.RandomCraft.Next(deqar.Count)];
				
				            Ein[i].Remove(random_mate);
				            Eout[random_mate].Remove(i);
				
		
			            }
		            }
	
	            }
	
	
	
	            if (defect) {
			
		            for (int i=0; i<num_nodes; i++)
			            while ( (Ein[i].Count<Ein.Count) &&  (double)(internal_kin(Ein, member_list, i))/Ein[i].Count > 1 - mixing_parameter) {
				
				            //---------------------------------------------------------------------------------
					
				
				            User.One.MessageToUser("degree sequence changed to respect the option -inf ... "+(++eras_add_times).ToString()+"\n");


				            int stopper_here=num_nodes;
				            int stopper_=0;

                            int random_mate = NumericMath.RandomCraft.Next(num_nodes);
				            while ( (    (they_are_mate(i, random_mate, member_list)) || Ein[i].Contains(random_mate))      &&      (stopper_<stopper_here) ) {

                                random_mate = NumericMath.RandomCraft.Next(num_nodes);
					            stopper_++;
				
				
				            }
				
				            if(stopper_==stopper_here) {	// this shouldn't happen...
				
					            User.One.SendErrorToUser(new Exception("sorry, something went wrong: there is a node which does not respect the constraints. (option -inf)\n"));
					            return -1;
				
				            }
				
				
				
				            Ein[i].Add(random_mate);
                            Eout[random_mate].Add(i);
				
								
		
			            }
			
		
	            }

	            //------------------------------------ Erasing links   ------------------------------------------------------

	


	            return 0;
	
            }
        int internal_kin_only_one(HashSet<int> Ein, List<int> member_matrix_j) 
        {		// return the overlap between Ein and member_matrix_j
	
	        int var_mate2=0;
	
	        for(HashSet<int>.Enumerator itss= Ein.GetEnumerator(); itss.MoveNext(); ) {
	
		        //if(binary_search(member_matrix_j.begin(), member_matrix_j.end(), *itss))
                if (member_matrix_j.BinarySearch(itss.Current)>=0)
			        var_mate2++;
	
	        }
	
	        return var_mate2;
	
        }
        
        double average_func(IList<double> sq) 
        {
	
	        if (sq.Count==0)
		        return 0;
	
	        double av=0;
            IEnumerator<double> it = sq.GetEnumerator(); 
	        while(it.MoveNext())
		        av+=it.Current;
	
	        av=av/sq.Count;
	
	        return av;
	    }
        
        double variance_func(List<double> sq) {
	
	        if (sq.Count==0)
		        return 0;
	
	        double av=0;
	        double var=0;
	
	
	        IEnumerator<double> it = sq.GetEnumerator();
	        while(it.MoveNext()) 
            {
		        av+=it.Current;
		        var+=(it.Current)*(it.Current);
	        }
	
	
	        av=av/sq.Count;
	        var=var/sq.Count;
	        var-=av*av;
	
	        if(var<1e-7)
		        return 0;
	
	        return var;
	
        }
        int log_histogram(List<int> c, string ostreamout, int number_of_bins) 
        {		// c is the set od data, min is the lower bound, max is the upper one
	
	
	
	        List <int> d=new List<int>();
	        for(int i=0; i<c.Count; i++) if (c[i]>0)
		        d.Add(c[i]);
	
	        c.Clear();
	        c=d;
	
	        double min=(double)(c[0]);
	        double max=(double)(c[0]);
	
	        for (int i=0; i<c.Count; i++) {
		
		        if (min>(double)(c[i]))
			        min=(double)(c[i]);
		
		        if (max<(double)(c[i]))
			        max=(double)(c[i]);
		
	        }
	
	
	
	
	        List <int> hist=new List<int>();
	        List <double> hist2=new List<double>();
	        List <double> bins=new List<double>();
	        double step=Math.Log(min);
	        if (max==min)
		        max++;
	
	        double bin=(Math.Log(max)-Math.Log(min))/number_of_bins;		// bin width
	
		

	        while (step<=Math.Log(max)+2*bin) {
		
		
		        bins.Add(Math.Exp(step));
		        hist.Add(0);			
		        hist2.Add(0);			
		        step+=bin;
	        }
	
	
	        for (int i=0; i<c.Count; i++) {
		
		
		        int index=bins.Count-1;
		        for (int j=0; j<bins.Count-1; j++) if(	(Math.Abs((double)(c[i])-bins[j])<1e-7) || (	(double)(c[i])>bins[j]	&&	(double)(c[i])<bins[j+1]	)	) { 
		        // this could be done in a more efficient way
			
			        index=j;
			        break;
		
		        }
		
		        //cout<<hist[index]<<" "<<index<<endl;
		
				
		        hist[index]++;
		        hist2[index]+=(double)(c[i]);
		
	        }
	
	
	
	
	        for (int i=0; i<hist.Count-1; i++) {
		
		        double h1= bins[i];
		        double h2= bins[i+1];
		
		
		        double x=hist2[i]/hist[i];
		        double y=(double)(hist[i])/(c.Count*(h2-h1));
		
		        if (Math.Abs(y)>1e-10)
			        TextDB.WriteTextFile(string.Format("{0}\t{1}\n",x,y),ostreamout);		
		
	
	
	        }
	
	
	
	        return 0;

        }
        void int_histogram (List<int> c, string ostreamout) {

	
	
	        Dictionary<int, double> hist=new Dictionary<int,double>();
	
	        double freq=1/(double)(c.Count);
	
	        for (int i=0; i<c.Count; i++) {
		
                //map<int, double>::iterator itf=hist.find(c[i]);
                //if (itf==hist.end())
                //    hist.insert(make_pair(c[i], 1.));
                //else
                //    itf->second++;
                
	             if(!hist.ContainsKey(c[i]))
                    hist.Add(c[i],1);
		        else
                    hist[c[i]]++;
	        }
	
	
	        //for (Dictionary<int, double>.Enumerator it=hist.GetEnumerator(); it.MoveNext();)
            for(int i=0;i<hist.Keys.Count;i++)
		        hist[hist.Keys.ElementAt(i)]=hist[hist.Keys.ElementAt(i)]*freq;
	
	        prints(hist, ostreamout);

	

        }
        void prints(Dictionary <int, double> sq,  string ostreamout) 
        {

	        Dictionary<int, double>.Enumerator it = sq.GetEnumerator();
	        while(it.MoveNext()) { 
		        TextDB.WriteTextFile(string.Format("{0}\t{1}\n",it.Current.Key,it.Current.Value),ostreamout);
	        } 
            TextDB.WriteTextFile("\n",ostreamout);
	
        }

        
    int not_norm_histogram (List<double> c, string ostreamout, int number_of_bins, double b1, double b2) 
        {		

	            // this should be OK
	            // c is the set of data, b1 is the lower bound, b2 is the upper one (if they are equal, default limits are used)
	
	
	
	            double min=(double)(c[0]);
	            double max=(double)(c[0]);
	
	            for (int i=0; i<c.Count; i++) {
		
		            if (min>(double)(c[i]))
			            min=(double)(c[i]);
		
		            if (max<(double)(c[i]))
			            max=(double)(c[i]);
		
	            }
	
	
	
	            min-=1e-6;
	            max+=1e-6;
	
	
	
	            if (b1!=b2) {
		
		            min=b1;
		            max=b2;
	
	            }
		
	            if (max==min)
		            max+=1e-3;
	
	
	
	            List <int> hist=new List<int>();
	            List <double> hist2=new List<double>();
		
	            double step=min;
	            double bin=(max-min)/number_of_bins;		// bin width

	            while (step<=max+2*bin) {
	
		            hist.Add(0);			
		            hist2.Add(0);			
		            step+=bin;
	            }
	

	
		
	
	
	            for (int i=0; i<c.Count; i++) {
		
		
		
		            double data=(double)(c[i]);
		
		            if (data>min && data<=max) {
			
			            int index=(int)((data-min)/bin);		
			
				
			            hist[index]++;
			            hist2[index]+=(double)(c[i]);
		
		            }
		
	            }
	
	
	            for (int i=0; i<hist.Count-1; i++) {
		
		
		
				
		            double x=hist2[i]/hist[i];
		            double y=(double)(hist[i])/(c.Count);
		
		            if (Math.Abs(y)>1e-10)
                        TextDB.WriteTextFile(string.Format("{0}\t{1}\n",x,y),ostreamout);
		
	
	            }
	
	
	
			
	            return 0;

            }

        int print_network(List<HashSet<int> > Ein, List<HashSet<int> > Eout, List<List<int> > member_list, List<List<int> > member_matrix, List<int> num_seq, string Network, string Community, string Analysis) 
        {

	
            int edges=0;

		
            int num_nodes=member_list.Count;
	
            List<double> double_mixing_in=new List<double>();
            for (int i=0; i<Ein.Count; i++) if(Ein[i].Count!=0) {
		
	            double one_minus_mu = (double)(internal_kin(Ein, member_list, i))/Ein[i].Count;
		
	            double_mixing_in.Add(Math.Abs(1.0- one_minus_mu));
	            edges+=Ein[i].Count;
		
            }
	
            List<double> double_mixing_out=new List<double>();
            for (int i=0; i<Eout.Count; i++) if(Eout[i].Count!=0) {
		
	            double one_minus_mu = (double)(internal_kin(Eout, member_list, i))/Eout[i].Count;		
	
	            double_mixing_out.Add(Math.Abs(1.0- one_minus_mu));
		
		
            }

	
	
            //cout<<"\n----------------------------------------------------------"<<endl;
            //cout<<endl;
	
	
            double density=0; 
            double sparsity=0;
	
            for (int i=0; i<member_matrix.Count; i++) {

	            double media_int=0;
	            double media_est=0;
		
	            for (int j=0; j<member_matrix[i].Count; j++) {
			
			
		            double kinj = (double)(internal_kin_only_one(Ein[member_matrix[i][j]], member_matrix[i]));
		            media_int+= kinj;
		            media_est+=Ein[member_matrix[i][j]].Count - (double)(internal_kin_only_one(Ein[member_matrix[i][j]], member_matrix[i]));
					
	            }
		
	            double pair_num=(member_matrix[i].Count*(member_matrix[i].Count-1));
	            double pair_num_e=((num_nodes-member_matrix[i].Count)*(member_matrix[i].Count));
		
	            if(pair_num!=0)
		            density+=media_int/pair_num;
	            if(pair_num_e!=0)
		            sparsity+=media_est/pair_num_e;
		
		
	
            }
	
            density=density/member_matrix.Count;
            sparsity=sparsity/member_matrix.Count;
	
	
	


            //ofstream out1("network.dat");
            string out1 = Network;//"network.txt";
            for (int u=0; u<Eout.Count; u++) {

	            HashSet<int>.Enumerator itb=Eout[u].GetEnumerator();
	
	            while (itb.MoveNext())
                    TextDB.WriteTextFile(string.Format("{0}\t{1}\n",u+1,itb.Current+1),out1);
		            //out1<<u+1<<"\t"<<*(itb++)+1<<endl;
		

            }
            User.One.MessageToUser("Network was saved in file: " + Network);

	
            //ofstream out2("community.dat");
            string out2 = Community;//"community.txt";
            string buffer = "";
            for (int i=0; i<member_list.Count; i++) {
		
	            
                buffer += string.Format("{0}\t", i + 1);
                //TextDB.WriteTextFile(string.Format("{0}\t",i+1),out2);
                for (int j = 0; j < member_list[i].Count; j++)
                    buffer += string.Format("{0}\t", member_list[i][j] + 1);
                    //TextDB.WriteTextFile(string.Format("{0}\t",member_list[i][j]+1),out2);
		        
                //TextDB.WriteTextFile("\n",out2);
                TextDB.WriteTextFile(buffer, out2);
                buffer = "";
	
            }
            User.One.MessageToUser("Communities/modules were saved in file: " + Community);
            User.One.MessageToUser("\n\n---------------------------------------------------------------------------\n");
	
	
            User.One.MessageToUser("network of "+num_nodes.ToString()+" vertices and "+edges.ToString()+" edges"+";\t average degree = "+((double)edges/num_nodes).ToString()+"\n");
            User.One.MessageToUser("\naverage mixing parameter (in-links): "+average_func(double_mixing_in).ToString()+" +/- "+Math.Sqrt(variance_func(double_mixing_in)).ToString()+"\n");
            User.One.MessageToUser("average mixing parameter (out-links): "+average_func(double_mixing_out).ToString()+" +/- "+Math.Sqrt(variance_func(double_mixing_out)).ToString()+"\n");
            User.One.MessageToUser("p_in: "+density.ToString()+"\tp_out: "+sparsity.ToString()+"\n");



            string statout = Analysis;//"statistics.txt";
	
            List<int> degree_seq_in=new List<int>();
            for (int i=0; i<Ein.Count; i++)
	            degree_seq_in.Add(Ein[i].Count);
	
            List<int> degree_seq_out=new List<int>();
            for (int i=0; i<Eout.Count; i++)
            degree_seq_out.Add(Eout[i].Count);

            TextDB.WriteTextFile("in-degree distribution (probability density function of the degree in logarithmic bins) \n",statout);
            log_histogram(degree_seq_in, statout, 10);
            TextDB.WriteTextFile("\nin-degree distribution (degree-occurrences) \n",statout);
            int_histogram(degree_seq_in, statout);
            TextDB.WriteTextFile("\n--------------------------------------\n",statout);


            TextDB.WriteTextFile("out-degree distribution (probability density function of the degree in logarithmic bins) \n", statout);
            log_histogram(degree_seq_out, statout, 10);
            TextDB.WriteTextFile("\nout-degree distribution (degree-occurrences) \n", statout);
            int_histogram(degree_seq_out, statout);
            TextDB.WriteTextFile("\n--------------------------------------\n", statout);

            TextDB.WriteTextFile("community distribution (size-occurrences)\n", statout);
            int_histogram(num_seq, statout);
            TextDB.WriteTextFile("\n--------------------------------------\n", statout);

            TextDB.WriteTextFile("mixing parameter (in-links)\n", statout);
            not_norm_histogram(double_mixing_in, statout, 20, 0, 0);
            TextDB.WriteTextFile("\n--------------------------------------\n", statout);

            TextDB.WriteTextFile("mixing parameter (out-links)\n", statout);
            not_norm_histogram(double_mixing_out, statout, 20, 0, 0);
            TextDB.WriteTextFile("\n--------------------------------------\n", statout);




            TextDB.WriteTextFile("\n\n", statout);
            User.One.MessageToUser("Statistics were saved in file: " + Analysis);

            return 0;

            }
        /// <summary>
        /// Create basicnetwork from benchmark graph
        /// </summary>
        /// <param name="Ein">List of nodes with their in-links</param>
        /// <param name="Eout">List of nodes with their out-links</param>
        /// <param name="member_list">List of nodes with their cluster ID</param>
        /// <param name="template">The template of network to create the returning network</param>
        /// <param name="Community">Output community composed of a list of nodes' ID  and the community IDs they belong to</param>
        /// <returns></returns>
        BasicNetwork export_network(List<HashSet<int>> Ein, List<HashSet<int>> Eout, List<List<int>> member_list, BasicNetwork template, ref Dictionary<int, List<int>> Community)
        {

            //Create network
            BasicNetwork Net = template.CreateObject() as BasicNetwork;
            for (int u = 0; u < Eout.Count; u++)
            {

                HashSet<int>.Enumerator itb = Eout[u].GetEnumerator();

                while (itb.MoveNext())
                {
                   
                    Net.AddNodeAndArc(new Interaction(Net.NewNode((u+1).ToString(),null), Net.NewNode((itb.Current+1).ToString(),null),Interaction.ArbitraryValue));
                }
            }
            //Create community
            Community = new Dictionary<int, List<int>>();
            for (int i = 0; i < member_list.Count; i++)
            {

                Community[i + 1] = new List<int>();
                for (int j = 0; j < member_list[i].Count; j++)
                    Community[i + 1].Add(member_list[i][j] + 1);

            }
            return Net;
        }
        /// <summary>
        /// Generate benchmark network to test modularity
        /// </summary>
        /// <param name="excess"></param>
        /// <param name="defect"></param>
        /// <param name="num_nodes">The number of nodes</param>
        /// <param name="average_k">Average of in-degree</param>
        /// <param name="max_degree">Maximum in-degree</param>
        /// <param name="tau">minus exponent for the degree sequence</param>
        /// <param name="tau2">minus exponent for the community size distribution</param>
        /// <param name="mixing_parameter">mixing paparmeter (m).
        /// The fraction of m of links connecting with other diffent-community nodes</param>
        /// <param name="overlapping_nodes">number of overlapping nodes</param>
        /// <param name="overlap_membership">number of memberships of the overlapping nodes</param>
        /// <param name="nmin">minimum for the community sizes</param>
        /// <param name="nmax">maximum for the community sizes</param>
        /// <param name="fixed_range"></param>
        /// <returns></returns>
       public int benchmark(bool excess, bool defect, int num_nodes, double  average_k, int  max_degree, double  tau, double  tau2, 
	        double  mixing_parameter, int  overlapping_nodes, int  overlap_membership, int  nmin, int  nmax, bool  fixed_range, string Network, string Community, string Analaysis) 
       {	
	
	        // it finds the minimum degree -----------------------------------------------------------------------

	        double dmin=solve_dmin(max_degree, average_k, -tau);
	        if (dmin==-1)
		        return -1;
	
	        int min_degree=(int)dmin;
	
	
	        double media1=integer_average(max_degree, min_degree, tau);
	        double media2=integer_average(max_degree, min_degree+1, tau);
	
	        if (Math.Abs(media1-average_k)>Math.Abs(media2-average_k))
		        min_degree++;
		
	        // range for the community sizes
	        if (!fixed_range) 
            {
		        nmax=max_degree;
		        nmin=Math.Max((int)min_degree, 3);
		        User.One.MessageToUser("-----------------------------------------------------------\n");
		        User.One.MessageToUser("community size range automatically set equal to ["+nmin.ToString()+" , "+nmax.ToString()+"]\n");
	        }
	
	
	        //----------------------------------------------------------------------------------------------------
	
	
	        List<int> degree_seq_in=new List<int>();		//  degree sequence of the nodes (in-links)
	        List <int> degree_seq_out=new List<int>();		//  degree sequence of the nodes (out-links)
	        List <double> cumulative=new List<double>();
           
	        powerlaw(max_degree, min_degree, tau, cumulative);
	
	        for (int i=0; i<num_nodes; i++) {
		
		        //int nn=lower_bound(cumulative.begin(), cumulative.end(), ran4())-cumulative.begin()+min_degree;
                int nn = Netutil.lower_bound<double>(cumulative, NumericMath.RandomCraft.NextDouble());//cumulative.IndexOf((from x in cumulative where x >= NumericMath.RandomCraft.NextDouble() select x).FirstOrDefault()) +min_degree;
		        degree_seq_in.Add(nn);
	
	        }
	        degree_seq_in.Sort();
	
	        //sort(degree_seq_in.begin(), degree_seq_in.end());
		
	        int inarcs=deque_int_sum(degree_seq_in);
	        compute_internal_degree_per_node(inarcs, degree_seq_in.Count, degree_seq_out);
	
	
		
	
	        List<List<int> >  member_matrix=new List<List<int>>();
	        List<int> num_seq=new List<int>();
	        List<int> internal_degree_seq_in=new List<int>();
	        List<int> internal_degree_seq_out=new List<int>();
	
	// ********************************			internal_degree and membership			***************************************************

	
	

	if(internal_degree_and_membership(mixing_parameter, overlapping_nodes, overlap_membership, num_nodes, member_matrix, excess, defect, degree_seq_in, degree_seq_out, num_seq, internal_degree_seq_in, internal_degree_seq_out, fixed_range, nmin, nmax, tau2)==-1)
		return -1;
	
	
	
	List<HashSet<int> > Ein=new List<HashSet<int>>();				// Ein is the adjacency matrix written in form of list of edges (in-links)
    List<HashSet<int>> Eout=new List<HashSet<int>>();				// Eout is the adjacency matrix written in form of list of edges (out-links)
	List<List<int> > member_list=new List<List<int>>();		// row i cointains the memberships of node i
	List<List<int> > link_list_in=new List<List<int>>();	// row i cointains degree of the node i respect to member_list[i][j]; there is one more number that is the external degree (in-links)
	List<List<int> > link_list_out=new List<List<int>>();	// row i cointains degree of the node i respect to member_list[i][j]; there is one more number that is the external degree (out-links)

	
	
	//cout<<"building communities... "<<endl;
    User.One.MessageToUser("building communities... \n");
	if(build_subgraphs(Ein, Eout, member_matrix, member_list, link_list_in, link_list_out, internal_degree_seq_in, degree_seq_in, internal_degree_seq_out, degree_seq_out, excess, defect)==-1)
		return -1;	
	

	
	User.One.MessageToUser("connecting communities... \n");
	connect_all_the_parts(Ein, Eout, member_list, link_list_in, link_list_out);
	


	if(erase_links(Ein, Eout, member_list, excess, defect, mixing_parameter)==-1)
		return -1;
	
	
	User.One.MessageToUser("recording network...\n");	
	print_network(Ein, Eout, member_list, member_matrix, num_seq,Network,Community,Analaysis);

	//printmcs(Ein, Eout);
	//printmcs(Eout, Ein);
	
		
	return 0;
	
}
        /// <summary>
        /// Create benchmark for a random network class that is derived from BasicNetwork class
        /// </summary>
        /// <param name="excess"></param>
        /// <param name="defect"></param>
       /// <param name="num_nodes">The number of nodes</param>
       /// <param name="average_k">Average of in-degree</param>
       /// <param name="max_degree">Maximum in-degree</param>
       /// <param name="tau">minus exponent for the degree sequence (gamma)</param>
       /// <param name="tau2">minus exponent for the community size distribution (beta)</param>
       /// <param name="mixing_parameter">mixing paparmeter (m).
       /// The fraction of m of links connecting with other diffent-community nodes</param>
       /// <param name="overlapping_nodes">number of overlapping nodes</param>
       /// <param name="overlap_membership">number of memberships of the overlapping nodes</param>
       /// <param name="nmin">minimum for the community sizes</param>
       /// <param name="nmax">maximum for the community sizes</param>
       /// <param name="fixed_range"></param>
        /// <param name="template">Template of network to create the benchmark network</param>
        /// <param name="Community">Community of the benchmark as output</param>
        /// <returns>The random benchmark network</returns>
       public BasicNetwork benchmark(bool excess, bool defect, int num_nodes, double average_k, int max_degree, double tau, double tau2,
           double mixing_parameter, int overlapping_nodes, int overlap_membership, int nmin, int nmax, bool fixed_range, BasicNetwork template, ref Dictionary<int, List<int>> Community)
       {
           Community = null;

           double dmin = solve_dmin(max_degree, average_k, -tau);
           if (dmin == -1)
               return null;

           int min_degree = (int)dmin;


           double media1 = integer_average(max_degree, min_degree, tau);
           double media2 = integer_average(max_degree, min_degree + 1, tau);

           if (Math.Abs(media1 - average_k) > Math.Abs(media2 - average_k))
               min_degree++;

           // range for the community sizes
           if (!fixed_range)
           {
               nmax = max_degree;
               nmin = Math.Max((int)min_degree, 3);
           }


           //----------------------------------------------------------------------------------------------------


           List<int> degree_seq_in = new List<int>();		//  degree sequence of the nodes (in-links)
           List<int> degree_seq_out = new List<int>();		//  degree sequence of the nodes (out-links)
           List<double> cumulative = new List<double>();

           powerlaw(max_degree, min_degree, tau, cumulative);

           for (int i = 0; i < num_nodes; i++)
           {

               //int nn=lower_bound(cumulative.begin(), cumulative.end(), ran4())-cumulative.begin()+min_degree;
               int nn = Netutil.lower_bound<double>(cumulative, NumericMath.RandomCraft.NextDouble());//cumulative.IndexOf((from x in cumulative where x >= NumericMath.RandomCraft.NextDouble() select x).FirstOrDefault()) +min_degree;
               degree_seq_in.Add(nn);

           }
           degree_seq_in.Sort();

           //sort(degree_seq_in.begin(), degree_seq_in.end());

           int inarcs = deque_int_sum(degree_seq_in);
           compute_internal_degree_per_node(inarcs, degree_seq_in.Count, degree_seq_out);




           List<List<int>> member_matrix = new List<List<int>>();
           List<int> num_seq = new List<int>();
           List<int> internal_degree_seq_in = new List<int>();
           List<int> internal_degree_seq_out = new List<int>();

           // ********************************			internal_degree and membership			***************************************************




           if (internal_degree_and_membership(mixing_parameter, overlapping_nodes, overlap_membership, num_nodes, member_matrix, excess, defect, degree_seq_in, degree_seq_out, num_seq, internal_degree_seq_in, internal_degree_seq_out, fixed_range, nmin, nmax, tau2) == -1)
               return null;



           List<HashSet<int>> Ein = new List<HashSet<int>>();				// Ein is the adjacency matrix written in form of list of edges (in-links)
           List<HashSet<int>> Eout = new List<HashSet<int>>();				// Eout is the adjacency matrix written in form of list of edges (out-links)
           List<List<int>> member_list = new List<List<int>>();		// row i cointains the memberships of node i
           List<List<int>> link_list_in = new List<List<int>>();	// row i cointains degree of the node i respect to member_list[i][j]; there is one more number that is the external degree (in-links)
           List<List<int>> link_list_out = new List<List<int>>();	// row i cointains degree of the node i respect to member_list[i][j]; there is one more number that is the external degree (out-links)

           if (build_subgraphs(Ein, Eout, member_matrix, member_list, link_list_in, link_list_out, internal_degree_seq_in, degree_seq_in, internal_degree_seq_out, degree_seq_out, excess, defect) == -1)
               return null;



           
           connect_all_the_parts(Ein, Eout, member_list, link_list_in, link_list_out);



           if (erase_links(Ein, Eout, member_list, excess, defect, mixing_parameter) == -1)
               return null;


           
           return export_network(Ein, Eout, member_list, template, ref Community);

       }

        #endregion

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetSimulation;
using Mathutil;
using System.Diagnostics;
using NetSimulation.Lib;
//using Algorithms.ShortestPath;
using MathNet.Numerics.LinearAlgebra;

namespace BasicNet
{
    /// <summary>
    /// Create Directed Random Graphs with Given Degree Distributions
    /// Note: the random directed graph returned can be an unconnected graph
    /// Source code: http://www2.warwick.ac.uk/fac/cross_fac/complexity/people/staff/delgenio/digsamp/
    /// Paper: http://iopscience.iop.org/1367-2630/14/2/023012/pdf/1367-2630_14_2_023012.pdf
    /// </summary>
    public class DiNetConfigureModel
    {
        struct BDS
        {
            public int []indegree;
            public int []outdegree;
            public int []label;
            public int []forbidden;
        };
        //BDS bds;
        public struct Digraph
        {
	        public int [][]list;
	        public double weight;
        };
        /// <summary>
        /// Initialize directed network generator, the sequence have to be in Lexicographical order
        /// </summary>
        /// <param name="seq">A sequence of in- and out- degree where in-degree is the seq(i,0) and out-degree is seq(i,1), with i is zero-based nodeID</param>
        //public DiNetConfigureModel(int[,] seq)
        //{
        //    digsaminit(seq, seq.GetLength(0));
        //}
        public DiNetConfigureModel(IEnumerable<Pair<int, int>> seq)
        {

            //digsaminit(SortDegreeSequence(seq));
            digsaminit(seq);
        }
        public static IEnumerable<Pair<int, int>> SortDegreeSequence(IEnumerable<Pair<int, int>> seq)
        {
            return from p in seq orderby p.First descending, p.Second descending select p;
        }
        public static List<Pair<int, int>> ConvertToDegSequenceList(int[,] seq)
        {
            List<Pair<int, int>> result = new List<Pair<int, int>>();
            for (int i = 0; i < seq.GetLength(0); i++)
                result.Add(new Pair<int, int>(seq[i, 0], seq[i, 1]));
            return result;
        }
        //Digraph digraph;
        int len;
        int []allowed, G, S;
        BDS orig, indseq, newbds, auxbds;
        Digraph sample;
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="seq">In-Out-degree array where in-degree is index of 0, outdegree is index of 1;
        /// NodeID is the order of the degree distribution array</param>
        /// <param name="n">The number of degree distribution array</param>
        private void digsaminit(int [,]seq, int n)
        {
	        int i, j;
	        long outstubs = 0;
	
	        /* Memory allocations */
	        orig.indegree    = new int[n];						// Original sequence in-degree
	        orig.outdegree   = new int[n];						// Original sequence out-degree
	        orig.label       = new int[n];						// Original node labels
	        orig.forbidden   = new int[n];						// Original forbidden status labels
	        indseq.indegree  = new int[n];						// Work sequence in-degree
	        indseq.outdegree = new int[n];						// Work sequence out-degree
	        indseq.label     = new int[n];						// Work sequence labels
	        indseq.forbidden = new int[n];						// Work sequence forbidden status labels
	        newbds.indegree  = new int[n];						// Derivative bi-degree sequence
	        newbds.outdegree = new int[n];
	        newbds.label     = new int[n];
	        newbds.forbidden = new int[n];
	        auxbds.indegree  = new int[n];						// Auxiliary bi-degree sequence
	        auxbds.outdegree = new int[n];
	        auxbds.label     = new int[n];
	        auxbds.forbidden = new int[n];
	        allowed          = new int[n];						// Allowed nodes
	        G                = new int[n+1];
	        S                = new int[n+1];
	
	        /* Initializations of sequences */
	        for (i=0; i<n; i++) {
		        orig.indegree[i]  = seq[i,0];
		        orig.outdegree[i] = seq[i,1];
		        orig.label[i]     = i;
		        orig.forbidden[i] = 0;
		        outstubs         += seq[i,1];
	        }
	
	        /* Initialization of sample */
            sample.list=new  int[n][];
            //for(i=0;i<n;i++)
            //    for(j=0;j<orig.outdegree[i];j++)
            //        sample.list[i,j]=-1;

            //sample.list = malloc(n * sizeof(int*));						// Adjacency list
            //sample.list[0] = new int[outstubs];
            //for (j = 0; j < orig.outdegree[0]; j++) sample.list[0][j] = -1;
           
            for (i = 0; i < n; i++)
            {
                sample.list[i] = new int[orig.outdegree[i]];
                for (j = 0; j < orig.outdegree[i]; j++) sample.list[i][j] = -1;
            }
	        len = n;
	        
        }
        private void digsaminit(IEnumerable<Pair<int, int>> seq)
        {
            //Debug.WriteLine("Begin Sequence!");
            //foreach (var p in seq)
            //{
            //    Debug.WriteLine(string.Format("{0}\t{1}", p.First, p.Second));
            //}
            //Debug.WriteLine("End Sequence!");
            int n = seq.Count();
            int i, j;
            long outstubs = 0;

            /* Memory allocations */
            orig.indegree = new int[n];						// Original sequence in-degree
            orig.outdegree = new int[n];						// Original sequence out-degree
            orig.label = new int[n];						// Original node labels
            orig.forbidden = new int[n];						// Original forbidden status labels
            indseq.indegree = new int[n];						// Work sequence in-degree
            indseq.outdegree = new int[n];						// Work sequence out-degree
            indseq.label = new int[n];						// Work sequence labels
            indseq.forbidden = new int[n];						// Work sequence forbidden status labels
            newbds.indegree = new int[n];						// Derivative bi-degree sequence
            newbds.outdegree = new int[n];
            newbds.label = new int[n];
            newbds.forbidden = new int[n];
            auxbds.indegree = new int[n];						// Auxiliary bi-degree sequence
            auxbds.outdegree = new int[n];
            auxbds.label = new int[n];
            auxbds.forbidden = new int[n];
            allowed = new int[n];						// Allowed nodes
            G = new int[n + 1];
            S = new int[n + 1];

            /* Initializations of sequences */
            for (i = 0; i < n; i++)
            {
                orig.indegree[i] = seq.ElementAt(i).First;
                orig.outdegree[i] = seq.ElementAt(i).Second;
                orig.label[i] = i;
                orig.forbidden[i] = 0;
                outstubs += seq.ElementAt(i).Second;
            }

            /* Initialization of sample */
            sample.list = new int[n][];
            //for(i=0;i<n;i++)
            //    for(j=0;j<orig.outdegree[i];j++)
            //        sample.list[i,j]=-1;

            //sample.list = malloc(n * sizeof(int*));						// Adjacency list
            //sample.list[0] = new int[outstubs];
            //for (j = 0; j < orig.outdegree[0]; j++) sample.list[0][j] = -1;

            for (i = 0; i < n; i++)
            {
                sample.list[i] = new int[orig.outdegree[i]];
                for (j = 0; j < orig.outdegree[i]; j++) sample.list[i][j] = -1;
            }
            len = n;

        }





/* Digraph sampler */
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rng">Random object to generate random number in the range of [0, 1].
        ///  The generator must be already seeded</param>
        /// <param name="stfl"> a flag governing the way target nodes are chosen for connection: if set to 0, the nodes are chosen randomly amongst those allowed; 
        /// if set to anything but 0, the nodes are chosen with a probability proportional to their residual in-degree. </param>
        /// <returns>G.list is a densely allocated matroid containing the out-adjacency list, 
        /// and G.weight is the logarithm of the weight associated with that particular sample.</returns>
        public Digraph digsam(Mathutil.NumericMath.RandomCraft rng, int stfl)
        {
	        int i;
	
	        for (i=0; i<len; i++) {
		        indseq.indegree[i] = orig.indegree[i];
		        indseq.outdegree[i] = orig.outdegree[i];
		        indseq.label[i] = i;
		        indseq.forbidden[i] = 0;
	        }
	        sample.weight=0.0;								// Sample weight
	
	        dirbuild(rng,stfl);								// Build the digraph
	
	        return sample;
        }
        /// <summary>
        /// Create a random directed network with in-out- degree distribution in the initialized list
        /// Can call multiple to create various random networks
        /// </summary>
        /// <param name="template">The template of network to create</param>
        /// <param name="rng">Random object to generate random number in the range of [0, 1].
        ///  The generator must be already seeded</param>
        /// <param name="stfl">a flag governing the way target nodes are chosen for connection: if set to 0, the nodes are chosen randomly amongst those allowed; 
        /// if set to anything but 0, the nodes are chosen with a probability proportional to their residual in-degree. </param>
        /// <returns>Random directed network</returns>
        public BasicNetwork CreateNetwork(BasicNetwork template, Mathutil.NumericMath.RandomCraft rng, int stfl = 0)
        {
            BasicNetwork Net=template.CreateObject() as BasicNetwork;
            Digraph G= digsam(rng, stfl);
          
            for(int i=0;i<len;i++)//NodeIDs
            {
               
                for (int j = 0; j < orig.outdegree[i]; j++)
                {
                    Net.AddNodeAndArc(new Interaction(Net.NewNode(i.ToString(),null),Net.NewNode(G.list[i][j].ToString(),null),Interaction.ArbitraryValue));
                }
            }

            return Net;
        }

        void lexisort(ref int w, int t)
        {
            int first, second, index=0, cpindex=0, saveinf, saveoutf, savelabf, saveins, saveouts, savelabs, labw;
	
	        if (indseq.indegree[w]>indseq.indegree[t]) {
		        first  = w;
		        second = t;
	        } else if (indseq.indegree[w]==indseq.indegree[t] && indseq.outdegree[w]>indseq.outdegree[t]) {
		        first  = w;
		        second = t;
	        } else {
		        first  = t;
		        second = w;
	        }
	
	        saveinf  = indseq.indegree[first];
	        saveoutf = indseq.outdegree[first];
	        savelabf = indseq.label[first];
	        saveins  = indseq.indegree[second];
	        saveouts = indseq.outdegree[second];
	        savelabs = indseq.label[second];
            labw = indseq.label[w];
	
	        while (index<len) {
		        if (index != w && index != t) {
			        auxbds.indegree[cpindex]  = indseq.indegree[index];
			        auxbds.outdegree[cpindex] = indseq.outdegree[index];
			        auxbds.label[cpindex]     = indseq.label[index];
			        auxbds.forbidden[cpindex] = indseq.forbidden[index];
			        cpindex++;
		        }
	        index++;
	        }
	
	        index = 0;
	        cpindex = 0;
	
	        while (index<len-2 && ( auxbds.indegree[index]>saveinf || (auxbds.indegree[index]==saveinf && auxbds.outdegree[index]>saveoutf))) {
		        indseq.indegree[cpindex]  = auxbds.indegree[index];
		        indseq.outdegree[cpindex] = auxbds.outdegree[index];
		        indseq.label[cpindex]     = auxbds.label[index];
		        indseq.forbidden[cpindex] = auxbds.forbidden[index];
		        cpindex++;
		        index++;
	        }
	        indseq.indegree[cpindex]  = saveinf;
	        indseq.outdegree[cpindex] = saveoutf;
	        indseq.label[cpindex]     = savelabf;
	        indseq.forbidden[cpindex] = 1;
            if (savelabf==labw) w=cpindex;
   	        cpindex++;
	        while (index<len-2 && ( auxbds.indegree[index]>saveins || (auxbds.indegree[index]==saveins && auxbds.outdegree[index]>saveouts))) {
		        indseq.indegree[cpindex]  = auxbds.indegree[index];
		        indseq.outdegree[cpindex] = auxbds.outdegree[index];
		        indseq.label[cpindex]     = auxbds.label[index];
		        indseq.forbidden[cpindex] = auxbds.forbidden[index];
		        cpindex++;
		        index++;
	        }
	        indseq.indegree[cpindex]  = saveins;
	        indseq.outdegree[cpindex] = saveouts;
	        indseq.label[cpindex]     = savelabs;
	        indseq.forbidden[cpindex] = 1;
            if (savelabs==labw) w=cpindex;
   	        cpindex++;
	        while (index<len-2) {
		        indseq.indegree[cpindex]  = auxbds.indegree[index];
		        indseq.outdegree[cpindex] = auxbds.outdegree[index];
		        indseq.label[cpindex]     = auxbds.label[index];
		        indseq.forbidden[cpindex] = auxbds.forbidden[index];
		        cpindex++;
		        index++;
	        }
	
	        return;
        }


/* Recursively places all the possible links in the graph */
        void dirbuild(Mathutil.NumericMath.RandomCraft rng, int stfl)
{
	int work, target, alll, i;
    long ext, count;
    long restubs;
    //try
    //{
        work = 0;																													// Find the first work node
        while (work < len && indseq.outdegree[work] == 0) work++;

        while (work < len)
        {																											// If there are still nodes with out-stubs
            indseq.forbidden[work] = 1;																								// Mark the work node as forbidden
            while (indseq.outdegree[work] != 0)
            {																						// If the work node still has out-stubs
                alll = 0;																												// Reset the size of the allowed nodes set
                restubs = 0;																											// Reset the count of the stubs in the allowed set
                dirallow(ref work, ref alll, ref restubs);																						// Build the set of allowed nodes
                if (stfl != 0)
                {																											// If we choose proportionally to the residual in-degree
                    i = -1;																											// then reset the selector,
                    ext = (long)(restubs * rng.GetUniDev());																							// extract an in-stub,
                    count = 0;																										// reset the counter,
                    do
                    {
                        i++;
                        count += indseq.indegree[allowed[i]];																		// and count the in-stubs
                    } while (count <= ext);																							// until the correct in-stub is found,
                    target = allowed[i];																							// then identify the target node
                    sample.weight += Math.Log(restubs) - Math.Log(indseq.indegree[target]);													// and update the sample weight accordingly.
                }
                else
                {																											// If instead we are choosing uniformly on the allowed nodes,
                    sample.weight += Math.Log(alll);																						// update the weight accordingly,
                    target = allowed[(int) (alll*rng.GetUniDev())];																	// and extract a random allowed node.
                    //target = allowed[rng.GetUniDevInt(0, alll + 1)];//
                }
                sample.list[indseq.label[work]][ orig.outdegree[indseq.label[work]] - indseq.outdegree[work]] = indseq.label[target];	// Connect the work node to the target
                indseq.outdegree[work]--;																							// Reduce the outdegree of the work node
                indseq.indegree[target]--;																	 						// Reduce the indegree of the target node
                lexisort(ref work, target);																								// Reorder the sequence if needed.
            }
            for (i = 0; i < len; i++) indseq.forbidden[i] = 0;																			// Reset the forbidden flags
            work = 0;																												// Find the new work node
            while (work < len && indseq.outdegree[work] == 0) work++;
        }
    //}
    //catch (Exception ex)
    //{
    //    throw ex;

    //}
	
	return;
}



        /* Builds the set of the nodes which we are allowed to connect to */
        void dirallow(ref int work, ref int alll, ref long restubs)
        {
	        int index=0, count=0, curdeg=-1, firstdeg=0, dumdeg, check, degchk, ind2, oripos, moven=0, tarpos, t, wlab, nw, wstin, wstfb=0;
	        long lhs, rhs, gtilde;
	
	        /* Start making a copy of the sequence.
	           Add the leftmost adjacency set to the allowed node set.
	           Also decrease the in-degrees of the nodes in the copy sequence
	           which are added to the set and make them forbidden */
	        wlab = indseq.label[work];
            while (index < len&& count < indseq.outdegree[work])
            {
		        newbds.indegree[index]  = indseq.indegree[index];
		        newbds.outdegree[index] = indseq.outdegree[index];
		        newbds.label[index]     = indseq.label[index];
		        newbds.forbidden[index] = 1;
		        if (indseq.forbidden[index]==0 && indseq.indegree[index]!=0) {
			        newbds.indegree[index]--;
			        alll++;
			        //*(allowed+(*alll)-1) = index;
                    allowed[ alll - 1] = index;
			        restubs += indseq.indegree[index];
			        count++;
			        if (curdeg!=newbds.indegree[index]) {
				        curdeg = newbds.indegree[index];
				        firstdeg = index;
			        }
		        } else {
			        if (index!=0) {
				        if (newbds.indegree[index]>newbds.indegree[index-1]) {
					        dumdeg = newbds.indegree[firstdeg];
					        newbds.indegree[firstdeg] = newbds.indegree[index];
					        newbds.indegree[index] = dumdeg;
					        dumdeg = newbds.outdegree[firstdeg];
					        newbds.outdegree[firstdeg] = newbds.outdegree[index];
					        newbds.outdegree[index] = dumdeg;
					        dumdeg = newbds.label[firstdeg];
					        newbds.label[firstdeg] = newbds.label[index];
					        newbds.label[index] = dumdeg;
					        firstdeg++;
				        }
			        } else {
				        firstdeg = 0;
				        curdeg = newbds.indegree[0];
			        }
		        }
		        index++;
	        }
	
	        /* In the copy sequence, revert the in-degree of the last node added to the set. */
           
           check = allowed[alll - 1]; //(*(allowed+(*alll)-1));
           
	        newbds.indegree[check]++;
	        if (check!=0) degchk = newbds.indegree[check-1];
	        else degchk = -1;
	        oripos = check;
	
	        if (index<len) {																			// If there are more nodes left
		        for (ind2=index; ind2<len; ind2++) {													// Finish copying the sequence
			        newbds.indegree[ind2]  = indseq.indegree[ind2];
			        newbds.outdegree[ind2] = indseq.outdegree[ind2];
			        newbds.label[ind2]     = indseq.label[ind2];
			        newbds.forbidden[ind2] = indseq.forbidden[ind2];
		        }
		
		        nw = 0;
		        while (newbds.label[nw]!=wlab) nw++;
		        if (nw>=oripos) {
			        wstin = newbds.indegree[nw];
			        wstfb = newbds.forbidden[nw];
			        while (nw<len-1 && newbds.indegree[nw+1]==wstin && newbds.outdegree[nw+1] !=0) {
				        newbds.indegree[nw]  = newbds.indegree[nw+1];
				        newbds.outdegree[nw] = newbds.outdegree[nw+1];
				        newbds.label[nw]     = newbds.label[nw+1];
				        newbds.forbidden[nw] = newbds.forbidden[nw+1];
				        nw++;
			        }
		        }
		        newbds.label[nw] = wlab;
		        newbds.forbidden[nw] = wstfb;
		        newbds.outdegree[nw] = 1;																// Set to 1 the out-degree of the work node
		
		        /* If needed, quickly reorder the sequence */
		        if (degchk!=-1) {
			        while (check<len && degchk<newbds.indegree[check]) {
				        auxbds.indegree[moven]  = newbds.indegree[check];
				        auxbds.outdegree[moven] = newbds.outdegree[check];
				        auxbds.label[moven]     = newbds.label[check];
				        auxbds.forbidden[moven] = newbds.forbidden[check];
				        moven++;
				        check++;
			        }
			        if (moven>0) {
				        degchk = newbds.indegree[oripos];
				        tarpos = oripos-1;
				        while (tarpos>=0 && degchk>newbds.indegree[tarpos]) tarpos--;
				        tarpos++;
				        for (t=oripos-1; t>=tarpos; t--) {
					        newbds.indegree[t+moven]  = newbds.indegree[t];
					        newbds.outdegree[t+moven] = newbds.outdegree[t];
					        newbds.label[t+moven]     = newbds.label[t];
					        newbds.forbidden[t+moven] = newbds.forbidden[t];
				        }
				        for (t=0; t<moven; t++) {
					        newbds.indegree[tarpos+t]  = auxbds.indegree[t];
					        newbds.outdegree[tarpos+t] = auxbds.outdegree[t];
					        newbds.label[tarpos+t]     = auxbds.label[t];
					        newbds.forbidden[tarpos+t] = auxbds.forbidden[t];
				        }
			        }
		        }
		
		        /* Build G and S for our digraphicality test */
		        for (t=0; t<=len; t++) S[t] = G[t] = 0;
		        G[newbds.outdegree[0]+1] = 1;
		        for (t=1; t<len; t++) {
			        G[newbds.outdegree[t]]++;
			        if (newbds.outdegree[t] >= t+1)  S[newbds.outdegree[t]]--;
			        if (newbds.outdegree[t]+1 > t+1) S[newbds.outdegree[t]+1]++;
		        }
		
		        /* Find the last good node */
		        lhs = newbds.indegree[0];
		        rhs = len-1-G[0];
		        gtilde = G[0]+G[1];
		        t = 1;
		        if (nw==0) {
			        t++;
			        lhs += newbds.indegree[t-1];
			        rhs += len-gtilde;
			        if (newbds.outdegree[t-1]>=t) rhs--;
			        gtilde += G[t]+S[t];
		        }
		
		        while (t<len && lhs!=rhs) {
			        t++;
			        lhs += newbds.indegree[t-1];
			        rhs += len-gtilde;
			        if (newbds.outdegree[t-1]>=t) rhs--;
			        gtilde += G[t]+S[t];
		        }
		
		        if (lhs!=rhs) t = -1;		// No fail node
		        else if (t==len) t = -1;
		        else {
			        while (t<len && newbds.forbidden[t]!=0) t++;
			        if (t==len) t = -1;
		        }
		        if (t!=-1) t = newbds.label[t];
		
		        /* Add to the allowed set all the non-forbidden nodes
		           to the left of the fail node */
		        index = oripos+1;
		        while (index<len && indseq.label[index]!=t) {
			        if ( indseq.forbidden[index]==0 && indseq.indegree[index]!=0) {
				        alll++;
                        allowed[ alll - 1] = index;//*(allowed+(*alll)-1) = index;
				        restubs += indseq.indegree[index];
			        }
			        index++;
		        }
	        }
	
	        return;
        }
       
        /// <summary>
        /// Generates and returns a graphical power-law distributed sequence with the given number of nodes and power-law exponent gamma
        /// http://www2.warwick.ac.uk/fac/cross_fac/complexity/people/staff/delgenio/seqgen/
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="gam">Exponent coeeficient of power law function. It should be greater than zero</param>
        /// <param name="rng"></param>
        /// <returns></returns>
        public static int[] plseqgen(int nodes, double gam, NumericMath.RandomCraft rng)
        {
            int i, n;
            int[] seq = new int[nodes];
            int[] xk = new int[nodes];
	        double logsum, logsumtmp, logprob, logcumultmp, logcumul;
	
            //xk  = calloc(nodes,sizeof(int));
            //seq = malloc(nodes*sizeof(int));
	
	        logsum = 0.0;
	        for (i=2; i<nodes; i++) {
		        logsumtmp = -gam*Math.Log(i);
		        logsum = Math.Max(logsum,logsumtmp) + Math.Log(1+Math.Exp(-Math.Abs(logsum-logsumtmp)));
	        }
	
	        do {
		        for (n=0; n<nodes; n++) {
			        logprob = Math.Log(1+rng.GetUniDev());
			        logcumul = 0.0;
			        i = 0;
			        do {
				        logcumultmp = -gam*Math.Log(++i)-logsum;
				        logcumul = Math.Max(logcumul,logcumultmp) + Math.Log(1+Math.Exp(-Math.Abs(logcumul-logcumultmp)));
			        } while (logprob>logcumul);
			        seq[n] = i;
		        }
		        //quicksort(seq, nodes);
                /* Sorts the sequence in non-increasing order */
                seq = (from p in seq orderby p descending select p).ToArray();
	        } while (GTest(seq, nodes,xk)!=1);
	        return seq;
        }
        /* Graphicality test */
        public static int GTest(int[] s, int n, int[] xk)
        {
	        int i, what, flag=0, k;
	        int degsum=0, minsum, c;
	
	        for (i=0;i<n;i++) degsum += s[i];
	        if (degsum%2!=0) what = -1;
	        else {
		        c = -1;
		
		        for (k=n-1; k>=s[0]; k--) xk[k] = 0;									// Hehehe...
                for (k = 1; k < n; k++) for (c = s[k - 1] - 1; c >= s [k]; c--) xk[c] = k;		// Dehihiho
		        for (k=c;k>=0;k--) xk[k]=n;
		
		        degsum = s[0];
		        minsum = n-1;
		
		        k = 1;
		        while (k<n-1 && degsum<=minsum) {										// Test!
	                degsum += s[k];
	                if (xk[k]<k+1) flag=1;
	                if (flag==0) minsum += xk[k]-1;
	                else minsum += 2*k-s[k];
	                k++;
	            }
	    
	            if (degsum>minsum) what = 0;
	            else what = 1;
	        }
    
            return what;
    
        }
        
        /// <summary>
        /// Generates and returns a graphical power-law bi-distributed sequence with the given number of nodes and power-law exponent gamma 
        /// http://www2.warwick.ac.uk/fac/cross_fac/complexity/people/staff/delgenio/bdsgen/
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="gamin">Exponent coeeficient of power law function. It should be greater than zero</param>
        /// <param name="gamout">Exponent coeeficient of power law function. It should be greater than zero</param>
        /// <param name="rng"></param>
        /// <returns></returns>
        public static List<Pair<int, int>> plbdsgen(int nodes, double gamin, double gamout, NumericMath.RandomCraft rng)
        {
	        int i, n, j, k, from, to, dir, mc;
                    int [,]seq=new int[nodes,2];
	        double logsum, logsumtmp, logprob, logcumultmp, logcumul, comp, lprob;
                    double []gam=new double[2];
	
	 
            //seq    = malloc(nodes*sizeof(int*));
            //seq[0] = calloc(2*nodes,sizeof(int));
            //for (n=1; n<nodes; n++) seq[n] = seq[n-1] + 2;
	        gam[0] = gamin;
	        gam[1] = gamout;
	
	        do {
		        if (rng.GetUniDev()<0.5) {
			        dir = 0;
			        mc  = 1;
		        } else {
			        dir = 1;
			        mc  = 0;
		        }
		
		        logsum = 0.0;
		        for (i=2; i<nodes; i++) {
			        logsumtmp = -gam[dir]*Math.Log(i);
			        logsum = Math.Max(logsum,logsumtmp) + Math.Log(1+Math.Exp(-Math.Abs(logsum-logsumtmp)));
		        }
		
		        for (n=0; n<nodes; n++) {
			        logprob  = Math.Log(1+rng.GetUniDev());
			        logcumul = 0.0;
			        i = 0;
			        do {
				        i++;
				        logcumultmp = -gam[dir]*Math.Log(i) - logsum;
				        logcumul = Math.Max(logcumul,logcumultmp) + Math.Log(1+Math.Exp(-Math.Abs(logcumul-logcumultmp)));
			        } while (logprob>logcumul);
			        seq[n,dir] = i;
			        seq[n,mc] = 0;
		        }
		
		        for (i=0; i<nodes; i++) {
			        j = (int)((nodes-i)*rng.GetUniDev()+1);
			        n = k = 0;
				        do if (seq[n++,mc]==0) k++;
				        while (k<j);
			        seq[n-1,mc] = seq[i,dir];
		        }
		
		        for (n=0; n<nodes*nodes; n++) {
			        from = (int)rng.GetUniDev()*nodes;
			        to   = (int)(rng.GetUniDev()*(nodes-1));
			        if (to>=from) to++;
			        if (seq[from,mc]!=1 && seq[to,mc]!=nodes-1) {
				        lprob = gam[mc]*(Math.Log(seq[to,mc])-Math.Log(1+seq[to,mc]));
				        comp  = lprob + Math.Log(1+Math.Exp(-lprob));
				        if (Math.Log(1+rng.GetUniDev())<comp) {
					        seq[from,mc]--;
					        seq[to,mc]++;
				        }
			        }
		        }
		
		        lexqsort(seq,nodes);
	        } while (diGTest(seq,nodes)!=1);
	
	        return DiNetConfigureModel.ConvertToDegSequenceList(seq);
        }

        public static int diGTest(int [,]seq, int n)
        {
	        int t;
            int []G=new int[n+1], S=new int[n+1];
	        int lhs=0, rhs=0, gtilde;
	
	        for (t=0; t<n; t++) {
		        S[t] = G[t] = 0;
		        lhs += seq[t,0];
		        rhs += seq[t,1];
	        }
	        G[n] = S[n] = 0;
	
	        if (lhs!=rhs) return -1;
	
	        G[seq[0,1]+1] = 1;
	        for (t=1; t<n; t++) {
		        G[seq[t,1]]++;
		        if (seq[t,1] >= t+1)  S[seq[t,1]]--;
		        if (seq[t,1]+1 > t+1) S[seq[t,1]+1]++;
	        }
	
	        lhs    = seq[0,0];
	        rhs    = n-1-G[0];
	        gtilde = G[0]+G[1];
	        t = 1;
	        while (t<n-1 && lhs<=rhs) {
		        t++;
		        lhs += seq[t-1,0];
		        rhs += n-gtilde;
		        if (seq[t-1,1]>=t) rhs--;
		        gtilde += G[t]+S[t];
	        }
	
	        if (lhs>rhs) return 0;
	        else return 1;
        }


        static void lexqsort(int [,]s, int n)
        {
	        int i, anyone, pivotin, pivotout, l_ind=0, s_ind=0, h_ind=0;
            int [,]lower=new int[n,2], higher=new int[n,2], same=new int[n,2];
	
	        if (n<=1) return;

            //lower = malloc(n * sizeof(int*));
            //lower[0] = calloc(2 * n, sizeof(int));
            //higher = malloc(n * sizeof(int*));
            //higher[0] = calloc(2 * n, sizeof(int));
            //same = malloc(n * sizeof(int*));
            //same[0] = calloc(2 * n, sizeof(int));
            //for (i=1;i<n;i++) {
            //    lower[i]  = lower[i-1]+2;
            //    higher[i] = higher[i-1]+2;
            //    same[i]   = same[i-1]+2;
            //}

            anyone = NumericMath.RandomCraft.Next(0, n); //(int)(Math.Floor((((double)rand()) / ((double)RAND_MAX)) * (double)n));
	        pivotin  = s[anyone,0];
	        pivotout = s[anyone,1];
	
	        for (i=0;i<n;i++) {
		        if (s[i,0]<pivotin) {
			        lower[l_ind,0]   = s[i,0];
			        lower[l_ind++,1] = s[i,1];
		        } else if (s[i,0]>pivotin) {
			        higher[h_ind,0]   = s[i,0];
			        higher[h_ind++,1] = s[i,1];
		        } else if (s[i,1]<pivotout) {
			        lower[l_ind,0]   = s[i,0];
			        lower[l_ind++,1] = s[i,1];
		        } else if (s[i,1]>pivotout) {
			        higher[h_ind,0]   = s[i,0];
			        higher[h_ind++,1] = s[i,1];
		        } else {
			        same[s_ind,0]   = s[i,0];
			        same[s_ind++,1] = s[i,1];
		        }
	        }
	
	        lexqsort(lower,l_ind);
	        lexqsort(higher,h_ind);
	
	        if (h_ind!=0) for (i=0;i<h_ind;i++) {
		        s[i,0] = higher[i,0];
		        s[i,1] = higher[i,1];
	        }
	        if (s_ind!=0) for (i=h_ind;i<s_ind+h_ind;i++) {
		        s[i,0] = same[i-h_ind,0];
		        s[i,1] = same[i-h_ind,1];
	        }
	        if (l_ind!=0) for (i=s_ind+h_ind;i<h_ind+s_ind+l_ind;i++) {
		        s[i,0] = lower[i-h_ind-s_ind,0];
		        s[i,1] = lower[i-h_ind-s_ind,1];
	        }
        }
        /// <summary>
        /// To test directed random network generator
        /// </summary>
        public static void Test()
        {
            
            int k, i, j, n = 50; int[,] seq;


            seq = new int[n, 2];

            seq[0, 0] = 16;
            seq[0, 1] = 15;

            seq[1, 0] = 13;
            seq[1, 1] = 2;

            seq[2, 0] = 10;
            seq[2, 1] = 2;

            seq[3, 0] = 9;
            seq[3, 1] = 3;

            seq[4, 0] = 9;
            seq[4, 1] = 2;

            seq[5, 0] = 9;
            seq[5, 1] = 1;

            seq[6, 0] = 8;
            seq[6, 1] = 12;

            seq[7, 0] = 7;
            seq[7, 1] = 5;

            seq[8, 0] = 7;
            seq[8, 1] = 4;

            seq[9, 0] = 6;
            seq[9, 1] = 4;

            seq[10, 0] = 5;
            seq[10, 1] = 9;

            seq[11, 0] = 5;
            seq[11, 1] = 2;

            seq[12, 0] = 5;
            seq[12, 1] = 1;

            seq[13, 0] = 4;
            seq[13, 1] = 6;

            seq[14, 0] = 4;
            seq[14, 1] = 3;

            seq[15, 0] = 4;
            seq[15, 1] = 2;

            seq[16, 0] = 4;
            seq[16, 1] = 1;

            seq[17, 0] = 3;
            seq[17, 1] = 2;

            seq[18, 0] = 3;
            seq[18, 1] = 1;

            seq[19, 0] = 3;
            seq[19, 1] = 1;

            seq[20, 0] = 2;
            seq[20, 1] = 14;

            seq[21, 0] = 2;
            seq[21, 1] = 11;

            seq[22, 0] = 2;
            seq[22, 1] = 3;

            seq[23, 0] = 2;
            seq[23, 1] = 2;

            seq[24, 0] = 2;
            seq[24, 1] = 2;

            seq[25, 0] = 2;
            seq[25, 1] = 2;

            seq[26, 0] = 2;
            seq[26, 1] = 2;

            seq[27, 0] = 2;
            seq[27, 1] = 1;

            seq[28, 0] = 1;
            seq[28, 1] = 7;

            seq[29, 0] = 1;
            seq[29, 1] = 5;

            seq[30, 0] = 1;
            seq[30, 1] = 3;

            seq[31, 0] = 1;
            seq[31, 1] = 3;

            seq[32, 0] = 1;
            seq[32, 1] = 2;

            seq[33, 0] = 1;
            seq[33, 1] = 2;

            seq[34, 0] = 1;
            seq[34, 1] = 2;

            seq[35, 0] = 1;
            seq[35, 1] = 1;

            seq[36, 0] = 1;
            seq[36, 1] = 1;

            seq[37, 0] = 1;
            seq[37, 1] = 1;

            seq[38, 0] = 1;
            seq[38, 1] = 1;

            seq[39, 0] = 1;
            seq[39, 1] = 1;

            seq[40, 0] = 0;
            seq[40, 1] = 5;

            seq[41, 0] = 0;
            seq[41, 1] = 2;

            seq[42, 0] = 0;
            seq[42, 1] = 2;

            seq[43, 0] = 0;
            seq[43, 1] = 2;

            seq[44, 0] = 0;
            seq[44, 1] = 2;

            seq[45, 0] = 0;
            seq[45, 1] = 1;

            seq[46, 0] = 0;
            seq[46, 1] = 1;

            seq[47, 0] = 0;
            seq[47, 1] = 1;

            seq[48, 0] = 0;
            seq[48, 1] = 1;

            seq[49, 0] = 0;
            seq[49, 1] = 1;

            //seq[0, 0] = 3;
            //seq[0, 1] = 0;
            //seq[1, 0] = 3;
            //seq[1, 1] = 0;
            //seq[2, 0] = 1;
            //seq[2, 1] = 2;
            //seq[3, 0] = 1;
            //seq[3, 1] = 2;
            //seq[4, 0] = 1;
            //seq[4, 1] = 2;
            //seq[5, 0] = 1;
            //seq[5, 1] = 2;
            //seq[6, 0] = 1;
            //seq[6, 1] = 2;
            //seq[7, 0] = 1;
            //seq[7, 1] = 2;
            int inTo = 0, outTo = 0;
            for(i=0;i<seq.GetLength(0);i++)
            {
                inTo += seq[i, 0];
                outTo += seq[i, 1];
            }
            DiNetConfigureModel Netgen = new DiNetConfigureModel(DiNetConfigureModel.ConvertToDegSequenceList(seq));
            DiNetConfigureModel.Digraph G;

            Mathutil.NumericMath.RandomCraft rnd = new NumericMath.RandomCraft((int)DateTime.Now.Ticks);

            Console.WriteLine("Sampling with uniform choice on the allowed nodes");
            for (k = 0; k < 500; k++)
            {

                G = Netgen.digsam(rnd, 0);

                Console.WriteLine("Adjacency list");
                for (i = 0; i < n; i++)
                {
                    Console.Write(string.Format("{0}: ", i));
                    for (j = 0; j < seq[i, 1]; j++)
                        Console.Write(string.Format("{0} ", G.list[i][j]));
                    Console.WriteLine();
                }
                Console.WriteLine();
                Console.Write(string.Format("log(W): {0}\n\n\n", G.weight));
            }
            Console.WriteLine("\nSampling with uniform choice of the allowed stubs");
            for (k = 0; k < 500; k++)
            {
                G = Netgen.digsam(rnd, 1);
                Console.WriteLine("Adjacency list");
                for (i = 0; i < n; i++)
                {
                    Console.Write(string.Format("{0}: ", i));
                    for (j = 0; j < seq[i, 1]; j++) Console.Write(string.Format("{0} ", G.list[i][j]));
                    Console.WriteLine();
                }
                Console.WriteLine();
                Console.Write(string.Format("log(W): {0}\n\n\n", G.weight));
            }

            BooleanNetwork Net = new BooleanNetwork();
            Net = Netgen.CreateNetwork(Net, rnd, 0) as BooleanNetwork;
            Netutil.DumpNet(Net);
            BooleanNetwork Net2 = Netgen.CreateNetwork(Net, rnd, 0) as BooleanNetwork;
            Netutil.DumpNet(Net2);
        }
    }
}

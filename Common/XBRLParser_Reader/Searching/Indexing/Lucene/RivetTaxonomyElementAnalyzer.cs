using System.Collections;
using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;

namespace Aucent.MAX.AXE.XBRLParser.Searching.Indexing.Lucene
{
    /// <summary>
    /// </summary>
    public class RivetTaxonomyElementAnalyzer : Analyzer
    {
        /// <summary>
        /// </summary>
        public static readonly string[] STOP_WORDS = StopAnalyzer.ENGLISH_STOP_WORDS;
        
        private readonly Hashtable stopSet;

        /// <summary>
        /// </summary>
        public RivetTaxonomyElementAnalyzer()
            : this(STOP_WORDS)
        {}

        /// <summary>
        /// </summary>
        public RivetTaxonomyElementAnalyzer(string[] stopWords)
        {
            stopSet = StopFilter.MakeStopSet(stopWords);
        }

        /// <summary>
        /// </summary>
        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            TokenStream stream = new StandardTokenizer(reader);
            stream = new PorterStemFilter(stream);
            return new StopFilter(new LowerCaseFilter(stream), stopSet);
        }
    }
}

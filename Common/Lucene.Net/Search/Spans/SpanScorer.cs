/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;

using Weight = Lucene.Net.Search.Weight;
using Scorer = Lucene.Net.Search.Scorer;
using Explanation = Lucene.Net.Search.Explanation;
using Similarity = Lucene.Net.Search.Similarity;

namespace Lucene.Net.Search.Spans
{
	
	
	class SpanScorer : Scorer
	{
		private Spans spans;
		private Weight weight;
		private byte[] norms;
		private float value_Renamed;
		
		private bool firstTime = true;
		private bool more = true;
		
		private int doc;
		private float freq;
		
		internal SpanScorer(Spans spans, Weight weight, Similarity similarity, byte[] norms) : base(similarity)
		{
			this.spans = spans;
			this.norms = norms;
			this.weight = weight;
			this.value_Renamed = weight.GetValue();
			doc = - 1;
		}
		
		public override bool Next()
		{
			if (firstTime)
			{
				more = spans.Next();
				firstTime = false;
			}
			return SetFreqCurrentDoc();
		}
		
		public override bool SkipTo(int target)
		{
			if (firstTime)
			{
				more = spans.SkipTo(target);
				firstTime = false;
			}
			if (!more)
			{
				return false;
			}
			if (spans.Doc() < target)
			{
				// setFreqCurrentDoc() leaves spans.doc() ahead
				more = spans.SkipTo(target);
			}
			return SetFreqCurrentDoc();
		}
		
		private bool SetFreqCurrentDoc()
		{
			if (!more)
			{
				return false;
			}
			doc = spans.Doc();
			freq = 0.0f;
			while (more && doc == spans.Doc())
			{
				int matchLength = spans.End() - spans.Start();
				freq += GetSimilarity().SloppyFreq(matchLength);
				more = spans.Next();
			}
			return more || (freq != 0);
		}
		
		public override int Doc()
		{
			return doc;
		}
		
		public override float Score()
		{
			float raw = GetSimilarity().Tf(freq) * value_Renamed; // raw score
			return raw * Similarity.DecodeNorm(norms[doc]); // normalize
		}
		
		public override Explanation Explain(int doc)
		{
			Explanation tfExplanation = new Explanation();
			
			SkipTo(doc);
			
			float phraseFreq = (Doc() == doc) ? freq : 0.0f;
			tfExplanation.SetValue(GetSimilarity().Tf(phraseFreq));
			tfExplanation.SetDescription("tf(phraseFreq=" + phraseFreq + ")");
			
			return tfExplanation;
		}
	}
}
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

namespace Lucene.Net.Analysis
{
	
    /// <summary> Normalizes token text to lower case.
    /// 
    /// </summary>
    /// <version>  $Id: LowerCaseFilter.java 150259 2004-03-29 22:48:07Z cutting $
    /// </version>
    public sealed class LowerCaseFilter : TokenFilter
    {
        public LowerCaseFilter(TokenStream in_Renamed) : base(in_Renamed)
        {
        }
		
        public override Token Next()
        {
            Token t = input.Next();
			
            if (t == null)
                return null;
			
            t.termText = t.termText.ToLower();
			
            return t;
        }
    }
}
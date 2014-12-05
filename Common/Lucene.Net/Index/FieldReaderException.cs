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

namespace Lucene.Net.Index
{
	/// <summary> 
	/// 
	/// 
	/// </summary>
	[Serializable]
	public class FieldReaderException : System.SystemException
	{
		/// <summary> Constructs a new runtime exception with <code>null</code> as its
		/// detail message.  The cause is not initialized, and may subsequently be
		/// initialized by a call to {@link #initCause}.
		/// </summary>
		public FieldReaderException()
		{
		}
		
		/// <summary> Constructs a new runtime exception with the specified cause and a
		/// detail message of <tt>(cause==null ? null : cause.toString())</tt>
		/// (which typically contains the class and detail message of
		/// <tt>cause</tt>).  This constructor is useful for runtime exceptions
		/// that are little more than wrappers for other throwables.
		/// 
		/// </summary>
		/// <param name="cause">the cause (which is saved for later retrieval by the
		/// {@link #getCause()} method).  (A <tt>null</tt> value is
		/// permitted, and indicates that the cause is nonexistent or
		/// unknown.)
		/// </param>
		/// <since> 1.4
		/// </since>
		public FieldReaderException(System.Exception cause)
		{
		}
		
		/// <summary> Constructs a new runtime exception with the specified detail message.
		/// The cause is not initialized, and may subsequently be initialized by a
		/// call to {@link #initCause}.
		/// 
		/// </summary>
		/// <param name="message">the detail message. The detail message is saved for
		/// later retrieval by the {@link #getMessage()} method.
		/// </param>
		public FieldReaderException(System.String message)
		{
		}
		
		/// <summary> Constructs a new runtime exception with the specified detail message and
		/// cause.  <p>Note that the detail message associated with
		/// <code>cause</code> is <i>not</i> automatically incorporated in
		/// this runtime exception's detail message.
		/// 
		/// </summary>
		/// <param name="message">the detail message (which is saved for later retrieval
		/// by the {@link #getMessage()} method).
		/// </param>
		/// <param name="cause">  the cause (which is saved for later retrieval by the
		/// {@link #getCause()} method).  (A <tt>null</tt> value is
		/// permitted, and indicates that the cause is nonexistent or
		/// unknown.)
		/// </param>
		/// <since> 1.4
		/// </since>
		public FieldReaderException(System.String message, System.Exception cause)
		{
		}
	}
}
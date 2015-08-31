/**
 * PrismTech licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with the
 * License and with the PrismTech Vortex product. You may obtain a copy of the
 * License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License and README for the specific language governing permissions and
 * limitations under the License.
 */
using System;

using System.Threading.Tasks;
using System.Collections.Generic;

namespace com.prismtech.vortex.web.cs.api
{
	public delegate K ExtractKey<K,V>(V value);

	public interface DataCache<K,V>
	{
		

		Task write (K key, V value);

		/// <summary>
		/// Returns the values included in the cache as a List.
		/// </summary>
		List<V> read();

		/// <summary>
		/// Returns the last value in the cache for each key.
		/// </summary>
		List<V> readLast();

		/// <summary>
		/// Returns all the values included in the cache as a List and
		/// empties the cache.
		/// </summary>
		List<V> take();

		/// <summary>
		/// Returns the last value in the cache for each key and removes
		/// them from the cache.
		/// </summary>
		List<V> takeLast();

		/// <summary>
		/// Returns the last value in the cache at the key.
		/// </summary>
		V get(K key);

		/// <summary>
		/// Clears the data cache.
		/// </summary>
		void clear();

		/// <summary>
		/// Binds a reader to a cache.
		/// </summary>
		/// <param name="reader">the data reader to bind to the cache</param>
		/// <param name="key">a function returning the topic key</param>
		void bind(DataReader<V> reader, ExtractKey<K,V> key);
	}
}


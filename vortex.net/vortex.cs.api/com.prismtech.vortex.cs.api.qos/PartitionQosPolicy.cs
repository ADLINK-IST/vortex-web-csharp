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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace com.prismtech.vortex.cs.api.qos
{
	[JsonConverter(typeof(ParitionQosPolicyConverter))]
	public struct PartitionQosPolicy
	{
		readonly string[] values;

		public PartitionQosPolicy (params string[] values)
		{
			this.values = values;
		}

		public string[] Values {
			get {
				return this.values;
			}
		}
	}

	public class ParitionQosPolicyConverter : JsonConverter 
	{
		public override void WriteJson (JsonWriter writer, object value, JsonSerializer serializer)
		{
			var policy = (PartitionQosPolicy)value;
			var json = new JObject ();
			json.Add (new JProperty ("id", 2));
			json.Add(new JProperty("vs", new JArray(policy.Values)));
			json.WriteTo (writer);
		}


		public override object ReadJson (JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			throw new NotImplementedException ();
		}

		public override bool CanConvert (Type objectType)
		{
			return objectType == typeof(PartitionQosPolicy);
		}
	}
}


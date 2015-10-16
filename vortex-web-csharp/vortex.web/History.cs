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

namespace vortex.web
{
	public enum HistoryKind : int {
		KEEP_ALL = 0,
		KEEP_LAST = 1
	}

	[JsonConverter(typeof(HistoryConverter))]
	public struct History : QosPolicy
	{
		private readonly HistoryKind _kind;
		private readonly int _depth;
		private static readonly History HistoryKeepLastOne = new History (HistoryKind.KEEP_LAST, 1);

		public readonly static History KeepAll = 
			new History (HistoryKind.KEEP_ALL, -1);

		public static History KeepLast (int n) {
			if (n == 1)
				return HistoryKeepLastOne;
			else
				return new History (HistoryKind.KEEP_LAST, n);
		}


		private History(HistoryKind kind, int depth)
		{
			_kind = kind;
			_depth = depth;
		}

		public HistoryKind Kind {
			get {
				return this._kind;
			}
		}

		public int Depth {
			get {
				return this._depth;
			}
		}



	}

	public class HistoryConverter : JsonConverter
	{
		public override void WriteJson (JsonWriter writer, object value, JsonSerializer serializer)
		{
			var policy = (History)value;
			var json = new JObject ();
			json.Add (new JProperty ("id", 0));
			json.Add (new JProperty ("k", (int)policy.Kind));

			if (policy.Kind == HistoryKind.KEEP_LAST) 
			{
				json.Add (new JProperty ("v", policy.Depth));
			}

			json.WriteTo (writer);
		}

		public override object ReadJson (JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			throw new NotImplementedException ();
		}

		public override bool CanConvert (Type objectType)
		{
			return objectType == typeof(History);
		}
	}
}


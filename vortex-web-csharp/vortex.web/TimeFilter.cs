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
using System.Runtime.InteropServices;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[assembly: ComVisible(false)]
namespace vortex.web
{
	[JsonConverter(typeof(TimeFilterConverter))]
    [ComVisible(true)]
    public struct TimeFilter : QosPolicy
	{
		private readonly int _value;

		public TimeFilter (int value)
		{
			_value = value;
		}

		public int Value {
			get {
				return this._value;
			}
		}
    }
    
    [ComVisible(false)]
	public class TimeFilterConverter : JsonConverter
	{
		public override void WriteJson (JsonWriter writer, object value, JsonSerializer serializer)
		{
			var policy = (TimeFilter)value;
			var json = new JObject ();

			json.Add (new JProperty ("id", 4));
			json.Add (new JProperty ("v", policy.Value));

			json.WriteTo (writer);
		}

		public override object ReadJson (JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			throw new NotImplementedException ();
		}

		public override bool CanConvert (Type objectType)
		{
			return objectType == typeof(TimeFilter);
		}
	}
}


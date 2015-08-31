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

using com.prismtech.vortex.web.cs.api;
using com.prismtech.vortex.cs.api.qos;

namespace com.prismtech.vortex.cs.api
{
	class VortexClientTest {
		public String URL { get; set; }
		public String Token { get; set; }

		public VortexClientTest (string uRL, string token)
		{
			this.URL = uRL;
			this.Token = token;
		}

		public async Task run() {
			var domainId = 0;
			Vortex v = new VortexImpl (domainId);

			await v.Connect (URL, Token);

			if (v.IsConnected) {
				var tn = "TChatMessage";
				var tt = "com.prismtech.cafe.demo.idl.ChatMessage";
				var trt = "com.prismtech.cafe.demo.idl.ChatMessage";
				var qos = new List<QosPolicy>();

				Console.WriteLine ("Create Topic");
				var topic = await v.CreateTopic<ChatMessage> (tn, tt, trt, qos);
				Console.WriteLine ("Create Topic: Complete");

				Console.WriteLine ("Create Reader");
				var reader = await v.CreateDataReader<ChatMessage> (topic, qos);
				Console.WriteLine ("Create Reader: Complete");
				reader.OnDataAvailable += (object sender, SampleData<ChatMessage> e) => {
					Console.WriteLine("Received: " + e.Data);
				};

				Console.WriteLine ("Create Writer");
				var writer = await v.CreateDataWriter<ChatMessage> (topic, qos);
				Console.WriteLine ("Create Writer: Complete");

				var msg = new ChatMessage ("csharp", "roks!");
				for (var i = 0; i < 10; i++) {
					await writer.Write (msg);
				}

				// Sleep for 5 seconds.
				System.Threading.Thread.Sleep (5000);
				await v.Disconnect ();
				await v.Close ();
			}
		}
	}

	public class VortexAPIExample
	{
		private VortexAPIExample ()
		{
		}

		public static void Main (string[] args)
		{
			var url = "ws://localhost:9000";
			var token = "nopwd";
			var client = new VortexClientTest (url, token);
			client.run ().Wait ();

			Console.WriteLine ("Completed");
		}
	}
}


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
using com.prismtech.vortex.web.proto;
using System.Threading.Tasks;
using WebSocketSharp;

namespace com.prismtech.vortex.web.cs.api
{
	public delegate void StringHandler(string server);
		
	public class Runtime
	{
		public event StringHandler OnConnect;
		public event StringHandler OnDisconnect;

		private SControlLink ctrlLink;
		private String url;

		public bool Connected { 
			get { return ctrlLink.Connected; } 
		}
		
		public Runtime ()
		{
			ctrlLink = new SControlLink ();	
		}

		public Task ConnectAsync (string url, string authToken) {			
			this.url = url;
			return ctrlLink.ConnectAsync (url, authToken);
		}

		public void Disconnect () {
			ctrlLink.DisconnectAsync ();
			OnDisconnect (this.url);
		}

		public  Task  CreateTopicAsync (int did, string tname, string ttype, string tregtype, string qos) { 			
			return ctrlLink.CreateTopicAsync (did, tname, ttype, tregtype, qos);
		}

		public Task<WebSocket> CreateReaderAsync (int did, string tname, string qos) { 
			return ctrlLink.CreateReaderAsync(did, tname, qos);
		}

		public Task<WebSocket> CreateWriterAsync (int did, string tname, string qos) { 
			return ctrlLink.CreateWriterAsync(did, tname, qos);
		}


	}
}


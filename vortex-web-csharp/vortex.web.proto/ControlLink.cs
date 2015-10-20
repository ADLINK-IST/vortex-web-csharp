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
using System.Text;
using System.Threading;
using vortex.web;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.WebSockets;

namespace vortex.web.proto
{
	public class ControlLink : IDisposable
	{
		private const string ctrlPath = "/vortex/controller/";
		private const string readerPath = "/vortex/reader/";
		private const string writerPath = "/vortex/writer/";
		private byte[] buf;
		private ArraySegment<byte> segment;
		string url;

		int seqNum = 0;
		ClientWebSocket ws = new ClientWebSocket ();

		public bool Connected 
		{ 
			get {				
				return ws.State == WebSocketState.Open; 			
			}
		} 
			

		public ControlLink() {
			buf = new byte[8192];
			segment = new ArraySegment<byte> (buf);
		}

		public Task ConnectAsync (string url, string authToken) {			
			if (!Connected) {
				this.url = url;
				Console.WriteLine ("Connecting to: " + url);
				var uri = new Uri (url + ctrlPath + authToken);
				Console.WriteLine ("Issuing connect to " + url);
				return ws.ConnectAsync (uri, CancellationToken.None);		

			} else
				throw new InvalidOperationException ("The runtime is already connected.");
		}
			
		int nextSequenceNumber() {
			return Interlocked.Increment (ref seqNum);
		}

		public Task DisconnectAsync () { 
			System.Console.WriteLine ("Closing Socket!");
			return ws.CloseAsync (WebSocketCloseStatus.NormalClosure, "Closed by client", CancellationToken.None);
		}
			
		private void assertConnection () {
			if (!Connected) throw new InvalidOperationException ("The runtime is already connected.");
		}

		private Task sendCommand<T> (T cmd) {
			var jsonCmd = JsonConvert.SerializeObject (cmd);
			var ecmd = Encoding.UTF8.GetBytes (jsonCmd);
			var buffer = new ArraySegment<Byte>(ecmd, 0, ecmd.Length);
			return ws.SendAsync (buffer, System.Net.WebSockets.WebSocketMessageType.Text, true, CancellationToken.None);
		}

		public async Task CreateTopicAsync(int did, string tname, string ttype, string tregtype, List<QosPolicy> qos) { 
			assertConnection ();
			var sn = nextSequenceNumber ();
			// In DDS types are represented with "::" separator
			var canonicalTregType = tregtype.Replace (".", "::");
			var tinfo = new TopicInfo (did, tname, ttype, canonicalTregType, qos);
			var cmd = new CreateTopic (tinfo, sn);
			await sendCommand (cmd);
			var reply = await receiveReplyAsync ();
			if (reply.h.cid != CommandId.OK)
				throw new InvalidOperationException ("The topic cration failed because of: " + reply.b.msg);
		}

		private async Task<CommandReply> receiveReplyAsync() {
			var result = await ws.ReceiveAsync (segment, CancellationToken.None);
			var count = result.Count;
			if (count > 0) {				
				var json = Encoding.UTF8.GetString (buf);
				return JsonConvert.DeserializeObject<CommandReply> (json);
			} else {
				var b = new ReplyBody (" ", "Error"); 
				var h = new Header (CommandId.Error, EntityKind.Runtime, -1);
				return  new CommandReply (h, b);
			}
				
		}
			
		public async Task<ClientWebSocket> CreateReaderAsync(int did, string tname, List<QosPolicy> qos) { 
			assertConnection ();
			var sn = nextSequenceNumber ();
			var ei = new EndpointInfo (did, tname, qos);
			var cmd = new CreateReader (ei, sn);
			await sendCommand (cmd);				
			var reply = await receiveReplyAsync ();
			Console.WriteLine ("Reply: {eid: " + reply.b.eid + ", msg: " + reply.b.msg + "}");
			if (reply.h.cid == CommandId.OK) {
				var rws = new ClientWebSocket ();
				var uri = new Uri (url + readerPath + reply.b.eid);
				await rws.ConnectAsync (uri, CancellationToken.None);
				return rws;
			} else
				return null;
		}

		public async Task<ClientWebSocket> CreateWriterAsync(int did, string tname, List<QosPolicy> qos) { 
			assertConnection ();
			assertConnection ();
			var sn = nextSequenceNumber ();
			var ei = new EndpointInfo (did, tname, qos);
			var cmd = new CreateWriter (ei, sn);
			await sendCommand (cmd);
			var reply = await receiveReplyAsync ();
			Console.WriteLine ("Reply: {eid: " + reply.b.eid + ", msg: " + reply.b.msg + "}");

			if (reply.h.cid == CommandId.OK) {
				var wws = new ClientWebSocket ();
				// wws.Options.UseDefaultCredentials ();
				var uri = new Uri (url + writerPath + reply.b.eid);
				Console.WriteLine ("Connecting Writer to: " + url + writerPath + reply.b.eid);
				await wws.ConnectAsync (uri, CancellationToken.None);
				Console.WriteLine ("Connected Writer to: " + url + writerPath + reply.b.eid);
				return wws;
			} else
				return null;
		}

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {                    
                    this.ws.Dispose();
                }
                this.ws = null;
                this.url = null;                
                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ControlLink() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion


    }
}


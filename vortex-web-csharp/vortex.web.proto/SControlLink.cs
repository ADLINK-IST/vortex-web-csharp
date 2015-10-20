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
using WebSocketSharp;
using System.Collections;
using System.Collections.Generic;

namespace vortex.web.proto
{
	public class SControlLink : IDisposable
	{
		private const string ctrlPath = "/vortex/controller/";
		private const string readerPath = "/vortex/reader/";
		private const string writerPath = "/vortex/writer/";
		private Hashtable taskMap;
		string url;
		WebSocket ws;

		int seqNum = 0;
		private bool connected = false;

		public bool Connected 
		{ 
			get {				
				return connected; 			
			}
		} 


		public SControlLink() {			
			taskMap = new Hashtable ();
		}

		private Task<bool> ConnectWebSockAsync (WebSocket ws) {
			var t = new TaskCompletionSource<bool> ();                                    
			ws.OnOpen += (sender, e) => { 
				t.SetResult(true); 
				connected = true;
			};
            try {
                ws.Connect();
            } 
            catch (System.Net.Sockets.SocketException se)
            {
                t.SetException(se);
            }
            catch (ArgumentException e)
            {
                t.SetException(e);       
            }
			return t.Task;
		}
		public Task<bool> ConnectAsync (string url, string authToken) {						
			if (!Connected) {
				this.url = url;
				var uri = url + ctrlPath + authToken;                
				ws = new WebSocket (uri);
				ws.OnMessage += (sender, e) =>  {
					var msg = JsonConvert.DeserializeObject<CommandReply> (e.Data);
					lock (taskMap) {
						var task = taskMap[msg.h.sn] as TaskCompletionSource<CommandReply>; 
						if (task != null) {							
							task.SetResult (msg);
						}
					}
				};
				return ConnectWebSockAsync (ws);

			} else
				throw new InvalidOperationException ("The runtime is already connected.");
		}

		int nextSequenceNumber() {
			return Interlocked.Increment (ref seqNum);
		}

		public Task DisconnectAsync () { 
			return Task.Run(() => ws.Close ());

		}

		private void assertConnection () {
			if (!Connected) throw new InvalidOperationException ("The runtime is already connected.");
		}

		private Task sendCommand<T> (T cmd, int sn) {
			var t = new TaskCompletionSource<CommandReply> ();
			lock (taskMap) {
				taskMap.Add (sn, t);
			}
			var jsonCmd = JsonConvert.SerializeObject (cmd);
			return Task.Run (() => ws.Send (jsonCmd));
		}

		public async Task CreateTopicAsync(int did, string tname, string ttype, string tregtype, List<QosPolicy> qos) { 
			assertConnection ();
			var sn = nextSequenceNumber ();
			// In DDS types are represented with "::" separator
			var canonicalTregType = tregtype.Replace (".", "::");
			var tinfo = new TopicInfo (did, tname, ttype, canonicalTregType, qos);
			var cmd = new CreateTopic (tinfo, sn);
			await sendCommand (cmd, sn);
			var reply = await receiveReplyAsync (sn);
			if (reply.h.cid != CommandId.OK)
				throw new InvalidOperationException ("The topic cration failed because of: " + reply.b.msg);
		}

		private Task<CommandReply> receiveReplyAsync(int sn) {
			TaskCompletionSource<CommandReply> task = null;
			lock (taskMap) {
				task = taskMap[sn] as TaskCompletionSource<CommandReply>;
			}
			if (task == null) 
				throw new InvalidOperationException ("The Control Link State is inconsistent. Could not find task for an outstanding request");			
			var t = task.Task;
			var continuation = t.ContinueWith (antecedent => {
				lock (taskMap) { taskMap.Remove (sn); }
				return antecedent.Result;
			});
			return continuation;
		}

		public async Task<WebSocket> CreateReaderAsync(int did, string tname, List<QosPolicy> qos) { 
			assertConnection ();
			var sn = nextSequenceNumber ();
			var ei = new EndpointInfo (did, tname, qos);
			var cmd = new CreateReader (ei, sn);
			await sendCommand (cmd, sn);				
			var reply = await receiveReplyAsync (sn);
			Console.WriteLine ("Reply: {eid: " + reply.b.eid + ", msg: " + reply.b.msg + "}");
			if (reply.h.cid == CommandId.OK) {
				var uri = url + readerPath + reply.b.eid;
				var rws = new WebSocket (uri);
				await ConnectWebSockAsync (rws);
				return rws;
			} else
				return null;
		}

		public async Task<WebSocket> CreateWriterAsync(int did, string tname, List<QosPolicy> qos) { 
			assertConnection ();
			var sn = nextSequenceNumber ();
			var ei = new EndpointInfo (did, tname, qos);
			var cmd = new CreateWriter (ei, sn);
			await sendCommand (cmd,sn);
			var reply = await receiveReplyAsync (sn);
			Console.WriteLine ("Reply: {eid: " + reply.b.eid + ", msg: " + reply.b.msg + "}");

			if (reply.h.cid == CommandId.OK) {
				var uri = url + writerPath + reply.b.eid;
				var wws = new WebSocket (uri);
				Console.WriteLine ("Connecting Writer to: " + url + writerPath + reply.b.eid);
				await ConnectWebSockAsync (wws);
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
                    // TODO: dispose managed state (managed objects).
                    this.ws.Close();                    
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                this.ws = null;
                this.taskMap = null;
                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~SControlLink() {
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


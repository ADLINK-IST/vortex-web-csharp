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
using vortex.web.proto;
using System.Threading.Tasks;
using WebSocketSharp;
using System.Collections.Generic;

namespace vortex.web
{
    public class StringHandlerEventArgs : EventArgs
    {
        public string Server { get; }

        public StringHandlerEventArgs(string server)
        {
            this.Server = server;
        }
    }
	public delegate void StringHandler(object sender, StringHandlerEventArgs e);
		
	public class Runtime : IDisposable
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
			OnDisconnect (this, new StringHandlerEventArgs(this.url));
		}

		public  Task  CreateTopicAsync (int did, string tname, string ttype, string tregtype, List<QosPolicy> qos) { 			
			return ctrlLink.CreateTopicAsync (did, tname, ttype, tregtype, qos);
		}			

		public Task<WebSocket> CreateReaderAsync (int did, string tname, List<QosPolicy> qos) { 
			return ctrlLink.CreateReaderAsync(did, tname, qos);
		}

		public Task<WebSocket> CreateWriterAsync (int did, string tname, List<QosPolicy> qos) { 
			return ctrlLink.CreateWriterAsync(did, tname, qos);
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
                    this.ctrlLink.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                this.ctrlLink = null;
                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Runtime() {
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


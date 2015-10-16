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
using WebSocketSharp;
using Newtonsoft.Json;

namespace vortex.web
{
	public interface DataReader {
		event EventHandler<OnDataReaderConnectEventArgs> OnConnectEvent;
		event EventHandler<OnDataReaderDisconnectEventArgs> OnDisconnectEvent;
		event EventHandler<OnDataReaderCloseEventArgs> OnCloseEvent;

		Task Close();
		Task Connect();
		Task Disconnect();
	}

	public interface DataReader<T> : DataReader {
		event EventHandler<SampleData<T>> OnDataAvailable;
	}

	public class DataHolder {
		public string value;

		public DataHolder(string v) {
			this.value = v;
		}
	}

	public class DataReaderImpl<T> : DataReader<T>
	{
		public event EventHandler<SampleData<T>> OnDataAvailable;

		public event EventHandler<OnDataReaderConnectEventArgs> OnConnectEvent;

		public event EventHandler<OnDataReaderDisconnectEventArgs> OnDisconnectEvent;

		public event EventHandler<OnDataReaderCloseEventArgs> OnCloseEvent;

		private readonly WebSocket ws;
		private Boolean isFlexy = true;

		public DataReaderImpl (WebSocket ws)
		{
			this.ws = ws;
			ws.OnClose += (object sender, CloseEventArgs e) => Close();
			ws.OnMessage += OnMessage;
			if (typeof(vortex.web.ITopicType).IsAssignableFrom (typeof(T)))
				isFlexy = false;
					
		}
		

		public async Task Close ()
		{
			await Disconnect();
			OnCloseEvent (this, new OnDataReaderCloseEventArgs ());
		}

		public Task Connect ()
		{
			return Task.Run( () => {
				ws.Connect ();
				OnConnectEvent (this, new OnDataReaderConnectEventArgs ());
			});
		}

		public Task Disconnect ()
		{
			return Task.Run (() => {
				ws.Close ();
				OnDisconnectEvent (this, new OnDataReaderDisconnectEventArgs ());
			});
		}

		public void OnMessage(object sender, MessageEventArgs e) {
			var json = e.Data;
			if (isFlexy) {
				DataHolder holder = JsonConvert.DeserializeObject<DataHolder> (json);
				T obj = JsonConvert.DeserializeObject<T> (holder.value);
				OnDataAvailable (this, new SampleData<T> (obj));
			} else {
				T obj = JsonConvert.DeserializeObject<T> (json);
				OnDataAvailable (this, new SampleData<T> (obj));
			}

		}
			
	}

	public sealed class OnDataReaderConnectEventArgs : EventArgs
	{
		public OnDataReaderConnectEventArgs ()
		{

		}
	}

	public sealed class OnDataReaderDisconnectEventArgs : EventArgs
	{
		public OnDataReaderDisconnectEventArgs ()
		{

		}
	}

	public sealed class OnDataReaderCloseEventArgs : EventArgs
	{
		public OnDataReaderCloseEventArgs ()
		{

		}
	}

	public sealed class SampleData<T> : EventArgs
	{
		public T Data { get; private set; }
		public SampleData (T data)
		{
			Data = data;
		}
	}
}


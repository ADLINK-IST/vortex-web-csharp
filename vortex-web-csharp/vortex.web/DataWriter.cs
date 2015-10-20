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
	public interface DataWriter {
		event EventHandler<OnDataWriterCloseEventArgs> OnCloseEvent;

		Task Close();
		// Task Write (params object[] ds);
		Task Write (object d);
	}

	public interface DataWriter<T>  : DataWriter {
		Task Write(T d);
	}

	public class DataWriterImpl<T> : DataWriter<T>
	{
		public event EventHandler<OnDataWriterCloseEventArgs> OnCloseEvent;

		private readonly WebSocket ws;


		public DataWriterImpl (WebSocket ws)
		{
			this.ws = ws;
			ws.OnClose += OnWebSocketClose;
			ws.OnMessage += OnWebSocketMessage;
			ws.OnError += OnWebSocketError;
		}

		public Task Write (T d) {
			return Write (d as object);
		}

		public Task Write (object d)
		{
			return Task.Run(() => {
				if (ws.IsAlive) {
					var data = new DataHolder(JsonConvert.SerializeObject (d));
					var json = JsonConvert.SerializeObject (data);
					ws.Send(json);
				}
			});
		}

//		public Task Write (params T[] ds)
//		{
//			return Write (ds as object[]);
//		}

		public Task Close ()
		{
			return Task.Run (() => {
				if (ws.IsAlive) {
					Console.WriteLine ("Closing WebSocket!!!");
					ws.Close ();
				}

				OnCloseEvent (this, new OnDataWriterCloseEventArgs ());
			});
		}
//
//		public Task Write (params object[] ds)
//		{
//			return Task.Run(() => {
//				if (ws.IsAlive) {
//					var data = new DataHolder(JsonConvert.SerializeObject (ds));
//					var json = JsonConvert.SerializeObject (data);
//					ws.Send(json);
//				}
//			});
//		}

		private void OnWebSocketClose(object sender, CloseEventArgs args) {
			Console.WriteLine ("Closing Data Writer!");
			Close ();
		}

		private void OnWebSocketMessage(object sender, MessageEventArgs args) {
		}

		private void OnWebSocketError(object sender, ErrorEventArgs args) {
			Console.WriteLine ("WebSocket Error!");
		}
	}

	public sealed class OnDataWriterCloseEventArgs : EventArgs
	{
		public OnDataWriterCloseEventArgs ()
		{
			
		}
	}
}


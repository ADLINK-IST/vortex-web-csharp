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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

using vortex.web;
using vortex.web.proto;

namespace vortex.web
{

    interface IVortex
	{
		event EventHandler<OnConnectEventArgs> RaiseConnectEvent;
		event EventHandler<OnDisconnectEventArgs> RaiseDisconnectEvent;
		event EventHandler<OnCloseEventArgs> RaiseCloseEvent;
		event EventHandler<OnNewTopicEventArgs> RaiseNewTopicEvent;
		event EventHandler<OnNewDataReaderEventArgs> RaiseNewDataReaderEvent;
		event EventHandler<OnNewDataWriterEventArgs> RaiseNewDataWriterEvent;

		/// <summary>
		/// Connect to the server. If already connected an exception is thrown.
		/// </summary>
		/// <param name="srv">Server URL</param>
		/// <param name="authToken">Authorization token.</param>
		Task<bool> Connect(string srv, string authToken);

		/// <summary>
		/// Disconnects, without closing. Upon re-connection all existing subscriptions
		/// and publications will be restablished.
		/// </summary>
		Task Disconnect();

		/// <summary>
		/// Closes the Vortex object and as a consequence all the 'DataReaders' and 'DataWriters'
		/// that belong to it.
		/// </summary>
		Task Close();

		/// <summary>
		/// Checks whether Vortex is connected.
		/// </summary>
		/// <returns><c>true</c>, if connected, <c>false</c> otherwise.</returns>
		bool IsConnected{ get; }

		/// <summary>
		/// Checks whether Vortex is closed.
		/// </summary>
		/// <returns><c>true</c>, if closed , <c>false</c> otherwise.</returns>
		bool IsClosed{ get; }

		/// <summary>
		/// Creates a new data writer.
		/// </summary>
		/// <returns>The newly created DataWriter</returns>
		/// <param name="topic">The topic that will be written by the DataWriter</param>
		/// <param name="qos">Possibly empty list of QoS policies for the DataWriter</param>
		Task<DataWriter<T>> CreateDataWriter<T>(Topic topic, List<QosPolicy> qos);

		/// <summary>
		/// Creates a new data reader.
		/// </summary>
		/// <returns>The newly created DataReader</returns>
		/// <param name="topic">The topic that will be read by the DataReader</param>
		/// <param name="qos">Possibly empty list of QoS policies for the DataReader.</param>
		Task<DataReader<T>> CreateDataReader<T>(Topic topic, List<QosPolicy> qos);

		/// <summary>
		/// Create a new Topic
		/// </summary>
		/// <returns>The newly created Topic.</returns>
		/// <param name="name">The name of the new topic.</param>
		/// <param name="type">The type to be used by the server for creating the topic.</param>
		/// <param name="typeRegistrationType">The type to register the topic with Vortex.</param>
		/// <param name="qos">Possibly empty list of Qos policies for the Topic.</param>
		Task<Topic> CreateTopic<T>(string name, string type, string typeRegistrationType, List<QosPolicy> qos);
	}

	public class Vortex : IVortex
	{
		
		public event EventHandler<OnConnectEventArgs> RaiseConnectEvent;
		public event EventHandler<OnDisconnectEventArgs> RaiseDisconnectEvent;
		public event EventHandler<OnCloseEventArgs> RaiseCloseEvent;
		public event EventHandler<OnNewTopicEventArgs> RaiseNewTopicEvent;
		public event EventHandler<OnNewDataReaderEventArgs> RaiseNewDataReaderEvent;
		public event EventHandler<OnNewDataWriterEventArgs> RaiseNewDataWriterEvent;

		private const Int32 CONNECTED = 1;
		private const Int32 NOT_CONNECTED = 0;

		private const Int32 CLOSED = 1;
		private const Int32 NOT_CLOSED = 0;

		private SControlLink _ctrl;

		private Int32 _isConnected;
		private Int32 _isClosed;

		public int Domain{ get; private set; }
		public string URL { get; private set; }

		public readonly List<QosPolicy> DefaultReaderQos = new List<QosPolicy> () { Reliability.BestEffort, History.KeepLast(1) };
		public readonly List<QosPolicy> DefaultWriterQos = new List<QosPolicy> () { Reliability.Reliable,   History.KeepLast(1) };
		public readonly List<QosPolicy> DefaultTopicQos = new List<QosPolicy> ()  { Reliability.BestEffort, History.KeepLast(1) };

		/// <summary>
		/// Checks whether Vortex is connected.
		/// </summary>
		/// <returns><c>true</c>, if connected, <c>false</c> otherwise.</returns>
		public bool IsConnected
		{
			get { return _isConnected == CONNECTED; }
		}

		/// <summary>
		/// Checks whether Vortex is closed.
		/// </summary>
		/// <returns><c>true</c>, if closed , <c>false</c> otherwise.</returns>
		public bool IsClosed 
		{
			get { return _isClosed == CLOSED; }
		}

		/// <summary>
		/// The domain for this Vortex object.
		/// </summary>
		/// <param name="domain">Domain.</param>
		public Vortex (int vortexDomain)
		{
			System.Diagnostics.Debug.WriteLine ($"Creating Vortex for domain {Domain}");
			Domain = vortexDomain;
			_isConnected = NOT_CONNECTED;
			_isClosed = NOT_CLOSED;
			URL = "";
		}

		/// <summary>
		/// Connect to the server. If already connected an exception is thrown.
		/// </summary>
		/// <param name="srv">Server URL</param>
		/// <param name="authToken">Authorization token.</param>
		public async Task<bool> Connect(string srv, string authToken) 
		{
			System.Diagnostics.Debug.WriteLine ($"Connecting to {srv}");
            bool connected = false;
			if (!IsClosed && NOT_CONNECTED == Interlocked.CompareExchange (ref _isConnected, CONNECTED, NOT_CONNECTED)) {
				URL = srv;
				_ctrl = new SControlLink ();
				await _ctrl.ConnectAsync (srv, authToken);
				OnConnect (new OnConnectEventArgs(URL));
				System.Diagnostics.Debug.WriteLine ($"Connected to {srv}");
                connected = true;
			} else {				
				System.Diagnostics.Trace.WriteLine ($"Unable to connect to {srv}");
			}
            return connected;
		}

		/// <summary>
		/// Disconnects, without closing. Upon re-connection all existing subscriptions
		/// and publications will be restablished.
		/// </summary>
		public async Task Disconnect() 
		{
			System.Diagnostics.Debug.WriteLine ($"Disconnecting from {URL}");

			if (CONNECTED == Interlocked.CompareExchange (ref _isConnected, NOT_CONNECTED, CONNECTED)) 
			{
				await _ctrl.DisconnectAsync ();
				_ctrl = null;
				OnDisconnect(new OnDisconnectEventArgs(URL));
				System.Diagnostics.Debug.WriteLine ($"Disconnected from {URL}");
			} else {
				System.Diagnostics.Debug.WriteLine ($"Unable to disconnect from {URL}");
			}
		}

		/// <summary>
		/// Closes the Vortex object and as a consequence all the 'DataReaders' and 'DataWriters'
		/// that belong to it.
		/// </summary>
		public async Task Close()
		{
			System.Diagnostics.Debug.WriteLine ($"Closing {URL}");
			if (NOT_CLOSED == Interlocked.CompareExchange (ref _isClosed, CLOSED, NOT_CLOSED)) {
				await Disconnect ();
				OnClose (new OnCloseEventArgs(URL));
				System.Diagnostics.Debug.WriteLine ($"Closed {URL}");
			} else {
				System.Diagnostics.Debug.WriteLine ($"Unable to close {URL}");
			}
		}


		/// <summary>
		/// Creates a new data writer.
		/// </summary>
		/// <returns>The newly created DataWriter</returns>
		/// <param name="topic">The topic that will be written by the DataWriter</param>
		public async Task<DataWriter<T>> CreateDataWriter<T>(Topic topic) 
		{
			return await CreateDataWriter<T> (topic, this.DefaultWriterQos);
		}

		/// <summary>
		/// Creates a new data writer.
		/// </summary>
		/// <returns>The newly created DataWriter</returns>
		/// <param name="topic">The topic that will be written by the DataWriter</param>
		/// <param name="qos">A non empty list of QoS policies for the DataWriter</param>
		public async Task<DataWriter<T>> CreateDataWriter<T>(Topic topic, List<QosPolicy> qos) 
		{
			if (IsConnected) {
				List<QosPolicy> vqos = (qos == null) || (qos.Count == 0) ? DefaultWriterQos : qos; 
				var ws = await _ctrl.CreateWriterAsync (Domain, topic.Name, vqos);
				var dw = new DataWriterImpl<T> (ws);
				OnNewDataWriter (new OnNewDataWriterEventArgs (dw));
				return dw;
			} else {
				throw new VortexAPIException ("Can not create a data writer when not connected to Vortex.");
			}
		}

		/// <summary>
		/// Creates a new data reader.
		/// </summary>
		/// <returns>The newly created DataReader</returns>
		/// <param name="topic">The topic that will be read by the DataReader</param>
		public async Task<DataReader<T>> CreateDataReader<T>(Topic topic) 
		{
			return await CreateDataReader<T> (topic, DefaultReaderQos);
		}

		/// <summary>
		/// Creates a new data reader.
		/// </summary>
		/// <returns>The newly created DataReader</returns>
		/// <param name="topic">The topic that will be read by the DataReader</param>
		/// <param name="qos">A non-empty list of QoS policies for the DataReader.</param>
		public async Task<DataReader<T>> CreateDataReader<T>(Topic topic, List<QosPolicy> qos) 
		{
			if (IsConnected) {
				var vqos = (qos == null) || (qos.Count == 0) ? DefaultReaderQos : qos;				
				var ws = await _ctrl.CreateReaderAsync (Domain, topic.Name, vqos);
				var dr = new DataReaderImpl<T> (ws);
				OnNewDataReader (new OnNewDataReaderEventArgs (dr));
				return dr;
			} else {
				throw new VortexAPIException ("Can not create a data reader when not connected to Vortex.");
			}
		}

		/// <summary>
		/// Create a new Topic
		/// </summary>
		/// <returns>The newly created Topic.</returns>
		/// <param name="name">The name of the new topic.</param>
		public async Task<Topic> CreateTopic<T>(string name) 
		{
			return await CreateTopic<T> (name, DefaultTopicQos);
		}
			
		/// <summary>
		/// Create a new Topic
		/// </summary>
		/// <returns>The newly created Topic.</returns>
		/// <param name="name">The name of the new topic.</param>
		public async Task<Topic> CreateTopic<T>(string name, List<QosPolicy> qos) 
		{
			// This is the default for flexy" types
			var tt = "org.omg.dds.types.JSONTopicType";
			var trt = "org::omg::dds::types::JSONTopicType";

			var vqos = (qos == null) || (qos.Count == 0) ? DefaultTopicQos : qos;

			if (typeof(ITopicType).IsAssignableFrom (typeof(T))) {
				// If this is a CDR topic type then we need to do things differently.
				tt = typeof(T).FullName;
				trt = tt.Replace (".", "::"); 
			} 				

			return await CreateTopic<T> (name, tt, trt, vqos); 
		}

		/// <summary>
		/// Create a new Topic
		/// </summary>
		/// <returns>The newly created Topic.</returns>
		/// <param name="name">The name of the new topic.</param>
		/// <param name="type">The type to be used by the server for creating the topic.</param>
		/// <param name="typeRegistrationType">The type to register the topic with Vortex.</param>
		/// <param name="qos">Possibly empty list of Qos policies for the Topic.</param>
		public async Task<Topic> CreateTopic<T>(string name, string tt, string trt, List<QosPolicy> qos) 
		{
			if (IsConnected) {
				var rqos = (qos == null) || (qos.Count == 0) ? DefaultTopicQos : qos;
				await _ctrl.CreateTopicAsync (Domain, name, tt, trt, rqos);
				var topic = new TopicImpl (Domain, name, tt, qos);
				OnNewTopic (new OnNewTopicEventArgs (topic));
				return topic;
			} else {
				throw new VortexAPIException ("Can not create a topic when not connected to Vortex.");
			}
		}

		protected virtual void OnConnect(OnConnectEventArgs args) {
			SafeRaiseEvent (RaiseConnectEvent, args);
		}

		protected virtual void OnDisconnect(OnDisconnectEventArgs args) {
			SafeRaiseEvent (RaiseDisconnectEvent, args);
		}

		protected virtual void OnClose(OnCloseEventArgs args) {
			SafeRaiseEvent (RaiseCloseEvent, args);
		}

		protected virtual void OnNewDataWriter(OnNewDataWriterEventArgs args) {
			SafeRaiseEvent (RaiseNewDataWriterEvent, args);
		}

		protected virtual void OnNewDataReader(OnNewDataReaderEventArgs args) {
			SafeRaiseEvent (RaiseNewDataReaderEvent, args);
		}

		protected virtual void OnNewTopic(OnNewTopicEventArgs args) {
			SafeRaiseEvent (RaiseNewTopicEvent, args);
		}

		private void SafeRaiseEvent<T>(EventHandler<T> handler, T args) {
			// Make a temporary copy of the event to avoid possibility of 
			// a race condition if the last subscriber unsubscribes 
			// immediately after the null check and before the event is raised.
			EventHandler<T> copyOfHandler = handler;

			// Event will be null if there are no subscribers 
			if (copyOfHandler != null)
			{
				// Use the () operator to raise the event.
				copyOfHandler(this, args);
			}
		}
	}

	public class OnConnectEventArgs : EventArgs {
		public string URL { get; private set; }

		public OnConnectEventArgs(String url)
		{
			URL = url;
		}
	}

	public class OnDisconnectEventArgs : EventArgs {
		public string URL { get; private set; }

		public OnDisconnectEventArgs(String url)
		{
			URL = url;
		}
	}

	public class OnNewDataWriterEventArgs : EventArgs {
		public DataWriter NewDataWriter { get; private set; }

		public OnNewDataWriterEventArgs (DataWriter dataWriter)
		{
			this.NewDataWriter = dataWriter;
		}
	}

	public class OnNewDataReaderEventArgs : EventArgs {
		public DataReader NewDataReader { get; private set; }

		public OnNewDataReaderEventArgs (DataReader dataReader)
		{
			this.NewDataReader = dataReader;
		}
	}

	public class OnNewTopicEventArgs : EventArgs {
		public Topic NewTopic { get; private set; }

		public OnNewTopicEventArgs (Topic topic)
		{
			this.NewTopic = topic;
		}
		
	}

	public class OnCloseEventArgs : EventArgs {
		public String URL { get; private set; }

		public OnCloseEventArgs (string uRL)
		{
			this.URL = uRL;
		}	
	}
}


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

using com.prismtech.vortex.cs.api.qos;
using com.prismtech.vortex.web.proto;

namespace com.prismtech.vortex.web.cs.api
{

    interface Vortex
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
		Task Connect(string srv, string authToken);

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

	public class VortexImpl : Vortex
	{
		/// <summary>
		/// Create a logger for use in this class
		/// </summary>
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
		public VortexImpl (int vortexDomain)
		{
			if (log.IsDebugEnabled) log.Debug ("Creating Vortex for domain " + Domain);

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
		public async Task Connect(string srv, string authToken) 
		{
			if (log.IsDebugEnabled) log.Debug ("Connecting to " + srv);
			
			if (!IsClosed && NOT_CONNECTED == Interlocked.CompareExchange (ref _isConnected, CONNECTED, NOT_CONNECTED)) {
				URL = srv;
				_ctrl = new SControlLink ();
				await _ctrl.ConnectAsync (srv, authToken);
				OnConnect (new OnConnectEventArgs(URL));
				if (log.IsDebugEnabled) log.Debug ("Connected to " + srv);
			} else {
				if (log.IsWarnEnabled) log.Warn ("Unable to connect to " + srv);
			}
		}

		/// <summary>
		/// Disconnects, without closing. Upon re-connection all existing subscriptions
		/// and publications will be restablished.
		/// </summary>
		public async Task Disconnect() 
		{
			if (log.IsDebugEnabled) log.Debug ("Disconnecting from " + URL);

			if (CONNECTED == Interlocked.CompareExchange (ref _isConnected, NOT_CONNECTED, CONNECTED)) 
			{
				await _ctrl.DisconnectAsync ();
				_ctrl = null;
				OnDisconnect(new OnDisconnectEventArgs(URL));
				if (log.IsDebugEnabled) log.Debug ("Disconnected from " + URL);
			} else {
				if (log.IsWarnEnabled) log.Warn ("Unable to disconnect from " + URL);
			}
		}

		/// <summary>
		/// Closes the Vortex object and as a consequence all the 'DataReaders' and 'DataWriters'
		/// that belong to it.
		/// </summary>
		public async Task Close()
		{
			if (log.IsDebugEnabled) log.Debug ("Closing " + URL);
			if (NOT_CLOSED == Interlocked.CompareExchange (ref _isClosed, CLOSED, NOT_CLOSED)) {
				await Disconnect ();
				OnClose (new OnCloseEventArgs(URL));
				if (log.IsDebugEnabled)
					log.Debug ("Closed " + URL);
			} else {
				if (log.IsWarnEnabled) log.Warn ("Unable to close " + URL);
			}
		}



		/// <summary>
		/// Creates a new data writer.
		/// </summary>
		/// <returns>The newly created DataWriter</returns>
		/// <param name="topic">The topic that will be written by the DataWriter</param>
		/// <param name="qos">Possibly empty list of QoS policies for the DataWriter</param>
		public async Task<DataWriter<T>> CreateDataWriter<T>(Topic topic, List<QosPolicy> qos) 
		{
			if (IsConnected) {
				var qosJson = (qos != null) ? JsonConvert.SerializeObject (qos) : "";
				var ws = await _ctrl.CreateWriterAsync (Domain, topic.Name, qosJson);
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
		/// <param name="qos">Possibly empty list of QoS policies for the DataReader.</param>
		public async Task<DataReader<T>> CreateDataReader<T>(Topic topic, List<QosPolicy> qos) 
		{
			if (IsConnected) {
				var qosJson = (qos != null) ? JsonConvert.SerializeObject (qos) : "";
				var ws = await _ctrl.CreateReaderAsync (Domain, topic.Name, qosJson);
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
		/// <param name="type">The type to be used by the server for creating the topic.</param>
		/// <param name="typeRegistrationType">The type to register the topic with Vortex.</param>
		/// <param name="qos">Possibly empty list of Qos policies for the Topic.</param>
		public async Task<Topic> CreateTopic<T>(string name, string type, string typeRegistrationType, List<QosPolicy> qos) 
		{
			if (IsConnected) {
				var qosJson = (qos != null) ? JsonConvert.SerializeObject (qos) : "";
				await _ctrl.CreateTopicAsync (Domain, name, type, typeRegistrationType, qosJson);
				var topic = new TopicImpl (Domain, name, type, qos.ToArray());
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


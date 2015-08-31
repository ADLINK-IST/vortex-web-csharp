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

using com.prismtech.vortex.cs.api.qos;

namespace com.prismtech.vortex.web.cs.api
{
	public interface Topic {
		int Domain { get; }
		string Name { get; }
		string Type { get; }
		QosPolicy[] QoS { get; }


	}

	public class TopicImpl : Topic
	{
		public int Domain {
			get;
			private set;
		}

		public string Name {
			get;
			private set;
		}

		public string Type {
			get;
			private set;
		}

		public QosPolicy[] QoS {
			get;
			private set;
		}

		public TopicImpl (int domain, string name, string type, QosPolicy[] qos)
		{
			Domain = domain;
			Name = name;
			Type = type;
			QoS = qos;
		}
	}
}


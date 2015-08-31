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

namespace com.prismtech.vortex.web.proto 
{
	
	public class CreateTopic {
		public readonly Header h;
		public readonly TopicInfo b;

		public CreateTopic (TopicInfo b, int sn) {
			h = new Header (CommandId.Create, EntityKind.Topic, sn);
			this.b = b;
		}
	}

	public class CreateReader {
		public readonly Header h;
		public readonly EndpointInfo b;

		public CreateReader (EndpointInfo ei, int sn) {
			h = new Header (CommandId.Create, EntityKind.DataReader, sn);
			b = ei;
		}
	}

	public class CreateWriter {
		public readonly Header h;
		public readonly EndpointInfo b;

		public CreateWriter (EndpointInfo ei, int sn) {
			h = new Header (CommandId.Create, EntityKind.DataWriter, sn);
			b = ei;
		}
	}

	public class CommandReply {
		public readonly Header h;
		public readonly ReplyBody b;

		public CommandReply (Header h, ReplyBody b) {
			this.h = h;
			this.b = b;
		}
	}

//	public class CreateEntity : Command {
//		public readonly string t;  // topic
//		public readonly string q;  // qos
//		public readonly string id; // entity id
//
//		CreateEntity (EntityKind kind, string t, string q, string id, int sn) : base (CommandId.Create, kind, sn) {
//			this.t = t;
//			this.q = q;
//			this.id = id;
//		}
//
//		public static CreateEntity CreateTopic (string t, string q, string id, int sn) {
//			return new CreateEntity (EntityKind.Topic, t, q, id, sn);
//		}
//
//		public static CreateEntity CreateReader (string t, string q, string id, int sn) {
//			return new CreateEntity (EntityKind.DataReader, t, q, id, sn);
//		}
//
//		public static CreateEntity CreateWriter (string t, string q, string id, int sn) {
//			return new CreateEntity (EntityKind.DataWriter, t, q, id, sn);
//		}
//	}
//

}
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

	public enum CommandId {
		OK 			= 0,
		Error 		= 1,
		Create		= 2,
		Connect		= 3,
		Disconnect	= 4,
		Close		= 5,
		Write		= 6,
		Log			= 7
	}

	public enum EntityKind {
		Topic   	= 0,
		DataReader	= 1,
		DataWriter	= 2,
		Runtime		= 3,
		Worker		= 4
	}

	/**
	 * Each protocol message starts with the header that specifies the 
	 * command and the entity for which the command has to be executed.
	 * As an example a Create command can be targeting a DR or DW entity.  
     */ 
	public class Header
	{	
		public readonly CommandId cid;
		public readonly EntityKind ek;
		public readonly int sn;

		public Header(CommandId cid, EntityKind kind, int sn) {
			this.cid = cid;
			this.ek = kind;
			this.sn = sn;
		}
	}
//
//	public class Command {
//		public readonly Header h;
//
//		public Command (CommandId cid, EntityKind kind, int sn) {
//			this.h = new Header (cid, kind, sn);
//		}
//	}
//
//	public class Reply : Command {		
//		public readonly CommandId cid;
//		public readonly EntityKind ek;
//
//		public Reply (Header h, CommandId cid, EntityKind ek) : base (h.cid, h.ek, h.sn)
//		{
//			this.cid = cid;
//			this.ek = ek;
//		}
//	}

	public class TopicInfo {
		public readonly int did;
		public readonly string tn;
		public readonly string tt;
		public readonly string trt;
		public readonly string qos;

		public TopicInfo(int did, string tn, string tt, string trt, string qos) {
			this.did = did;
			this.tn = tn;
			this.tt = tt;
			this.trt = trt;
			this.qos = qos;
		}
	}
		
	/**
	 * Information for a Reader/Writer endpoint.
	 */
	public class EndpointInfo {
		public readonly int did;
		public readonly string tn;
		public readonly string qos;

		public EndpointInfo(int did, string tn, string qos) {
			this.did = did;
			this.tn = tn;
			this.qos = qos;
		}
	}

	public class ReplyBody {
		public readonly string eid;
		public readonly string msg;

		public ReplyBody (string eid, string msg) {
			this.eid = eid;
			this.msg = msg;
		}	

	}

}

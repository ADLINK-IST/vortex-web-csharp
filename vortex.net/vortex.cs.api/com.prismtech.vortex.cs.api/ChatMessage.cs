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
using com.prismtech.vortex.web.proto;
using com.prismtech.vortex.web.cs.api;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using WebSocketSharp;

namespace com.prismtech.vortex.cs.api
{
	class ChatMessage {
		public string user;
		public string msg;

		public ChatMessage(string usr, string msg) {
			this.user = usr;
			this.msg = msg;
		}

		public override string ToString ()
		{
			return string.Format ("{0}> {1}", user, msg);
		}
	}
}

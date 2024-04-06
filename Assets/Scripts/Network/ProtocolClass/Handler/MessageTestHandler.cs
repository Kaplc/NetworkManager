using System;
using Network;
using Network.Base;
using UnityEngine;

namespace Network.ProtocolClass.Handler
{
	public class MessageTestHandler: BaseHandler
	{
		public MessageTest Message=> message as MessageTest;

		public override void Handle()
		{

		}
	}
}

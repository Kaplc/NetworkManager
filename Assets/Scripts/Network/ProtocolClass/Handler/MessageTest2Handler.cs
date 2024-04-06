using System;
using Network;
using Network.Base;
using UnityEngine;

namespace Network.ProtocolClass.Handler
{
	public class MessageTest2Handler: BaseHandler
	{
		public MessageTest2 Message=> message as MessageTest2;

		public override void Handle()
		{

		}
	}
}

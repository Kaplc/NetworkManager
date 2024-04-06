using System;
using Network;
using Network.Base;
using UnityEngine;

namespace Network.ProtocolClass.Handler
{
	public class TextMessageHandler: BaseHandler
	{
		public TextMessage Message=> message as TextMessage;

		public override void Handle()
		{

		}
	}
}

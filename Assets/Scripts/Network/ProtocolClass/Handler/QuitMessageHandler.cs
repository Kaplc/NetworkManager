using System;
using Network;
using Network.Base;
using UnityEngine;

namespace Network.ProtocolClass.Handler
{
	public class QuitMessageHandler: BaseHandler
	{
		public QuitMessage Message=> message as QuitMessage;

		public override void Handle()
		{

		}
	}
}

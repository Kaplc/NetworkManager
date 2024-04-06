using System;
using Network;
using Network.Base;
using UnityEngine;

namespace Network.ProtocolClass.Handler
{
	public class HeartMessageHandler: BaseHandler
	{
		public HeartMessage Message=> message as HeartMessage;

		public override void Handle()
		{
			Debug.Log("Heart");
		}
	}
}

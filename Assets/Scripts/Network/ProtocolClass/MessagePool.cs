using System;
using System.Collections.Generic;
using Network.Base;
using Network.ProtocolClass.Handler;
using UnityEngine;

namespace Network.ProtocolClass
{
	public class MessagePool
	{
		private Dictionary<int, Type> messagePool = new Dictionary<int, Type>();
		private Dictionary<int, Type> handlerPool = new Dictionary<int, Type>();

		public MessagePool()
		{
			// register message
			RegisterMessage(1, typeof(MessageTest), typeof(MessageTestHandler));
			RegisterMessage(2, typeof(MessageTest2), typeof(MessageTest2Handler));
			RegisterMessage(500, typeof(QuitMessage), typeof(QuitMessageHandler));
			RegisterMessage(123, typeof(TextMessage), typeof(TextMessageHandler));
			RegisterMessage(99, typeof(HeartMessage), typeof(HeartMessageHandler));
		}

		private void RegisterMessage(int id, Type messageType, Type handlerType)
		{
			messagePool.Add(id, messageType);
			handlerPool.Add(id, handlerType);
		}

		public BaseMessage GetMessage(int id)
		{
			if (messagePool.ContainsKey(id) == false)
			{
				Debug.Log("not found message id: " + id);
				return null;
			}

			return Activator.CreateInstance(messagePool[id]) as BaseMessage;
		}

		public BaseHandler GetHandler(int id)
		{
			if (handlerPool.ContainsKey(id) == false)
			{
				Debug.Log("not found handler id: " + id);
				return null;
			}

			return Activator.CreateInstance(handlerPool[id]) as BaseHandler;
		}
	}
}

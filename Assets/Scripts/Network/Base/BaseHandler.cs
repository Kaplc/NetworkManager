using UnityEditor;

namespace Network.Base
{
    public abstract class BaseHandler
    {
        public BaseMessage message;
        
        public abstract void Handle();
    }
}
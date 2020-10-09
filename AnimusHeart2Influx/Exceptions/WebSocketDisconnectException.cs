using System;
using System.Runtime.Serialization;

namespace AnimusHeart2Influx.Exceptions
{
    [Serializable]
    public class WebSocketDisconnectException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public WebSocketDisconnectException()
        {
        }

        public WebSocketDisconnectException(string message) : base(message)
        {
        }

        public WebSocketDisconnectException(string message, Exception inner) : base(message, inner)
        {
        }

        protected WebSocketDisconnectException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
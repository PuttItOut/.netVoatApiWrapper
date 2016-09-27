using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace VoatApiWrapper.Framework
{
    public class Callback<T> 
    {
        private Callback() { }

        public static Callback<T> Create(EventHandler<T> handler, TimeSpan delay, CancellationToken? token = null)
        {
            var callback = new Callback<T>();
            callback.Handler = handler;
            callback.Delay = delay;
            callback.CancellationToken = (token.HasValue ? token.Value : CancellationToken.None);
            return callback;
        }

        public TimeSpan Delay { get; set; }

        public EventHandler<T> Handler { get; set; }

        public CancellationToken CancellationToken { get; private set; }

        internal HttpMethod Method { get; set; }

        internal string Endpoint { get; set; }

        internal object Body { get; set; } = null;

        internal object QueryStringPairs { get; set; } = null;

        public Task Task { get; internal set; }
    }
}

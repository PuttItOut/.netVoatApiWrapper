using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VoatApiWrapper.Models;

namespace VoatApiWrapper.Framework
{

    public class ApiResponseCallback<T> : Callback<ApiResponse<T>>
    {
        public ApiResponseCallback(EventHandler<ApiResponse<T>> handler, TimeSpan delay, CancellationToken? token = null)
            : base(handler, delay, token)
        {
        }
    }

    public class Callback<T> 
    {
        public Callback(EventHandler<T> handler, TimeSpan delay, CancellationToken? token = null)
        {
            Handler = handler;
            Delay = delay;
            CancellationToken = (token.HasValue ? token.Value : CancellationToken.None);
        }

        //public static Callback<T> Create(EventHandler<T> handler, TimeSpan delay, CancellationToken? token = null)
        //{
        //    var callback = new Callback<T>();
        //    callback.Handler = handler;
        //    callback.Delay = delay;
        //    callback.CancellationToken = (token.HasValue ? token.Value : CancellationToken.None);
        //    return callback;
        //}

        public TimeSpan Delay { get; set; }

        public EventHandler<T> Handler { get; set; }

        public CancellationToken CancellationToken { get; protected set; }

        internal HttpMethod Method { get; set; }

        internal string Endpoint { get; set; }

        internal object Body { get; set; } = null;

        internal object QueryStringPairs { get; set; } = null;

        public Task Task { get; internal set; }
    }
}

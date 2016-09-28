using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;

namespace VoatApiWrapper
{
    public interface ITokenStore
    {
        AuthToken Find(string id);

        void Store(string id, AuthToken token);

        void Purge(string id);
    }
    /// <summary>
    /// Stores to IsolatedStorage locations
    /// </summary>
    public class IsolatedStorageTokenStore : ITokenStore
    {
        protected virtual IsolatedStorageFile GetStore
        {
            get
            {
                return IsolatedStorageFile.GetMachineStoreForAssembly();
            }
        }

        protected virtual string FileName(string id)
        {
            return String.Format("{0}.token", id).ToLower();
        }

        public virtual AuthToken Find(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return null;
            }

            string fileName = FileName(id);

            using (IsolatedStorageFile io = GetStore)
            {
                if (io.FileExists(fileName))
                {
                    using (StreamReader sr = new StreamReader(io.OpenFile(fileName, FileMode.Open)))
                    {
                        string jsonPayload = sr.ReadToEnd();
                        AuthToken token = JsonConvert.DeserializeObject<AuthToken>(jsonPayload);
                        return token;
                    }
                }
            }

            return null;
        }

        public virtual void Store(string id, AuthToken token)
        {
            if (String.IsNullOrEmpty(id) || token == null || !token.IsValid)
            {
                return;
            }

            string fileName = FileName(id);

            using (IsolatedStorageFile io = GetStore)
            {
                using (StreamWriter sw = new StreamWriter(io.OpenFile(fileName, FileMode.Create)))
                {
                    sw.Write(JsonConvert.SerializeObject(token));
                }
            }
        }

        public virtual void Purge(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return;
            }

            string fileName = FileName(id);

            using (IsolatedStorageFile io = GetStore)
            {
                if (io.FileExists(fileName))
                {
                    io.DeleteFile(fileName);
                }
            }
        }
    }
    /// <summary>
    /// Disables all token storage
    /// </summary>
    public class DisabledTokenStore : ITokenStore
    {
        public AuthToken Find(string id)
        {
            return null;
        }

        public void Store(string id, AuthToken token)
        {
            /*no-op*/
        }

        public void Purge(string id)
        {
            /*no-op*/
        }
    }
    /// <summary>
    /// Stores tokens in in-memory dictionary
    /// </summary>
    public class MemoryTokenStore : ITokenStore
    {
        private Dictionary<string, AuthToken> _tokens = new Dictionary<string, AuthToken>();

        public AuthToken Find(string id)
        {
            if (_tokens.ContainsKey(id))
            {
                return _tokens[id];
            }
            return null;
        }

        public void Store(string id, AuthToken token)
        {
            _tokens[id] = token;
        }

        public void Purge(string id)
        {
            if (_tokens.ContainsKey(id))
            {
                _tokens.Remove(id);
            }
        }
    }
}

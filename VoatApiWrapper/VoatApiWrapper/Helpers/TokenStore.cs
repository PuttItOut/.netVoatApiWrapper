using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoatApiWrapper {

    public interface ITokenStore {

        AuthToken Find(string id);

        void Store(string id, AuthToken token);

        void Purge(string id);

    }
    
    public class IsolatedStorageTokenStore : ITokenStore {

        protected virtual IsolatedStorageFile GetStore {
            get {
                return IsolatedStorageFile.GetMachineStoreForAssembly();
            }
        }

        protected virtual string FileName(string id) {

            return String.Format("{0}.token", id).ToLower();
        
        }

        public virtual AuthToken Find(string id) {

            if (String.IsNullOrEmpty(id)) {
                return null;
            }

            string fileName = FileName(id);

            using (IsolatedStorageFile io = GetStore) {
                
                if (io.FileExists(fileName)) {
                
                    using (StreamReader sr = new StreamReader(io.OpenFile(fileName, FileMode.Open))) {
                    
                        string jsonPayload = sr.ReadToEnd();
                        AuthToken token = JsonConvert.DeserializeObject<AuthToken>(jsonPayload);
                        return token;
                    
                    }

                }
               
            }

            return null;
        }

        public virtual void Store(string id, AuthToken token) {

            if (String.IsNullOrEmpty(id) || token == null || !token.IsValid) {
                return;
            }

            string fileName = FileName(id);

            using (IsolatedStorageFile io = GetStore) {

                using (StreamWriter sw = new StreamWriter(io.OpenFile(fileName, FileMode.Create))) {
                    sw.Write(JsonConvert.SerializeObject(token));
                }

            }

        }

        public virtual void Purge(string id) {

            if (String.IsNullOrEmpty(id)) {
                return;
            }

            string fileName = FileName(id);

            using (IsolatedStorageFile io = GetStore) {

                if (io.FileExists(fileName)) {
                    io.DeleteFile(fileName);
                }

            }
        }
    }

    public class DummyTokenStore : ITokenStore {

        public AuthToken Find(string id) {
            return null;
        }

        public void Store(string id, AuthToken token) {
            /*no-op*/
        }

        public void Purge(string id) {
            /*no-op*/
        }
    }

}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoatApiWrapper {

    public static class Helper {

        public static string ReadStream(Stream stream) {
            string result = "";
            if (stream != null && stream.CanRead) {
                using (var sr = new StreamReader(stream)) {
                    result = sr.ReadToEnd();
                }
            }
            return result;
        }
    }

}

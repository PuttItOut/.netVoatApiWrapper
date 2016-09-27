﻿using System.IO;
using System.Net;

namespace VoatApiWrapper
{
    public static class Helper
    {
        public static ApiResponse NoServerResponse
        {
            get
            {
                return new ApiResponse()
                {
                    StatusCode = HttpStatusCode.ServiceUnavailable,
                    Success = false,
                    Error = new ApiResponse.ErrorInfo() { Type = "NoResponse", Message = "No response was received from the server." }
                };
            }
        }

        public static string ReadStream(Stream stream)
        {
            string result = "";
            if (stream != null && stream.CanRead)
            {
                using (var sr = new StreamReader(stream))
                {
                    result = sr.ReadToEnd();
                }
            }
            return result;
        }
    }
}

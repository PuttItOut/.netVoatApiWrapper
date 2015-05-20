using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoatApiWrapper;

namespace Harness {
    class Program {
        static void Main(string[] args) {


            //********************************************************************************************************
            //This is just a simple console application to demonstrate how to use the .netVoatApiWrapper. In your
            //project you will simply compile the VoatApiWrapper (class library) project (or include the source 
            //project in your solution) and consume it with your Tool/App.
            //********************************************************************************************************

            //Set your api key and endpoint location
            ApiInfo.ApiPublicKey = "[Your Api Key Here]";
            ApiInfo.BaseEndpoint = "[API Endpoint URL Here]";

            //Authenticate a user
            var authResult = ApiAuthenticator.Instance.Login("username", "password");
            if (!authResult.Success) {
                Console.WriteLine("{0}: {1}", authResult.Error.Type, authResult.Error.Message);
                Console.WriteLine("Press <Enter> to Exit");
                Console.ReadLine();
                return;
            }

            //Create the Proxy Object
            VoatApiProxy api = new VoatApiProxy();

            //All API Responses will be returned in this form (ApiResponse). You can check the .Success property or the .StatusCode to determine if operation succeeded.
            ApiResponse response;

            //Set api defaults example (there isn't big reasons to change the defaults as it provides the safest way to access the api and handles any ApiThrottleLimit exceptions.
            api.EnableMultiThreading = false;
            api.RetryOnThrottleLimit = true;
            api.WaitTimeOnThrottleLimit = 1500;
            api.MaxThrottleRetryCount = 1;


            //Create Discussion
            response = api.PostDiscussion("PuttItOutPlease", "This is a post using .NET C# Voat API Wrapper", "Title says it all - Do you like wrappers?");
            if (response.Success) {
                Console.WriteLine(response.Data.ToString());
            } else {
                Console.WriteLine("{0}: {1}", response.Error.Type, response.Error.Message);
            }


            //Retrieve Subverse Submissions 
            response = api.GetSubmissionsBySubverse("PuttItOutPlease", new { sort = "top", count = 5, index = 0, span = "week" });
            if (response.Success) {
                Console.WriteLine(response.Data[0].ToString());
            } else {
                Console.WriteLine("{0}: {1}", response.Error.Type, response.Error.Message);
            }


            //Test logout 
            ApiAuthenticator.Instance.Logout();

            response = api.GetUserComments("DerpyGuy");
            if (response.Success) {
                Console.WriteLine(response.Data[0].ToString());
            } else {
                Console.WriteLine("{0}: {1}", response.Error.Type, response.Error.Message);
            }

            //Keep console window up
            Console.WriteLine("Press <Enter> to Exit");
            Console.ReadLine();

        }
    }
}


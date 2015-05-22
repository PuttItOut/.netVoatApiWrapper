#.netVoatApiWrapper

##.NET Voat API Wrapper


This project is a simple lightweight wrapper for the Voat API version 1 for use with .NET clients.


It is written in C# and abstracts the HTTP request and parsing details needed for communicating with the Voat REST API.

#Getting Started

1. Add the VoatApiWrapper project to your existing solution or add a references to the VoatApiWrapper.dll (if compiled seperately).

2. Provide setup information in code.
~~~
//Set your api key and endpoint location
ApiInfo.ApiPublicKey = "[Your Api Key Here]";
ApiInfo.BaseEndpoint = "[API Endpoint URL Here]"; //This value is the root of the site hosting the API.
~~~

2. Perform a login using the ApiAuthenticator Object. All requests will use this objects state to provide Authentication information when making API calls.

~~~
//Authenticate a user using the ApiAuthenticator Object
var authResult = ApiAuthenticator.Instance.Login("<username>", "<password>");
if (!authResult.Success) {
  //Authentication Failed, do something about it
  Console.WriteLine("{0}: {1}", authResult.Error.Type, authResult.Error.Message);
} 
~~~

3. Create the proxy gateway object that calls the API.
~~~
//Create the Proxy Object
VoatApiProxy api = new VoatApiProxy();
~~~

4. Use the Proxy Object to make API calls.

~~~
 //Retrieve Subverse Submissions 
ApiResponse response = api.GetSubmissionsBySubverse("<subverse>", 
      new { sort = "top", count = 5, index = 0, span = "week" });
if (response.Success) {
    Console.WriteLine(response.Data[0].ToString());
} else {
    Console.WriteLine("{0}: {1}", response.Error.Type, response.Error.Message);
}

~~~

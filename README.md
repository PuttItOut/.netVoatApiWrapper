#.NET Voat API Wrapper

This project is a simple lightweight wrapper for the Voat API (version 1) for use with .NET clients.


It is written in C# and abstracts the HTTP request and parsing details needed for communicating with the Voat REST API.

##Getting Started

Add the VoatApiWrapper project to your existing solution or add a reference to the VoatApiWrapper.dll (if compiled seperately).

Provide setup information in code.
~~~
//Set your api key and endpoint location
ApiInfo.ApiPublicKey = "[Your Api Key Here]";
ApiInfo.BaseEndpoint = "[API Endpoint URL Here]"; //This value is the root of the site hosting the API.
~~~

Perform a login using the ApiAuthenticator object. All requests will use this objects state to provide Authentication information when making API calls.

~~~
//Authenticate a user using the ApiAuthenticator Object
var authResult = ApiAuthenticator.Instance.Login("<username>", "<password>");

if (!authResult.Success) {
  //Authentication Failed, do something about it
  Console.WriteLine("{0}: {1}", authResult.Error.Type, authResult.Error.Message);
} 
~~~

Create the proxy gateway object that calls the API.
~~~
//Create the Proxy Object
VoatApiProxy api = new VoatApiProxy();
~~~

Use the Proxy Object to make API calls.
~~~
 //Retrieve Subverse Submissions 
ApiResponse response = api.GetSubmissionsBySubverse("<subverse>", new { sort = "top", span = "week" });

if (response.Success) {
    Console.WriteLine(response.Data[0].ToString());
} else {
    Console.WriteLine("{0}: {1}", response.Error.Type, response.Error.Message);
}
~~~

###The ApiResponse Object

All API calls using the VoatApiProxy returns the same object.

~~~
public class ApiResponse {

  public HttpStatusCode StatusCode { get; set; }
  
  public bool Success { get; set; }
  
  public dynamic Data { get; set; }
  
  public ErrorInfo Error { get; set; }
  
  public class ErrorInfo {
  
      public string Type { get; set; }
      
      public string Message { get; set; }
      
  }

}
~~~

All data the API returns will be contained in a dynamic type named Data. What the ApiResponse.Data will contain will vary depending on what API method is called. Please refer to the API Help documentation to understand the structure and contents of the various API calls.


Here is an example of retrieving submissions from /v/all 
~~~
ApiResponse response = api.GetSubmissionsAll(new { count = 15});

if (response.Success) {

  foreach (var submission in response.Data) {
  
    Console.WriteLine("Retrieved: Submission: {0}, Title: {1}, Subverse: {2}",
      submission.id.Value.ToString(),
      submission.title.Value,
      submission.subverse.Value);
      
  }
  
}
~~~

###ApiResponse Failures

If the API call fails you can easily asses this by checking the ApiResponse.Success property.
~~~
ApiResponse response = api.GetComment(...);

if (!response.Success){
  //the call failed
}

~~~

If an API call fails you can check the ApiResponse.Error property for information returned from the API.

~~~
ApiResponse response = api.GetComment(...);

if (!response.Success){
  Console.WriteLine("{0}: {1}", response.Error.Type, response.Error.Message);
}

~~~

Good luck and more updates will be coming soon such as serializing Authentication Tokens.

# Filesystem Identity Provider
Local OpenID token provider, hosted in file system, and attachable to any .NET Core (3.1) Web API solution

## Concept Diagram
![Filesystem Identity Provider](https://user-images.githubusercontent.com/6631390/182035145-9bd01e6c-4570-4e72-a296-98719dabf562.jpg)

## Basic .NET Core Web Api Middleware Framework
ASP.NET Core introduced a new concept called Middleware. A middleware is a class which is executed on every request in ASP.NET Core application. In the classic ASP.NET, HttpHandlers and HttpModules were part of request pipeline. Middleware is similar to HttpHandlers and HttpModules where both needs to be configured and executed in each request.

Typically, there will be multiple middleware in ASP.NET Core web application. It can be either framework provided middleware, added via NuGet or your own custom middleware. We can set the order of middleware execution in the request pipeline. Each middleware adds or modifies http request and optionally passes control to the next middleware component. The following figure illustrates the execution of middleware components.

![Dotnet Core Middleware Pipeline](https://user-images.githubusercontent.com/6631390/182035384-91ab32f2-8f0c-42f5-a069-7ff8d9c4187d.JPG)

Middlewares build the request pipeline. The following figure illustrates the ASP.NET Core request processing.

![Middleware Request Processing](https://user-images.githubusercontent.com/6631390/182035434-22b3119c-99a8-43ee-99e1-8f901bb0a878.JPG)

## Basic .NET Core Web Api Action Filters
Filters in .NET offer a great way to hook into the MVC action invocation pipeline. These filters to extract code that can be reused and make the actions cleaner and maintainable. There are some filters that are already provided by ASP.NET Core like the authorization filter, and there are the custom ones that we can create ourselves.

There are different filter types:

* **Authorization filters** – They run first to determine whether a user is authorized for the current request
* **Resource filters** – They run right after the authorization filters and are very useful for caching and performance
* **Action filters** – They run right before and after the action method execution
* **Exception filters** – They are used to handle exceptions before the response body is populated
* **Result filters** – They run before and after the execution of the action methods result.

## Use Middleware for Authentication & Authorization
Current projects makes use of .NET Core Middleware Framework for capturing all requests to a controller endpoint, and parse <b>context</b> object to extract information from request header (including <b>bearer token</b>)
Based on a file system storage file named <b>service.access.roles.json</b> that keeps the mapping between the endpoint routing path and the required role, access to the endpoint is permitted or denied.

.NET Core Middleware Framework is a good alternative to <b>action filters</b> and <b>controller policies</b> for authenticating and authorizing endpoint access

## References
* https://www.tutorialsteacher.com/core/aspnet-core-middleware
* https://docs.microsoft.com/en-us/aspnet/web-api/overview/security/authentication-and-authorization-in-aspnet-web-api
* https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-3.1
* https://docs.microsoft.com/en-us/aspnet/core/security/authorization/roles?view=aspnetcore-3.1
* https://docs.microsoft.com/en-us/aspnet/core/security/authorization/claims?view=aspnetcore-3.1

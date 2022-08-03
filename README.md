# Filesystem Identity Provider - PoC
Local OpenID token provider, hosted in file system, and attachable to any .NET Core (3.1) Web API solution

## Concept Diagram
![Filesystem Identity Provider](https://user-images.githubusercontent.com/6631390/182035145-9bd01e6c-4570-4e72-a296-98719dabf562.jpg)

## ASP.NET Core Middleware in a Nutshell
ASP.NET Core introduced a new concept called Middleware. A middleware is a class which is executed on every request in ASP.NET Core application. In the classic ASP.NET, HttpHandlers and HttpModules were part of request pipeline. Middleware is similar to HttpHandlers and HttpModules where both needs to be configured and executed in each request.

Typically, there will be multiple middleware in ASP.NET Core web application. It can be either framework provided middleware, added via NuGet or your own custom middleware. We can set the order of middleware execution in the request pipeline. Each middleware adds or modifies http request and optionally passes control to the next middleware component. The following figure illustrates the execution of middleware components.

![Dotnet Core Middleware Pipeline](https://user-images.githubusercontent.com/6631390/182035384-91ab32f2-8f0c-42f5-a069-7ff8d9c4187d.JPG)

Middlewares build the request pipeline. The following figure illustrates the ASP.NET Core request processing.

![Middleware Request Processing](https://user-images.githubusercontent.com/6631390/182035434-22b3119c-99a8-43ee-99e1-8f901bb0a878.JPG)

## ASP.NET Core Web Api Action Filters
Filters in .NET offer a great way to hook into the MVC action invocation pipeline. These filters to extract code that can be reused and make the actions cleaner and maintainable. There are some filters that are already provided by ASP.NET Core like the authorization filter, and there are the custom ones that we can create ourselves.

There are different filter types:

* **Authorization filters** – They run first to determine whether a user is authorized for the current request
* **Resource filters** – They run right after the authorization filters and are very useful for caching and performance
* **Action filters** – They run right before and after the action method execution
* **Exception filters** – They are used to handle exceptions before the response body is populated
* **Result filters** – They run before and after the execution of the action methods result.

The following picture shows the location of **action filter execution** in MVC request pipeline:
![action-filter-request-pipeline](https://user-images.githubusercontent.com/6631390/182193199-4ac4c165-45c2-4424-bfc6-80940707ac5c.JPG)

The following picture shows the order of invocation of various action filters
![Order-of-invocation](https://user-images.githubusercontent.com/6631390/182192112-4b8cfd35-c0a0-4166-83ab-de33fc94ef44.JPG)

## ASP.NET Policy-based Authorization
In ASP.NET Core, the policy-based authorization framework is designed to decouple authorization and application logic. Simply put, a policy is an entity devised as a *collection of requirements*, which themselves are conditions that the current user must meet. The simplest policy is that the user is authenticated, while a common requirement is that the user is associated with a given role. Another common requirement is for the user to have a particular claim or a particular claim with a particular value. In the most general terms, a requirement is an assertion about the user identity that attempts to access a method that holds true.

## Examples of Authentication & Authorization
Current projects comes with 2 examples of implementation:
1. Use ASP.NET Core Middleware
* makes use of .NET Core Middleware Framework for capturing all requests to LoanManager controller endpoints
* parse <b>context</b> object to extract information from request header (including <b>bearer token</b>)<br/>
* Based on a file system storage file named <b>service.access.roles.json</b> that keeps the mapping between the endpoint routing path and the required role, access to the endpoint is permitted or denied.<br/>

2. Use ASP.NET Core Action Filters
* makes use of .NET Core Action Filters to capture all requests to LoanManager controller endpoints
* parse <b>context</b> object to extract information from request header (including <b>bearer token</b>)
* same as before, the mappings between endpoint path and required role are kept in a file system storage file named <b>service.access.roles.json</b>

3. Use ASP.NET Core Policy-based Authorization
* makes use of .NET Core Action Filters to capture all requests to LoanManager controller endpoints
* parse <b>context</b> object to extract information from request header (including <b>bearer token</b>)
* same as before, the mappings between endpoint path and required role are kept in a file system storage file named <b>service.access.roles.json</b>

## Assessment
Based on the current PoC results, we conclude that **ASP.NET Core Middleware** is a good alternative to <b>action filters</b> and <b>policy-based authorization</b> for authenticating and authorizing endpoint access due to a few advantageous design concerns:<br/><br/>
&nbsp;&nbsp;&nbsp;a) Like the other two alternatives, **Middleware** processing happens after the routing (so that we have endpoint path infrmation available) but before action execution (ie endpoint run) so we can avoid unauthorized runs<br/><br/>
&nbsp;&nbsp;&nbsp;b) Unlike the other two alternatives, **Middleware** implementation is light and dose not require any changes in Startup class<br/><br/>
&nbsp;&nbsp;&nbsp;c) **Middleware** processing does not require a **principal object** wherefrom to read the claims, like policy-based authorization does; it reads the claims right from the bearer token, on the context thread<br/><br/>
&nbsp;&nbsp;&nbsp;d) In the first two implementations (Middleware and Action Filters), the use of services.AddAuthentication(...) method with authentication scheme is not necessary, as the Bearer tokens can be generated by multiple issuers (eg AAD V1, AAD V2, SLP V1, etc) and they have multiple options. However, policy-based authorization seems to not work without it.<br/>

## References
* https://docs.microsoft.com/en-us/aspnet/core/security/?view=aspnetcore-3.1
* https://www.tutorialsteacher.com/core/aspnet-core-middleware
* https://docs.microsoft.com/en-us/aspnet/web-api/overview/security/authentication-and-authorization-in-aspnet-web-api
* https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-3.1
* https://docs.microsoft.com/en-us/aspnet/core/security/authorization/roles?view=aspnetcore-3.1
* https://docs.microsoft.com/en-us/aspnet/core/security/authorization/claims?view=aspnetcore-3.1
* https://github.com/CodeMazeBlog/action-filters-dotnetcore-webapi/tree/end-project
* https://code-maze.com/action-filters-aspnetcore/
* https://dev.to/leading-edje/custom-authorization-filters-in-asp-net-web-api-3hnm
* https://livebook.manning.com/book/asp-net-core-in-action/chapter-13/15
* https://docs.microsoft.com/en-us/aspnet/web-api/overview/security/authentication-filters
* https://docs.microsoft.com/en-us/aspnet/core/security/authorization/claims?view=aspnetcore-3.1
* https://dotnetcoretutorials.com/2020/01/15/creating-and-validating-jwt-tokens-in-asp-net-core/
* https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-3.1
* https://referbruv.com/blog/implementing-policy-based-authorization-in-aspnet-core-getting-started/
* https://blog.devgenius.io/jwt-bearer-authentication-for-machine-to-machine-and-single-page-applications-1c8ba1211a90


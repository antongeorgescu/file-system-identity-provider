# Filesystem Identity Authorization Provider
Local OpenID token provider, hosted in file system, and attachable to any .NET Core (3.1) Web API solution

## Concept Diagram
![Filesystem Identity Provider](https://user-images.githubusercontent.com/6631390/182035145-9bd01e6c-4570-4e72-a296-98719dabf562.jpg)

## Filesystem Authrorization Provider - Components and Authrorization Flow
![SLP Identity Provider - Components   Flow (Github)](https://user-images.githubusercontent.com/6631390/183084741-8d0e9dbb-0bb0-4716-838e-218a8608269f.jpg)

## ASP.NET Core Web API Middleware
ASP.NET Core introduced a new concept called Middleware. A middleware is a class which is executed on every request in ASP.NET Core application. In the classic ASP.NET, HttpHandlers and HttpModules were part of request pipeline. Middleware is similar to HttpHandlers and HttpModules where both needs to be configured and executed in each request.

Typically, there will be multiple middleware in ASP.NET Core web application. It can be either framework provided middleware, added via NuGet or your own custom middleware. We can set the order of middleware execution in the request pipeline. Each middleware adds or modifies http request and optionally passes control to the next middleware component. The following figure illustrates the execution of middleware components.

![Dotnet Core Middleware Pipeline](https://user-images.githubusercontent.com/6631390/182035384-91ab32f2-8f0c-42f5-a069-7ff8d9c4187d.JPG)

Middlewares build the request pipeline. The following figure illustrates the ASP.NET Core request processing.

![Middleware Request Processing](https://user-images.githubusercontent.com/6631390/182035434-22b3119c-99a8-43ee-99e1-8f901bb0a878.JPG)

## ASP.NET Core Web API Action Filters
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

## ASP.NET Core Web API Policy-based Authorization
An authorization policy is a set of requirements and handlers. These requirements define what a request user need to satisfy inorder to proceed further. The respective handlers define how these requirements are processed when a request is made and what action needs to be presented if a rule is satisfied or failed. These requirements and handlers are registered in the Startup when the application bootstraps.

A Policy constitutes:

* A Requirement that defines some criterion for Authorization
* An AuthorizationHandler that validates the Requirement

Once a policy is defined and registered, the runtime applies these policies for validation at the endpoints where the policies are decorated with. When we have these policies in force, we can ensure that the APIs are further secured on top of Authentication and only the set of Authorized users who satisfy these policies are allowed access, else are forbidden (403) from access.

![policy-requirements-handlers](https://user-images.githubusercontent.com/6631390/182645076-a73dbeaa-f14e-416a-88d5-d72d70a19077.JPG)

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
* our claims-based authorization use a requirement, a requirement handler, and a preconfigured policy. 
* the primary service that determines if authorization is successful is IAuthorizationService
* IAuthorizationRequirement is a marker service with no methods, and the mechanism for tracking whether authorization is successful.
* each IAuthorizationHandler is responsible for checking if requirements are met:
* same as before, the mappings between endpoint path and required role are kept in a file system storage file named <b>service.access.roles.json</b>

4. Authentication Scheme

* The authentication scheme can select which authentication handler is responsible for generating the correct set of claims. 
* An authentication scheme is a name that corresponds to:<br/>
&nbsp;&nbsp;&nbsp;(i) An authentication handler.<br/>
&nbsp;&nbsp;&nbsp;(ii) Options for configuring that specific instance of the handler.<br/>

* Schemes are useful as a mechanism for referring to the authentication, challenge, and forbid behaviors of the associated handler. For example, an authorization policy can use scheme names to specify which authentication scheme (or schemes) should be used to authenticate the user. When configuring authentication, it's common to specify the default authentication scheme. The default scheme is used unless a resource requests a specific scheme. It's also possible to:<br/>
&nbsp;&nbsp;&nbsp;(i) Specify different default schemes to use for authenticate, challenge, and forbid actions.<br/>
&nbsp;&nbsp;&nbsp;(ii) Combine multiple schemes into one using policy schemes.<br/>

## Assessment
Based on the current PoC results, we conclude that **ASP.NET Core Middleware** is a good alternative to <b>action filters</b> and <b>policy-based authorization</b> for authenticating and authorizing endpoint access due to a few advantageous design concerns:<br/><br/>
&nbsp;&nbsp;&nbsp;a) Like the other two alternatives, **Middleware** processing happens after the routing (so that we have endpoint path infrmation available) but before action execution (ie endpoint run) so we can avoid unauthorized runs<br/><br/>
&nbsp;&nbsp;&nbsp;b) Unlike the other two alternatives, **Middleware** implementation is light and dose not require any changes in Startup class<br/><br/>
&nbsp;&nbsp;&nbsp;c) **Middleware** processing does not require a **principal object** wherefrom to read the claims, like policy-based authorization does; it reads the claims right from the bearer token, on the context thread<br/><br/>
&nbsp;&nbsp;&nbsp;d) In the first two implementations (Middleware and Action Filters), the use of services.AddAuthentication(...) method with authentication scheme is not necessary, as the Bearer tokens can be generated by multiple issuers (eg AAD V1, AAD V2, SLP V1, etc) and they have multiple options. However, policy-based authorization seems to not work without it.<br/>

## References
* https://docs.microsoft.com/en-us/aspnet/core/security/?view=aspnetcore-3.1
* https://www.tutorialsteacher.com/core/aspnet-core-middleware
* https://dev.to/dotnet/authentication-in-asp-net-core-59k8
* https://medium.com/dataseries/public-claims-and-how-to-validate-a-jwt-1d6c81823826
* https://referbruv.com/blog/implementing-policy-based-authorization-in-aspnet-core-getting-started/
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
* https://docs.microsoft.com/en-us/archive/msdn-magazine/2017/october/cutting-edge-policy-based-authorization-in-asp-net-core


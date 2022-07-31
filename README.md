# Filesystem Identity Provider
Local OpenID token provider, hosted in file system, and attachable to any .NET Core (3.1) Web API solution

## Concept Diagram
![Filesystem Identity Provider](https://user-images.githubusercontent.com/6631390/182035145-9bd01e6c-4570-4e72-a296-98719dabf562.jpg)

## Use .NET Core Middleware Framework
ASP.NET Core introduced a new concept called Middleware. A middleware is a class which is executed on every request in ASP.NET Core application. In the classic ASP.NET, HttpHandlers and HttpModules were part of request pipeline. Middleware is similar to HttpHandlers and HttpModules where both needs to be configured and executed in each request.

Typically, there will be multiple middleware in ASP.NET Core web application. It can be either framework provided middleware, added via NuGet or your own custom middleware. We can set the order of middleware execution in the request pipeline. Each middleware adds or modifies http request and optionally passes control to the next middleware component. The following figure illustrates the execution of middleware components.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace SampleLogger.Web.Code
{
  // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
  public class ErrorLog
  {
    private readonly RequestDelegate _next;

    public ErrorLog(RequestDelegate next)
    {
      _next = next;
    }

    public Task Invoke(HttpContext httpContext)
    {

      if (httpContext.Request.Path.Value.Equals("/errorlog", StringComparison.CurrentCultureIgnoreCase)) {
        return ShowErrorLog(httpContext);
      }

      return _next(httpContext);
    }

    private Task ShowErrorLog(HttpContext context)
    {

      context.Response.Clear();

      context.Response.ContentType = "application/json";
      return context.Response.WriteAsync(JsonConvert.SerializeObject(Code.SimpleWebLoggerProvider.LogMessages));

    }
  }

}

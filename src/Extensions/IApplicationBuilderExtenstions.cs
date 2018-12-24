using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Shared.Middleware;

namespace Shared.Extensions
{
    public static class IApplicationBuilderExtenstions
    {
        #region public static void AddExceptionHandler(this IApplicationBuilder applicationBuilder)        
        /// <summary>
        /// Adds the exception handler.
        /// </summary>
        /// <param name="applicationBuilder">The application builder.</param>
        public static void AddExceptionHandler(this IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.UseMiddleware<ExceptionHandlerMiddleware>();
        }
        #endregion
    }
}

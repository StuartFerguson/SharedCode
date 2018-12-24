﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Shared.EventStore;
using Shared.General;
using Shared.Middleware;
using Shouldly;
using Xunit;

namespace Shared.Tests
{
    public class ExceptionHandlerMiddewareUnitTest
    {
        private const String ExceptionMessage = "Test Exception Message";

        [Fact]
        public async void ExceptionHandlerMiddleware_ArgumentNullExceptionThrown_BadRequestHttpStatusCodeReturned()
        {
            Logger.Initialise(NullLogger.Instance);
            
            var middleware = new ExceptionHandlerMiddleware((innerHttpContext) =>
                throw new ArgumentNullException("TestParam",ExceptionMessage));

            var context = CreateContext();

            await middleware.Invoke(context);

            var responseData = GetErrorResponse(context);

            context.Response.StatusCode.ShouldBe((Int32) HttpStatusCode.BadRequest);
            responseData.ShouldNotBeNull();
            responseData.Message.ShouldNotBeNullOrEmpty();
        }

        [Fact]
        public async void ExceptionHandlerMiddleware_InvalidDataExceptionThrown_BadRequestHttpStatusCodeReturned()
        {
            Logger.Initialise(NullLogger.Instance);
            
            var middleware = new ExceptionHandlerMiddleware((innerHttpContext) =>
                throw new InvalidDataException(ExceptionMessage));

            var context = CreateContext();

            await middleware.Invoke(context);

            var responseData = GetErrorResponse(context);

            context.Response.StatusCode.ShouldBe((Int32) HttpStatusCode.BadRequest);
            responseData.ShouldNotBeNull();
            responseData.Message.ShouldNotBeNullOrEmpty();
        }

        [Fact]
        public async void ExceptionHandlerMiddleware_InvalidOperationExceptionThrown_BadRequestHttpStatusCodeReturned()
        {
            Logger.Initialise(NullLogger.Instance);
            
            var middleware = new ExceptionHandlerMiddleware((innerHttpContext) =>
                throw new InvalidOperationException(ExceptionMessage));

            var context = CreateContext();

            await middleware.Invoke(context);

            var responseData = GetErrorResponse(context);

            context.Response.StatusCode.ShouldBe((Int32) HttpStatusCode.BadRequest);
            responseData.ShouldNotBeNull();
            responseData.Message.ShouldNotBeNullOrEmpty();
        }

        [Fact]
        public async void ExceptionHandlerMiddleware_FormatExceptionThrown_BadRequestHttpStatusCodeReturned()
        {
            Logger.Initialise(NullLogger.Instance);
            
            var middleware = new ExceptionHandlerMiddleware((innerHttpContext) =>
                throw new FormatException(ExceptionMessage));

            var context = CreateContext();

            await middleware.Invoke(context);

            var responseData = GetErrorResponse(context);

            context.Response.StatusCode.ShouldBe((Int32) HttpStatusCode.BadRequest);
            responseData.ShouldNotBeNull();
            responseData.Message.ShouldNotBeNullOrEmpty();
        }

        [Fact]
        public async void ExceptionHandlerMiddleware_NotSupportedExceptionThrown_BadRequestHttpStatusCodeReturned()
        {
            Logger.Initialise(NullLogger.Instance);
            
            var middleware = new ExceptionHandlerMiddleware((innerHttpContext) =>
                throw new NotSupportedException(ExceptionMessage));

            var context = CreateContext();

            await middleware.Invoke(context);

            var responseData = GetErrorResponse(context);

            context.Response.StatusCode.ShouldBe((Int32) HttpStatusCode.BadRequest);
            responseData.ShouldNotBeNull();
            responseData.Message.ShouldNotBeNullOrEmpty();
        }

        [Fact]
        public async void ExceptionHandlerMiddleware_NotFoundExceptionThrown_NotFoundHttpStatusCodeReturned()
        {
            Logger.Initialise(NullLogger.Instance);
            
            var middleware = new ExceptionHandlerMiddleware((innerHttpContext) =>
                throw new NotFoundException(ExceptionMessage));

            var context = CreateContext();

            await middleware.Invoke(context);

            var responseData = GetErrorResponse(context);

            context.Response.StatusCode.ShouldBe((Int32) HttpStatusCode.NotFound);
            responseData.ShouldNotBeNull();
            responseData.Message.ShouldNotBeNullOrEmpty();
        }

        [Fact]
        public async void ExceptionHandlerMiddleware_NotImplementedExceptionThrown_NotImplementedHttpStatusCodeReturned()
        {
            Logger.Initialise(NullLogger.Instance);
            
            var middleware = new ExceptionHandlerMiddleware((innerHttpContext) =>
                throw new NotImplementedException(ExceptionMessage));

            var context = CreateContext();

            await middleware.Invoke(context);

            var responseData = GetErrorResponse(context);

            context.Response.StatusCode.ShouldBe((Int32) HttpStatusCode.NotImplemented);
            responseData.ShouldNotBeNull();
            responseData.Message.ShouldNotBeNullOrEmpty();
        }

        [Fact]
        public async void ExceptionHandlerMiddleware_OtherExceptionThrown_InternalServerErrorHttpStatusCodeReturned()
        {
            Logger.Initialise(NullLogger.Instance);
            
            var middleware = new ExceptionHandlerMiddleware((innerHttpContext) =>
                throw new Exception(ExceptionMessage));

            var context = CreateContext();

            await middleware.Invoke(context);

            var responseData = GetErrorResponse(context);

            context.Response.StatusCode.ShouldBe((Int32) HttpStatusCode.InternalServerError);
            responseData.ShouldNotBeNull();
            responseData.Message.ShouldNotBeNullOrEmpty();
        }

        private static ErrorResponse GetErrorResponse(DefaultHttpContext context)
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(context.Response.Body);
            var streamText = reader.ReadToEnd();
            var responseData = JsonConvert.DeserializeObject<ErrorResponse>(streamText);
            return responseData;
        }

        private static DefaultHttpContext CreateContext()
        {
            var context = new DefaultHttpContext();
            context.Request.Scheme = "http";
            context.Request.Host = new HostString("localhost");
            context.Request.Path = new PathString("/test");
            context.Request.PathBase = new PathString("/");
            context.Request.Method = "GET";
            context.Request.Body = new MemoryStream();
            context.Request.QueryString = new QueryString("?param1=2");
            context.Response.Body = new MemoryStream();
            return context;
        }
    }
}

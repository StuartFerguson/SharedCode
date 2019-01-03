﻿using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ClientProxyBase
{
    public abstract class ClientProxyBase
    {
        /// <summary>
        /// Handles the response.
        /// </summary>
        /// <param name="responseMessage">The response message.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <exception cref="System.UnauthorizedAccessException"></exception>
        /// <exception cref="InvalidDataException"></exception>
        /// <exception cref="System.Exception">An internal error has occurred
        /// or
        /// An internal error has occurred</exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="Exception">An internal error has occurred
        /// or
        /// An internal error has occurred</exception>
        protected virtual async Task<String> HandleResponse(HttpResponseMessage responseMessage, CancellationToken cancellationToken)
        {
            String result = String.Empty;

            // Read the content from the response
            String content = await responseMessage.Content.ReadAsStringAsync();

            // Check the response code
            if (!responseMessage.IsSuccessStatusCode)
            {
                // throw a specific  exception to inherited class
                switch (responseMessage.StatusCode)
                {                                         
                    case HttpStatusCode.BadRequest:                        
                        throw new InvalidOperationException(content);
                    case HttpStatusCode.Unauthorized:
                    case HttpStatusCode.Forbidden:
                        throw new UnauthorizedAccessException(content);
                    case HttpStatusCode.NotFound:
                        throw new InvalidDataException(content);
                    case HttpStatusCode.InternalServerError:
                        throw new Exception("An internal error has occurred");
                    default:
                        throw new Exception($"An internal error has occurred ({responseMessage.StatusCode})");
                }
            }

            // Set the result
            result = content;

            return result;
        }
    }
}

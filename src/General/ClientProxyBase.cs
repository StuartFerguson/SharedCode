using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Shared.EventStore;

namespace Shared.General
{
    public abstract class ClientProxyBase
    {
        /// <summary>
        /// Handles the response.
        /// </summary>
        /// <param name="responseMessage">The response message.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="NotFoundException"></exception>
        /// <exception cref="Exception">
        /// An internal error has occurred
        /// or
        /// An internal error has occurred
        /// </exception>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <exception cref="System.Exception">An internal error has occurred
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
                        throw new NotFoundException(content);
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

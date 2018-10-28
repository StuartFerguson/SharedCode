using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI.Projections;
using EventStore.ClientAPI.SystemData;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ESLogger = EventStore.ClientAPI;

namespace Shared.EventStore
{
    public class EventStoreProjectionManager : IProjectionManager
    {
        #region Private Properties

        /// <summary>
        /// The projection manager
        /// </summary>
        private ProjectionsManager ProjectionManager;

        /// <summary>
        /// The serializer
        /// </summary>
        private readonly ISerialiser Serialiser;

        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger<EventStoreProjectionManager> Logger;

        /// <summary>
        /// Connection details
        /// </summary>
        private EventStoreConnectionSettings EventStoreConnectionSettings { get; set; }

        /// <summary>
        /// The event store logger
        /// </summary>
        private readonly ESLogger.ILogger EventStoreLogger;

        /// <summary>
        /// The file reader
        /// </summary>
        private readonly IFileReader FileReader;

        #endregion

        #region Private Constants        

        /// <summary>
        /// The timeout hours
        /// </summary>
        private const Int32 TimeoutHours = 0;

        /// <summary>
        /// The timeout mins
        /// </summary>
        private const Int32 TimeoutMins = 0;

        /// <summary>
        /// The timeout seconds
        /// </summary>
        private const Int32 TimeoutSeconds = 30;

        /// <summary>
        /// The get status query timeout
        /// </summary>
        private const Int32 GetStatusQueryTimeout = 15000;

        /// <summary>
        /// The sleep value
        /// </summary>
        private const Int32 SleepValue = 500;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="EventStoreProjectionManager"/> class.
        /// </summary>
        public EventStoreProjectionManager(ILoggerFactory loggerFactory, IOptions<EventStoreConnectionSettings> eventStoreConnectionSettings, 
                                           ESLogger.ILogger eventStoreLogger, ISerialiser serialiser, IFileReader fileReader)
        {
            // Perform initial validation on input parameters  
            Guard.ThrowIfNull(loggerFactory, nameof(loggerFactory));
            Guard.ThrowIfNull(eventStoreLogger, nameof(eventStoreLogger));
            Guard.ThrowIfNull(serialiser, nameof(serialiser));
            Guard.ThrowIfNull(fileReader, nameof(fileReader));

            ValidateConnectionDetails(eventStoreConnectionSettings);

            // Set the local props
            this.Logger = loggerFactory.CreateLogger<EventStoreProjectionManager>();
            EventStoreConnectionSettings = eventStoreConnectionSettings.Value;

            this.Logger.LogDebug(new EventId(), "Event Store Connection Settings");
            this.Logger.LogDebug(new EventId(), $"IP Address [{this.EventStoreConnectionSettings.IPAddress}]");
            this.Logger.LogDebug(new EventId(), $"TCP Port [{this.EventStoreConnectionSettings.TcpPort}]");
            this.Logger.LogDebug(new EventId(), $"HTTP Port [{this.EventStoreConnectionSettings.HttpPort}]");
            this.Logger.LogDebug(new EventId(), $"User Name [{this.EventStoreConnectionSettings.UserName}]");
            this.Logger.LogDebug(new EventId(), $"Password [{this.EventStoreConnectionSettings.Password}]");

            this.EventStoreLogger = eventStoreLogger;
            this.Serialiser = serialiser;
            this.FileReader = fileReader;

            // Craete the Projection manager
            CreateProjectionManager();
        }

        #endregion

        #region Public Methods

        #region public async Task<T> GetState<T>(String projectionName, CancellationToken cancellationToken)        

        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="projectionName">Name of the projection.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<T> GetState<T>(String projectionName, CancellationToken cancellationToken)
        {
            T result = default(T);

            // Validate the Arguments
            Guard.ThrowIfNullOrEmpty(projectionName, nameof(projectionName));

            // Get the state of the projection 
            var state = await this.ProjectionManager.GetStateAsync(projectionName);

            // Now build the result from the state returned
            result = this.Serialiser.Deserialise<T>(state);

            return result;
        }

        #endregion

        #region public async Task<T> RunTransientQuery<T>(String queryName, String queryContents, CancellationToken cancellationToken)

        /// <summary>
        /// Runs the transient query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryName">Name of the query.</param>
        /// <param name="queryContents">The query contents.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">No user details within the uri connection string</exception>
        public async Task<T> RunTransientQuery<T>(String queryName, String queryContents, CancellationToken cancellationToken)
        {
            T result = default(T);

            // Validate the Arguments
            Guard.ThrowIfNullOrEmpty(queryName, nameof(queryName));
            Guard.ThrowIfNullOrEmpty(queryContents, nameof(queryContents));
           
            UserCredentials userCredentials = new UserCredentials(this.EventStoreConnectionSettings.UserName, this.EventStoreConnectionSettings.Password);

            // Create the transient query
            await this.ProjectionManager.CreateTransientAsync(queryName, queryContents, userCredentials);

            Int32 elapsed = 0;
            while (true && (elapsed < GetStatusQueryTimeout))
            {
                // Get the status if the query and keep doing so until its complete
                String json = await this.ProjectionManager.GetStatusAsync(queryName, null);
                QueryStatus queryStatus = this.Serialiser.Deserialise<QueryStatus>(json);

                // Break out if the query has completed.
                if (queryStatus.Status.StartsWith("Complete", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                Thread.Sleep(SleepValue);
                elapsed += 500;
            }

            // Get the state of the projection 
            var state = await this.ProjectionManager.GetStateAsync(queryName, userCredentials);

            // Check the query has run successfully
            if (String.IsNullOrEmpty(state))
            {
                this.Logger.LogWarning(new EventId(), $"No state returned from transient query."); // TODO: Should we write out some more information here ???
            }
            else if (state != "{}")
            {
                // Now build the result from the state returned
                result = JsonConvert.DeserializeObject<T>(state);
            }

            return result;
        }

        #endregion

        #region public async Task<T> RunTransientQuery<T>(String fullFilePath, Dictionary<String, String> queryReplacements, CancellationToken cancellationToken)        
        /// <summary>
        /// Runs the transient query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fullFilePath">The full file path.</param>
        /// <param name="queryReplacements">The query replacements.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<T> RunTransientQuery<T>(String fullFilePath, Dictionary<String, String> queryReplacements, CancellationToken cancellationToken)
        {
            T result = default(T);

            // Read the transient query file
            String query = await GetQueryFileContents(fullFilePath, cancellationToken);

            // Perform any required replacements
            foreach (var queryReplacement in queryReplacements)
            {
                query = query.Replace(queryReplacement.Key, queryReplacement.Value);    
            }

            result = await RunTransientQuery<T>(Guid.NewGuid().ToString("N"), query, cancellationToken);

            return result;
        }
        #endregion

        #region public async Task<T> GetPartitionState<T>(String projectionName, String partitionId, CancellationToken cancellationToken)        
        /// <summary>
        /// Gets the state of the partition.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="projectionName">Name of the projection.</param>
        /// <param name="partitionId">The partition identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<T> GetPartitionState<T>(String projectionName, String partitionId, CancellationToken cancellationToken)
        {
            T result = default(T);

            // Validate the Arguments
            Guard.ThrowIfNullOrEmpty(projectionName, nameof(projectionName));
            Guard.ThrowIfNullOrEmpty(partitionId, nameof(partitionId));

            // Get the state of the projection 
            var state = await this.ProjectionManager.GetPartitionStateAsync(projectionName, partitionId);

            // Now build the result from the state returned
            result = this.Serialiser.Deserialise<T>(state);

            return result;
        }
        #endregion

        #endregion

        #region Private Methods

        #region private void ValidateConnectionDetails(IOptions<EventStoreConnectionSettings> eventStoreConnectionSettings)

        /// <summary>
        /// Validates the connection details
        /// </summary>
        /// <param name="eventStoreConnectionSettings"></param>
        private void ValidateConnectionDetails(IOptions<EventStoreConnectionSettings> eventStoreConnectionSettings)
        {
            Guard.ThrowIfNull(eventStoreConnectionSettings, typeof(ArgumentNullException),"Event Store connection details could not be found.");
            Guard.ThrowIfNull(eventStoreConnectionSettings.Value, typeof(ArgumentNullException),"Event Store connection values could not be found.");
            Guard.ThrowIfNullOrEmpty(eventStoreConnectionSettings.Value.IPAddress, typeof(ArgumentNullException),"Event Store IP Address is invalid.");
            Guard.ThrowIfNegative(eventStoreConnectionSettings.Value.HttpPort, typeof(ArgumentNullException),"Event Store HttpPort cannot be negative.");
            Guard.ThrowIfZero(eventStoreConnectionSettings.Value.HttpPort, typeof(ArgumentNullException),"Event Store HttpPort cannot be zero.");
            Guard.ThrowIfNullOrEmpty(eventStoreConnectionSettings.Value.ConnectionName, typeof(ArgumentNullException),"Event Store Connection name is invalid.");
            Guard.ThrowIfNullOrEmpty(eventStoreConnectionSettings.Value.UserName, typeof(ArgumentNullException), "Event Store Connection User Name is invalid.");
            Guard.ThrowIfNullOrEmpty(eventStoreConnectionSettings.Value.Password, typeof(ArgumentNullException), "Event Store Connection Password is invalid.");
        }

        #endregion

        #region private void CreateProjectionManager()        

        /// <summary>
        /// Creates the projection manager.
        /// </summary>
        private void CreateProjectionManager()
        {
            // Set up the Http Endpoint
            IPEndPoint esEndPoint = new IPEndPoint(IPAddress.Parse(EventStoreConnectionSettings.IPAddress),EventStoreConnectionSettings.HttpPort);

            // Create the projection manager
            this.ProjectionManager = new ProjectionsManager(this.EventStoreLogger, esEndPoint, new TimeSpan(TimeoutHours, TimeoutMins, TimeoutSeconds));
        }

        #endregion

        #region private async Task<String> GetQueryFileContents(String queryFileName, CancellationToken cancellationToken)
        /// <summary>
        /// Gets the query file contents.
        /// </summary>
        /// <param name="fullFilePath">The full file path.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        /// <exception cref="System.IO.FileNotFoundException">Unable to find query file in expected locations</exception>
        private async Task<String> GetQueryFileContents(String fullFilePath, CancellationToken cancellationToken)
        {           
            List<String> errorMessage = new List<String>();

            // Read the file contents
            String queryContents = await this.FileReader.ReadContentsOfFile(fullFilePath, cancellationToken);

            // Check the contents
            if (String.IsNullOrEmpty(queryContents))
            {
                throw new NotFoundException($"Query file [{fullFilePath}] was read successfully but had no contents");
            }

            // Return the contents
            return queryContents;
        }
        #endregion

        #endregion
    }
}
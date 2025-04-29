using System.ComponentModel.DataAnnotations;

namespace AzBae.MCP.Models
{
    public class CreateCosmosContainerRequest
    {
        /// <summary>
        /// The Cosmos DB account endpoint URL
        /// </summary>
        [Required]
        public string AccountEndpoint { get; set; } = "";

        /// <summary>
        /// The Cosmos DB account key
        /// </summary>
        [Required]
        public string AccountKey { get; set; } = "";

        /// <summary>
        /// Database to create or use
        /// </summary>
        [Required]
        public string DatabaseName { get; set; } = "";

        /// <summary>
        /// Name of the container to create
        /// </summary>
        [Required]
        public string ContainerName { get; set; } = "";

        /// <summary>
        /// Partition key path for the container
        /// </summary>
        [Required]
        public string PartitionKey { get; set; } = "";
    }

    public class CreateCosmosContainerResponse
    {
        /// <summary>
        /// Indicates whether the operation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Message providing details about the operation result
        /// </summary>
        public string Message { get; set; } = "";

        /// <summary>
        /// Database name that was created or used
        /// </summary>
        public string? DatabaseName { get; set; }

        /// <summary>
        /// Container name that was created or used
        /// </summary>
        public string? ContainerName { get; set; }
    }
}
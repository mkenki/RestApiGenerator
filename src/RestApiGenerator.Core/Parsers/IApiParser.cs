using RestApiGenerator.Core.Models;

namespace RestApiGenerator.Core.Parsers
{
    public interface IApiParser
    {
        Task<SwaggerDocument> ParseAsync(string input);
        Task<SwaggerDocument> ParseFromUrlAsync(string url);
        Task<SwaggerDocument> ParseFromFileAsync(string filePath);
        bool CanParse(string input);
    }
}
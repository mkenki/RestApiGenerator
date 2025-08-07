using RestApiGenerator.Core.Models;

namespace RestApiGenerator.Core.Generators
{
    public interface ICodeGenerator
    {
        Task<string> GenerateClientAsync(CodeModel model);
        Task<string> GenerateModelsAsync(CodeModel model);
        Task<string> GenerateInterfaceAsync(CodeModel model);
        Task<Dictionary<string, string>> GenerateAllAsync(CodeModel model);
    }
}
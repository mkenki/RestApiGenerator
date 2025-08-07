// 1. Plugin Interface Definitions
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestApiGenerator.Plugins
{
    // Ana plugin interface'i
    public interface IPlugin
    {
        string Name { get; }
        string Version { get; }
        string Description { get; }
        PluginType Type { get; }
        
        Task InitializeAsync(PluginContext context);
        Task<bool> CanExecuteAsync(PluginExecutionContext executionContext);
        void Dispose();
    }

    // Plugin türleri
    public enum PluginType
    {
        PreProcessor,
        PostProcessor,
        CodeTransformer,
        Validator,
        Generator,
        OutputFormatter
    }

    // Pre-processing plugin'ları için
    public interface IPreProcessorPlugin : IPlugin
    {
        Task<SwaggerDocument> ProcessAsync(SwaggerDocument document, PreProcessorContext context);
    }

    // Post-processing plugin'ları için
    public interface IPostProcessorPlugin : IPlugin
    {
        Task<GeneratedCode> ProcessAsync(GeneratedCode code, PostProcessorContext context);
    }

    // Code transformation plugin'ları için
    public interface ICodeTransformerPlugin : IPlugin
    {
        Task<CodeModel> TransformAsync(CodeModel model, TransformationContext context);
        bool SupportsLanguage(string language);
    }

    // Validation plugin'ları için
    public interface IValidatorPlugin : IPlugin
    {
        Task<ValidationResult> ValidateAsync(SwaggerDocument document, ValidationContext context);
    }

    // Custom generator plugin'ları için
    public interface IGeneratorPlugin : IPlugin
    {
        Task<GeneratedCode> GenerateAsync(CodeModel model, GeneratorContext context);
        string TargetLanguage { get; }
        string[] SupportedTemplates { get; }
    }
}

// 2. Plugin Context Classes
namespace RestApiGenerator.Plugins
{
    public class PluginContext
    {
        public IServiceProvider ServiceProvider { get; set; }
        public ILogger Logger { get; set; }
        public Dictionary<string, object> Configuration { get; set; }
        public string WorkingDirectory { get; set; }
    }

    public class PluginExecutionContext
    {
        public string Language { get; set; }
        public GenerationSettings Settings { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }

    public class PreProcessorContext : PluginExecutionContext
    {
        public string SourceType { get; set; } // File, Url, Text
        public string SourcePath { get; set; }
    }

    public class PostProcessorContext : PluginExecutionContext
    {
        public string OutputPath { get; set; }
        public GenerationTarget Target { get; set; }
    }

    public class TransformationContext : PluginExecutionContext
    {
        public string[] SelectedOperations { get; set; }
        public Dictionary<string, string> TypeMappings { get; set; }
    }

    public class ValidationContext : PluginExecutionContext
    {
        public ValidationLevel Level { get; set; }
        public string[] IgnoredRules { get; set; }
    }

    public class GeneratorContext : PluginExecutionContext
    {
        public string TemplatePath { get; set; }
        public Dictionary<string, string> TemplateVariables { get; set; }
    }
}

// 3. Plugin Results
namespace RestApiGenerator.Plugins
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<ValidationError> Errors { get; set; } = new();
        public List<ValidationWarning> Warnings { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class ValidationError
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string Path { get; set; }
        public Severity Severity { get; set; }
    }

    public class ValidationWarning
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string Path { get; set; }
    }

    public enum Severity
    {
        Info,
        Warning,
        Error,
        Critical
    }

    public enum ValidationLevel
    {
        Basic,
        Standard,
        Strict
    }

    public class GeneratedCode
    {
        public Dictionary<string, string> Files { get; set; } = new();
        public List<string> Dependencies { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
}

// 4. Plugin Manager
namespace RestApiGenerator.Plugins
{
    public interface IPluginManager
    {
        Task LoadPluginsAsync(string pluginDirectory);
        Task LoadPluginAsync(string pluginPath);
        Task<IEnumerable<T>> GetPluginsAsync<T>() where T : class, IPlugin;
        Task<T> GetPluginAsync<T>(string name) where T : class, IPlugin;
        Task ExecutePreProcessorsAsync(SwaggerDocument document, PreProcessorContext context);
        Task ExecutePostProcessorsAsync(GeneratedCode code, PostProcessorContext context);
        Task<ValidationResult> ExecuteValidatorsAsync(SwaggerDocument document, ValidationContext context);
        void RegisterPlugin<T>(T plugin) where T : class, IPlugin;
        void UnregisterPlugin(string name);
    }

    public class PluginManager : IPluginManager
    {
        private readonly Dictionary<string, IPlugin> _plugins = new();
        private readonly ILogger<PluginManager> _logger;
        private readonly IServiceProvider _serviceProvider;

        public PluginManager(ILogger<PluginManager> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task LoadPluginsAsync(string pluginDirectory)
        {
            if (!Directory.Exists(pluginDirectory))
            {
                _logger.LogWarning("Plugin directory not found: {Directory}", pluginDirectory);
                return;
            }

            var pluginFiles = Directory.GetFiles(pluginDirectory, "*.dll", SearchOption.AllDirectories);
            
            foreach (var pluginFile in pluginFiles)
            {
                try
                {
                    await LoadPluginAsync(pluginFile);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load plugin: {PluginFile}", pluginFile);
                }
            }
        }

        public async Task LoadPluginAsync(string pluginPath)
        {
            var assembly = Assembly.LoadFrom(pluginPath);
            var pluginTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && typeof(IPlugin).IsAssignableFrom(t));

            foreach (var pluginType in pluginTypes)
            {
                var plugin = (IPlugin)Activator.CreateInstance(pluginType);
                await plugin.InitializeAsync(new PluginContext
                {
                    ServiceProvider = _serviceProvider,
                    Logger = _logger,
                    Configuration = new Dictionary<string, object>(),
                    WorkingDirectory = Path.GetDirectoryName(pluginPath)
                });

                RegisterPlugin(plugin);
                _logger.LogInformation("Loaded plugin: {Name} v{Version}", plugin.Name, plugin.Version);
            }
        }

        public Task<IEnumerable<T>> GetPluginsAsync<T>() where T : class, IPlugin
        {
            var plugins = _plugins.Values.OfType<T>();
            return Task.FromResult(plugins);
        }

        public Task<T> GetPluginAsync<T>(string name) where T : class, IPlugin
        {
            _plugins.TryGetValue(name, out var plugin);
            return Task.FromResult(plugin as T);
        }

        public async Task ExecutePreProcessorsAsync(SwaggerDocument document, PreProcessorContext context)
        {
            var processors = await GetPluginsAsync<IPreProcessorPlugin>();
            
            foreach (var processor in processors)
            {
                if (await processor.CanExecuteAsync(context))
                {
                    document = await processor.ProcessAsync(document, context);
                }
            }
        }

        public async Task ExecutePostProcessorsAsync(GeneratedCode code, PostProcessorContext context)
        {
            var processors = await GetPluginsAsync<IPostProcessorPlugin>();
            
            foreach (var processor in processors)
            {
                if (await processor.CanExecuteAsync(context))
                {
                    code = await processor.ProcessAsync(code, context);
                }
            }
        }

        public async Task<ValidationResult> ExecuteValidatorsAsync(SwaggerDocument document, ValidationContext context)
        {
            var validators = await GetPluginsAsync<IValidatorPlugin>();
            var combinedResult = new ValidationResult { IsValid = true };
            
            foreach (var validator in validators)
            {
                if (await validator.CanExecuteAsync(context))
                {
                    var result = await validator.ValidateAsync(document, context);
                    
                    combinedResult.Errors.AddRange(result.Errors);
                    combinedResult.Warnings.AddRange(result.Warnings);
                    
                    if (!result.IsValid)
                        combinedResult.IsValid = false;
                }
            }
            
            return combinedResult;
        }

        public void RegisterPlugin<T>(T plugin) where T : class, IPlugin
        {
            _plugins[plugin.Name] = plugin;
        }

        public void UnregisterPlugin(string name)
        {
            if (_plugins.TryGetValue(name, out var plugin))
            {
                plugin.Dispose();
                _plugins.Remove(name);
            }
        }
    }
}

// 5. Plugin Configuration
namespace RestApiGenerator.Plugins
{
    public class PluginConfiguration
    {
        public string Name { get; set; }
        public bool Enabled { get; set; } = true;
        public int Priority { get; set; } = 0;
        public Dictionary<string, object> Settings { get; set; } = new();
    }

    public class PluginSettings
    {
        public List<PluginConfiguration> Plugins { get; set; } = new();
        public string PluginDirectory { get; set; } = "plugins";
        public bool EnableAutoDiscovery { get; set; } = true;
    }
}

// 6. Base Plugin Implementation
namespace RestApiGenerator.Plugins
{
    public abstract class BasePlugin : IPlugin
    {
        public abstract string Name { get; }
        public abstract string Version { get; }
        public abstract string Description { get; }
        public abstract PluginType Type { get; }

        protected ILogger Logger { get; private set; }
        protected PluginContext Context { get; private set; }

        public virtual Task InitializeAsync(PluginContext context)
        {
            Context = context;
            Logger = context.Logger;
            return Task.CompletedTask;
        }

        public virtual Task<bool> CanExecuteAsync(PluginExecutionContext executionContext)
        {
            return Task.FromResult(true);
        }

        public virtual void Dispose()
        {
            // Override if cleanup is needed
        }
    }
}

// 7. Plugin Discovery Service
namespace RestApiGenerator.Plugins
{
    public interface IPluginDiscoveryService
    {
        Task<IEnumerable<PluginManifest>> DiscoverPluginsAsync(string directory);
        Task<PluginManifest> LoadManifestAsync(string pluginPath);
        Task<bool> ValidatePluginAsync(string pluginPath);
    }

    public class PluginManifest
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string AssemblyPath { get; set; }
        public List<string> Dependencies { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
        public PluginType[] SupportedTypes { get; set; } = Array.Empty<PluginType>();
    }

    public class PluginDiscoveryService : IPluginDiscoveryService
    {
        private readonly ILogger<PluginDiscoveryService> _logger;

        public PluginDiscoveryService(ILogger<PluginDiscoveryService> logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<PluginManifest>> DiscoverPluginsAsync(string directory)
        {
            var manifests = new List<PluginManifest>();

            if (!Directory.Exists(directory))
                return manifests;

            var manifestFiles = Directory.GetFiles(directory, "plugin.json", SearchOption.AllDirectories);

            foreach (var manifestFile in manifestFiles)
            {
                try
                {
                    var manifest = await LoadManifestAsync(manifestFile);
                    if (manifest != null)
                    {
                        manifests.Add(manifest);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load plugin manifest: {ManifestFile}", manifestFile);
                }
            }

            return manifests;
        }

        public async Task<PluginManifest> LoadManifestAsync(string manifestPath)
        {
            var json = await File.ReadAllTextAsync(manifestPath);
            var manifest = JsonSerializer.Deserialize<PluginManifest>(json);
            
            // Set absolute path for assembly
            var pluginDirectory = Path.GetDirectoryName(manifestPath);
            manifest.AssemblyPath = Path.Combine(pluginDirectory, manifest.AssemblyPath);
            
            return manifest;
        }

        public async Task<bool> ValidatePluginAsync(string pluginPath)
        {
            try
            {
                var assembly = Assembly.LoadFrom(pluginPath);
                var hasPluginTypes = assembly.GetTypes()
                    .Any(t => t.IsClass && !t.IsAbstract && typeof(IPlugin).IsAssignableFrom(t));
                
                return hasPluginTypes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Plugin validation failed: {PluginPath}", pluginPath);
                return false;
            }
        }
    }
}

// 8. Plugin Execution Pipeline
namespace RestApiGenerator.Plugins
{
    public interface IPluginExecutionPipeline
    {
        Task<SwaggerDocument> ExecutePreProcessingAsync(SwaggerDocument document, PreProcessorContext context);
        Task<CodeModel> ExecutateTransformationAsync(CodeModel model, TransformationContext context);
        Task<GeneratedCode> ExecuteGenerationAsync(CodeModel model, GeneratorContext context);
        Task<GeneratedCode> ExecutePostProcessingAsync(GeneratedCode code, PostProcessorContext context);
        Task<ValidationResult> ExecuteValidationAsync(SwaggerDocument document, ValidationContext context);
    }

    public class PluginExecutionPipeline : IPluginExecutionPipeline
    {
        private readonly IPluginManager _pluginManager;
        private readonly ILogger<PluginExecutionPipeline> _logger;

        public PluginExecutionPipeline(IPluginManager pluginManager, ILogger<PluginExecutionPipeline> logger)
        {
            _pluginManager = pluginManager;
            _logger = logger;
        }

        public async Task<SwaggerDocument> ExecutePreProcessingAsync(SwaggerDocument document, PreProcessorContext context)
        {
            _logger.LogInformation("Executing pre-processing pipeline");
            
            var processors = (await _pluginManager.GetPluginsAsync<IPreProcessorPlugin>())
                .OrderBy(p => GetPluginPriority(p));

            var processedDocument = document;

            foreach (var processor in processors)
            {
                if (await processor.CanExecuteAsync(context))
                {
                    _logger.LogDebug("Executing pre-processor: {ProcessorName}", processor.Name);
                    processedDocument = await processor.ProcessAsync(processedDocument, context);
                }
            }

            return processedDocument;
        }

        public async Task<CodeModel> ExecutateTransformationAsync(CodeModel model, TransformationContext context)
        {
            _logger.LogInformation("Executing transformation pipeline");
            
            var transformers = (await _pluginManager.GetPluginsAsync<ICodeTransformerPlugin>())
                .Where(t => t.SupportsLanguage(context.Language))
                .OrderBy(p => GetPluginPriority(p));

            var transformedModel = model;

            foreach (var transformer in transformers)
            {
                if (await transformer.CanExecuteAsync(context))
                {
                    _logger.LogDebug("Executing transformer: {TransformerName}", transformer.Name);
                    transformedModel = await transformer.TransformAsync(transformedModel, context);
                }
            }

            return transformedModel;
        }

        public async Task<GeneratedCode> ExecuteGenerationAsync(CodeModel model, GeneratorContext context)
        {
            _logger.LogInformation("Executing generation pipeline");
            
            var generators = (await _pluginManager.GetPluginsAsync<IGeneratorPlugin>())
                .Where(g => g.TargetLanguage == context.Language)
                .OrderBy(p => GetPluginPriority(p));

            foreach (var generator in generators)
            {
                if (await generator.CanExecuteAsync(context))
                {
                    _logger.LogDebug("Executing generator: {GeneratorName}", generator.Name);
                    return await generator.GenerateAsync(model, context);
                }
            }

            throw new InvalidOperationException($"No suitable generator found for language: {context.Language}");
        }

        public async Task<GeneratedCode> ExecutePostProcessingAsync(GeneratedCode code, PostProcessorContext context)
        {
            _logger.LogInformation("Executing post-processing pipeline");
            
            var processors = (await _pluginManager.GetPluginsAsync<IPostProcessorPlugin>())
                .OrderBy(p => GetPluginPriority(p));

            var processedCode = code;

            foreach (var processor in processors)
            {
                if (await processor.CanExecuteAsync(context))
                {
                    _logger.LogDebug("Executing post-processor: {ProcessorName}", processor.Name);
                    processedCode = await processor.ProcessAsync(processedCode, context);
                }
            }

            return processedCode;
        }

        public async Task<ValidationResult> ExecuteValidationAsync(SwaggerDocument document, ValidationContext context)
        {
            _logger.LogInformation("Executing validation pipeline");
            
            return await _pluginManager.ExecuteValidatorsAsync(document, context);
        }

        private int GetPluginPriority(IPlugin plugin)
        {
            // Plugin metadata'dan priority bilgisini al, default 0
            return 0; // Bu implementasyon plugin configuration'dan gelecek
        }
    }
}
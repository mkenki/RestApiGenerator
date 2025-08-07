namespace RestApiGenerator.Core.Models
{
    public class GeneratorConfig
    {
        private string namespaceName = string.Empty;

        public string NamespaceName 
        { 
            get => namespaceName;
            set => namespaceName = value?.Trim() ?? string.Empty;
        }

        public string ClientName { get; set; } = "ApiClient";

        public AuthenticationConfig Authentication { get; set; } = new();

        public bool HasAuthentication => 
            Authentication?.Type != AuthenticationType.None;

        public void Validate()
        {
            if (HasAuthentication)
            {
                Authentication.Validate();
            }
        }
    }

    public class AuthenticationConfig
    {
        private string name = string.Empty;

        public AuthenticationType Type { get; set; }
        public AuthenticationLocation Location { get; set; }
        
        public string Name 
        { 
            get => name;
            set => name = value?.Trim() ?? string.Empty;
        }

        public void Validate()
        {
            if (Type != AuthenticationType.None)
            {
                if (string.IsNullOrWhiteSpace(Name))
                {
                    throw new InvalidOperationException(
                        "Authentication name must be specified when authentication is enabled");
                }

                if (Type == AuthenticationType.ApiKey && Location == AuthenticationLocation.None)
                {
                    throw new InvalidOperationException(
                        "Authentication location must be specified for API Key authentication");
                }
            }
        }
    }

    public enum AuthenticationType
    {
        None,
        ApiKey,
        Bearer
    }

    public enum AuthenticationLocation
    {
        None,
        Header,
        Query
    }
}

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

        public OAuth2Config OAuth2 { get; set; } = new();

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

                // OAuth2 validation
                if (Type >= AuthenticationType.OAuth2AuthorizationCode && Type <= AuthenticationType.OAuth2Password)
                {
                    OAuth2.Validate();
                }
            }
        }
    }

    public class OAuth2Config
    {
        public string ClientId { get; set; } = "";
        public string ClientSecret { get; set; } = "";
        public string TokenEndpoint { get; set; } = "";
        public string AuthorizationEndpoint { get; set; } = "";
        public string RedirectUri { get; set; } = "";
        public string Scopes { get; set; } = "";
        public OAuth2Flow Flow { get; set; } = OAuth2Flow.AuthorizationCode;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ClientId))
            {
                throw new InvalidOperationException("Client ID is required for OAuth2 authentication");
            }

            if (string.IsNullOrWhiteSpace(ClientSecret))
            {
                throw new InvalidOperationException("Client Secret is required for OAuth2 authentication");
            }

            if (string.IsNullOrWhiteSpace(TokenEndpoint))
            {
                throw new InvalidOperationException("Token endpoint is required for OAuth2 authentication");
            }
        }
    }

    public enum OAuth2Flow
    {
        AuthorizationCode,
        ClientCredentials,
        Password,
        Implicit
    }

    public enum AuthenticationType
    {
        None,
        ApiKey,
        Bearer,
        OAuth2AuthorizationCode,
        OAuth2ClientCredentials,
        OAuth2Password
    }

    public enum AuthenticationLocation
    {
        None,
        Header,
        Query
    }
}

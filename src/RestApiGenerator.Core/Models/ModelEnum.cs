namespace RestApiGenerator.Core.Models
{
    public class ModelEnum
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Values { get; set; } = new();
        public string? Description { get; set; }
    }
}

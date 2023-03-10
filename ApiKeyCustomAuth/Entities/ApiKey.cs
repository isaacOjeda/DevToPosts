namespace ApiKeyCustomAuth.Entities;

public class ApiKey
{
    public int ApiKeyId { get; set; }
    public Guid Key { get; set; }
    public string Name { get; set; }
}
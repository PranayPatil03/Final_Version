namespace CRISP.Application.DTOs;

public sealed class EntityDto
{
    public EntityDto() { }
    public EntityDto(int id, string entityName) { Id = id; EntityName = entityName; }
    public int Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
}

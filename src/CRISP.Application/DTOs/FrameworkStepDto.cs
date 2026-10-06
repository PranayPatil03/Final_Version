namespace CRISP.Application.DTOs;

public sealed class FrameworkStepDto
{
    public FrameworkStepDto() { }
    public FrameworkStepDto(int id, string stepName, int displayOrder, bool isActive)
    { Id = id; StepName = stepName; DisplayOrder = displayOrder; IsActive = isActive; }
    public int Id { get; set; }
    public string StepName { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

namespace CRISP.Domain.Entities;

public class CustomerMaster
{
    public int Id { get; set; }
    public string EntityName { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public ICollection<FrameworkInstance> Frameworks { get; set; } = new List<FrameworkInstance>();
}

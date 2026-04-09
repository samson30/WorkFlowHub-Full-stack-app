using System.ComponentModel.DataAnnotations;

namespace WorkFlowHub.Core.DTOs.Tasks;

public class CreateLabelDto
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Color { get; set; } = "#6366f1";
}

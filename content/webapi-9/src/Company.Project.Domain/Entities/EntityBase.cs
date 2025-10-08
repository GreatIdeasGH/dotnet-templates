using System.ComponentModel.DataAnnotations;

namespace Company.Project.Domain.Entities;

public abstract record EntityBase
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    [Timestamp]
    public uint RowVersion { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public string? CreatedBy { get; set; }

    public DateTimeOffset? DateModified { get; set; }
    public string? ModifiedBy { get; set; }
} //end EntityBase

//end namespace Entities

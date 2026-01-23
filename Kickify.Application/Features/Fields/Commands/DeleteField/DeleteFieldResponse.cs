namespace Kickify.Application.Features.Fields.Commands.DeleteField
{
    public record DeleteFieldResponse(
        Guid FieldId,
        bool IsDeleted
    );
}

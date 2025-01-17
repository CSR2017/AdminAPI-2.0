using EdFi.Ods.AdminApi.Infrastructure.ClaimSetEditor;
using EdFi.Security.DataAccess.Contexts;

namespace EdFi.Ods.AdminApi.Infrastructure.Database.Queries;

public interface IGetResourceClaimByResourceClaimIdQuery
{
    ResourceClaim Execute(int id);
}

public class GetResourceClaimByResourceClaimIdQuery : IGetResourceClaimByResourceClaimIdQuery
{
    private readonly ISecurityContext _securityContext;

    public GetResourceClaimByResourceClaimIdQuery(ISecurityContext securityContext)
    {
        _securityContext = securityContext;
    }
    public ResourceClaim Execute(int id)
    {
        var result = new ResourceClaim();
        var resource = _securityContext.ResourceClaims.FirstOrDefault(x => x.ResourceClaimId == id);
        if (resource != null)
        {
            var children = _securityContext.ResourceClaims.Where(x => x.ParentResourceClaimId == resource.ResourceClaimId);
            result = new ResourceClaim
            {
                Children = children.Select(child => new ResourceClaim()
                {
                    Id = child.ResourceClaimId,
                    Name = child.ResourceName,
                    ParentId = resource.ResourceClaimId,
                    ParentName = resource.ResourceName,
                }).ToList(),
                Name = resource.ResourceName,
                Id = resource.ResourceClaimId
            };
        }
        else
        {
            result = null;
        }

        return result;
    }
}


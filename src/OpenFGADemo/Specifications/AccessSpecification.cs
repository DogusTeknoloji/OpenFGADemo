using OpenFGADemo.Services;

namespace OpenFGADemo.Specifications
{
    public interface IAuthorizationSpecification
    {
        Task<bool> IsSatisfiedAsync();
    }

    public class AccessSpecification : IAuthorizationSpecification
    {
        private readonly IAuthorizeService _authorizeService;
        private readonly string _user;
        private readonly string _resource;
        private readonly string[] _permissions;
        private readonly FgaResultOperator _operator;

        public AccessSpecification(IAuthorizeService authorizeService, string user, string resource, string[] permissions, FgaResultOperator resultOperator)
        {
            _authorizeService = authorizeService;
            _user = user;
            _resource = resource;
            _permissions = permissions;
            _operator = resultOperator;
        }

        public async Task<bool> IsSatisfiedAsync()
        {
            var hasAccessArray = await _authorizeService.CheckAccess(_user, _resource, _permissions);
            if (_operator == FgaResultOperator.Or)
            {
                // Specification satisfied if at least one permission is granted
                return hasAccessArray.Any(x => x);
            }
            else
            {
                // Specification satisfied if all permissions are granted
                return hasAccessArray.All(x => x);
            }
        }
    }
}

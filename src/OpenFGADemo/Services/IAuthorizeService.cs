
namespace OpenFGADemo.Services
{
    public interface IAuthorizeService
    {
        Task<bool> CheckAccess(string userId, string documentId, string permission);
    }
}
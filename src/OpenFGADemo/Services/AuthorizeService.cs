using OpenFga.Sdk.Client;
using OpenFga.Sdk.Client.Model;

namespace OpenFGADemo.Services
{
    public class AuthorizeService : IAuthorizeService
    {
        private readonly OpenFgaClient _openFgaClient;
        private readonly string _storeId;

        public AuthorizeService(OpenFgaClient openFgaClient)
        {
            _openFgaClient = openFgaClient;
            //_storeId = "default"; // Production'da gerçek store ID kullanılmalı
            // 01JPMEMAWSXD1VPBWKSBCVQ4KH
            _storeId = "01JPMEMAWSXD1VPBWKSBCVQ4KH";
        }

        public async Task<bool> CheckAccess(string userId, string documentId, string permission)
        {
            try
            {
                var response = await _openFgaClient.Check(new ClientCheckRequest
                {
                    User = $"user:{userId}",
                    Relation = permission,
                    Object = $"document:{documentId}"
                }, new ClientCheckOptions { StoreId = _storeId });

                return response.Allowed == true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool[]> CheckAccess(string userId, string documentId, params string[] permissions)
        {
            try
            {
                var response = await _openFgaClient.BatchCheck([.. permissions.Select(x=> new ClientCheckRequest
                {
                    User = $"user:{userId}",
                    Relation = x,
                    Object = $"document:{documentId}"
                })], new ClientBatchCheckOptions { StoreId = _storeId });

                return [.. response.Responses.Select(x => x.Allowed)];
            }
            catch
            {
                return [.. permissions.Select(_ => false)];
            }
        }
    }
}
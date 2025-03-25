using Microsoft.AspNetCore.Mvc;
using OpenFga.Sdk.Client;
using OpenFga.Sdk.Client.Model;
using OpenFga.Sdk.Model;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenFGADemo.Controllers;

[ApiController]
[Route("[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly OpenFgaClient _openFgaClient;
    private readonly string _storeId;

    public DocumentsController(OpenFgaClient openFgaClient)
    {
        _openFgaClient = openFgaClient;
        //_storeId = "default"; // Production'da gerçek store ID kullanılmalı 
        // 01JPMEMAWSXD1VPBWKSBCVQ4KH
        _storeId = "01JPMEMAWSXD1VPBWKSBCVQ4KH";
    }

    [HttpPut("create-store")]
    public async Task<IActionResult> CreateStore()
    {
        var configuration = new ClientConfiguration()
        {
            ApiUrl = "http://localhost:8080", // required, e.g. https://api.fga.example
        };
        var fgaClient = new OpenFgaClient(configuration);

        var store = await fgaClient.CreateStore(new ClientCreateStoreRequest() { Name = "FGA Demo Store" });

        return this.Content($"{{ \"storeId\": \"{store.Id}\" }}", "application/json", Encoding.UTF8);
    }



    [HttpGet("list-store")]
    public async Task<IActionResult> ListStore()
    {
        var configuration = new ClientConfiguration()
        {
            ApiUrl = "http://localhost:8080", // required, e.g. https://api.fga.example
        };
        var fgaClient = new OpenFgaClient(configuration);

        var store = await fgaClient.ListStores();

        return this.Content(JsonSerializer.Serialize(store), "application/json", Encoding.UTF8);
    }

    [HttpPut("create")]
    public async Task<IActionResult> CreateModel()
    {
        var modelJson = "{\"schema_version\":\"1.1\",\"type_definitions\":[{\"type\":\"user\"},{\"type\":\"document\",\"relations\":{\"reader\":{\"this\":{}},\"writer\":{\"this\":{}},\"owner\":{\"this\":{}}},\"metadata\":{\"relations\":{\"reader\":{\"directly_related_user_types\":[{\"type\":\"user\"}]},\"writer\":{\"directly_related_user_types\":[{\"type\":\"user\"}]},\"owner\":{\"directly_related_user_types\":[{\"type\":\"user\"}]}}}}]}";
        var body = JsonSerializer.Deserialize<ClientWriteAuthorizationModelRequest>(modelJson);

        var response = await _openFgaClient.WriteAuthorizationModel(body);

        return this.Content(JsonSerializer.Serialize(response));
    }

    [HttpPost("check")]
    public async Task<IActionResult> CheckAccess([FromBody] AuthenticationTuple input)
    {
        try
        {
            var response = await _openFgaClient.Check(new ClientCheckRequest
            {
                User = $"user:{input.UserId}",
                Relation = input.Permission,
                Object = $"document:{input.DocumentId}"
            }, new ClientCheckOptions { StoreId = _storeId });

            if (response.Allowed == true)
            {
                return Ok(response);
            }
            else
            {
                return base.StatusCode(500, response);
            }
            //return Ok(new { allowed = response.Allowed });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("assign")]
    public async Task<IActionResult> AssignPermission([FromQuery] string userId,
                                                      [FromQuery] string documentId,
                                                      [FromQuery] string permission)
    {
        try
        {
            var tupleKeys = new List<ClientTupleKey>
            {
                new() {
                    User = $"user:{userId}",
                    Relation = permission,
                    Object = $"document:{documentId}"
                }
            };

            await _openFgaClient.Write(new ClientWriteRequest
            {
                Writes = tupleKeys
            }, new ClientWriteOptions { StoreId = _storeId });

            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

public record AuthenticationTuple(string UserId, string DocumentId, string Permission);
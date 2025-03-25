namespace OpenFGADemo.Models;

public static class AuthorizationModel
{
    public const string ModelId = "1";
    
    public static readonly string Schema = @"{
      schema_version: ""1.1"",
      type_definitions: [
        {
          type: ""document"",
          relations: {
            reader: {
              this: {}
            },
            writer: {
              this: {}
            },
            owner: {
              this: {}
            }
          },
          metadata: {
            relations: {
              reader: { directly_related_user_types: [{ type: ""user"" }] },
              writer: { directly_related_user_types: [{ type: ""user"" }] },
              owner: { directly_related_user_types: [{ type: ""user"" }] }
            }
          }
        },
        {
          type: ""user"",
          relations: {}
        }
      ]
    }";
}
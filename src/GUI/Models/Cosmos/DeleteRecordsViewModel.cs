using GUI.Configuration;
using Microsoft.Extensions.Options;

namespace GUI.Models.Cosmos;

public partial class DeleteRecordsViewModel : BaseCosmosViewModel
{

    public DeleteRecordsViewModel(IOptions<CosmosAppSettings> cosmosAppSettings) : base(cosmosAppSettings.Value)
    {
    }
    public override void Validate()
    {
        throw new NotImplementedException();
    }
}

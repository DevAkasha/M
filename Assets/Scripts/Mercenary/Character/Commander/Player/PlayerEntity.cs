using Akasha;
using UnityEngine;

[GenerateWrappers]
public sealed partial class PlayerEntity : BaseEntity<PlayerModel>
{
    protected override PlayerModel SetupModel()
    {
        return DataManager.Instance.CreateModel<PlayerModel>();
    }
}

using Akasha;
using UnityEngine;

public sealed class PlayerEntity : BaseEntity<PlayerModel>
{
    protected override PlayerModel SetupModel()
    {
        return DataManager.Instance.CreateModel<PlayerModel>();
    }
}

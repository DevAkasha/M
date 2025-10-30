using Akasha;
using UnityEngine;

public sealed class PlayerModel : BaseModel
{
    public RxVar<float> MoveSpeed;
    public RxMod<float> AttackPower;
    public RxMod<int> Health;
    public RxComputed<float> TotalDamage;
    
    public PlayerModel()
    {
        MoveSpeed = new RxVar<float>(5f, nameof(MoveSpeed), this);
        AttackPower = new RxMod<float>(10f, nameof(AttackPower), this);
        Health = new RxMod<int>(100, nameof(Health), this);
        TotalDamage = CreateComputed(() => AttackPower.Value * 1.5f, nameof(TotalDamage));
    }
}

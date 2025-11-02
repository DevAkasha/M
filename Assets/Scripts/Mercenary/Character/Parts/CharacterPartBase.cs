using Akasha;
using UnityEngine;

/// <summary>
/// 모든 캐릭터 Part의 기본 클래스
/// </summary>
public abstract class CharacterPartBase<TEntity, TModel> : BasePart<TEntity, TModel>
    where TEntity : BaseEntity<TModel>
    where TModel : CharModel
{
    // 캐릭터의 기본 스탯에 쉽게 접근하기 위한 프로퍼티들
    protected int Strength => Model.Strength.Value;
    protected int Control => Model.Control.Value;
    protected int Intelligence => Model.Intelligence.Value;
    protected int Divine => Model.Divine.Value;
    protected int Agility => Model.Agility.Value;
    protected int Spirit => Model.Spirit.Value;
    protected int Luck => Model.Luck.Value;

    protected float MeleeAttack => Model.MeleeAttack.Value;
    protected float RangedAttack => Model.RangedAttack.Value;
    protected float MagicAttack => Model.MagicAttack.Value;
    protected float DivineAttack => Model.DivineAttack.Value;

    protected int CurrentHP => Model.CurrentHP.Value;
    protected int MaxHP => Model.MaxHP.Value;
    protected int CurrentMP => Model.CurrentMP.Value;
    protected int MaxMP => Model.MaxMP.Value;

    protected bool IsAlive => Model.IsAlive.Value;
    protected int Level => Model.Level.Value;
}

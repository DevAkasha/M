using Akasha;
using UnityEngine;

public class UnitController<E, M> : CharController<E, M> where E : BaseEntity<M> where M : BaseModel
{

}
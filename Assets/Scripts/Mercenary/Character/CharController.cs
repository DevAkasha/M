using UnityEngine;
using Akasha;

public abstract class CharController<E,M> : Controller<E,M> where E : BaseEntity<M> where M : BaseModel
{

}
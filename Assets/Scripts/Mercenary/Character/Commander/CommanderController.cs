using Akasha;
using UnityEngine;

public abstract class CommanderController<E, M> : CharController<E, M> where E : BaseEntity<M> where M : BaseModel
{

}

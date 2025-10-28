using Akasha;
using UnityEngine;

public class BossController<E, M> : CommanderController<E, M> where E : BaseEntity<M> where M : BaseModel
{

}
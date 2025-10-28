
using Akasha;

public abstract class HeroController<E, M> : CommanderController<E, M> where E : BaseEntity<M> where M : BaseModel
{

}
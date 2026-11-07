using FarmGame2D.Player;
using UnityEngine;

namespace FarmGame2D.Interactions
{
    [RequireComponent(typeof(Collider2D))]
    public abstract class InteractableBehaviour : MonoBehaviour
    {
        public abstract string Prompt { get; }
        public abstract void Interact(PrototypePlayerController player);

        protected virtual void Reset()
        {
            var interactionCollider = GetComponent<Collider2D>();
            interactionCollider.isTrigger = true;
        }
    }
}

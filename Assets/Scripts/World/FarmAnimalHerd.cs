using System.Collections.Generic;
using FarmGame2D.Art;
using FarmGame2D.Core;
using FarmGame2D.Interactions;
using UnityEngine;

namespace FarmGame2D.World
{
    public enum FarmAnimalKind
    {
        Chicken,
        Cow
    }

    public sealed class FarmAnimalHerd : MonoBehaviour
    {
        private readonly List<GameObject> animals = new();

        private GameSession session;
        private FarmAnimalKind animalKind;
        private WanderArea roamingArea;
        private float moveSpeed;
        private int separationGroup;

        public int VisibleCount => animals.Count;

        public void Configure(
            GameSession gameSession,
            FarmAnimalKind kind,
            WanderArea area,
            float speed,
            int group)
        {
            session = gameSession;
            animalKind = kind;
            roamingArea = area;
            moveSpeed = speed;
            separationGroup = group;
            session.StateChanged += Synchronize;
            Synchronize();
        }

        private void OnDestroy()
        {
            if (session != null)
            {
                session.StateChanged -= Synchronize;
            }
        }

        private void Synchronize()
        {
            var targetCount = animalKind == FarmAnimalKind.Chicken
                ? session.Data.ownedChickens
                : session.Data.ownedCows;

            while (animals.Count < targetCount)
            {
                animals.Add(CreateAnimal(animals.Count));
            }

            while (animals.Count > targetCount)
            {
                var lastIndex = animals.Count - 1;
                Destroy(animals[lastIndex]);
                animals.RemoveAt(lastIndex);
            }
        }

        private GameObject CreateAnimal(int index)
        {
            var displayName = animalKind == FarmAnimalKind.Chicken ? "Chicken" : "Cow";
            var animal = new GameObject(index == 0 ? displayName : $"{displayName} {index + 1}");
            animal.transform.SetParent(transform);
            animal.transform.position = GetSpawnPosition(index);

            var renderer = animal.AddComponent<SpriteRenderer>();
            renderer.sprite = WorldArtCatalog.GetSprite(animalKind == FarmAnimalKind.Chicken
                ? WorldArtKind.Chicken
                : WorldArtKind.Cow);
            animal.AddComponent<WorldDepthSorter>().Configure(2);

            var collider = animal.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = animalKind == FarmAnimalKind.Chicken
                ? new Vector2(0.8f, 0.55f)
                : new Vector2(1.3f, 0.8f);

            if (animalKind == FarmAnimalKind.Chicken)
            {
                animal.AddComponent<ChickenInteractable>();
            }
            else
            {
                animal.AddComponent<CowInteractable>();
            }

            var mover = animal.AddComponent<WanderingMover>();
            mover.Configure(roamingArea, moveSpeed, 1701 + (index * 37) + separationGroup, separationGroup);
            return animal;
        }

        private Vector2 GetSpawnPosition(int index)
        {
            if (index == 0)
            {
                return roamingArea.Center;
            }

            var x01 = Mathf.Repeat(0.5f + (index * 0.618034f), 1f);
            var y01 = Mathf.Repeat(0.5f + (index * 0.414214f), 1f);
            return roamingArea.GetPoint(x01, y01);
        }
    }
}

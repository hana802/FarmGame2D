using FarmGame2D.Art;
using FarmGame2D.Interactions;
using UnityEngine;

namespace FarmGame2D.World
{
    public sealed partial class PrototypeWorldBuilder
    {
        private void BuildTownHall(Transform town)
        {
            var entrance = CreateInteractable<DoorInteractable>("Town Hall Entrance", new Vector2(0f, 10.55f),
                new Vector2(1.2f, 0.55f), Color.clear, town);
            entrance.Configure(WorldAreaProfile.TownHallEntry, "시청에 들어가기", string.Empty);

            var hall = CreateWorldArt("Town Hall Interior", WorldArtKind.TownHallInterior,
                WorldAreaProfile.TownHallCenter, town, 0);
            var room = hall.transform;
            var roomSprite = hall.GetComponent<SpriteRenderer>().sprite;

            // Colliders line up with the furnishings and perimeter in the room artwork.
            AddHallBlocker(room, roomSprite, "North Wall", Rect.MinMaxRect(236, 250, 1438, 285));
            AddHallBlocker(room, roomSprite, "West Wall", Rect.MinMaxRect(236, 275, 264, 896));
            AddHallBlocker(room, roomSprite, "East Wall", Rect.MinMaxRect(1408, 275, 1438, 896));
            AddHallBlocker(room, roomSprite, "South Wall Left", Rect.MinMaxRect(236, 843, 728, 903));
            AddHallBlocker(room, roomSprite, "South Wall Right", Rect.MinMaxRect(944, 843, 1438, 903));
            AddHallBlocker(room, roomSprite, "Threshold Edge", Rect.MinMaxRect(728, 898, 944, 919));
            AddHallBlocker(room, roomSprite, "Counter", Rect.MinMaxRect(639, 275, 1034, 443));
            AddHallBlocker(room, roomSprite, "Noticeboard", Rect.MinMaxRect(281, 351, 499, 643));
            AddHallBlocker(room, roomSprite, "Meeting Table", Rect.MinMaxRect(1117, 432, 1393, 614));

            var exit = CreateInteractable<DoorInteractable>("Town Hall Interior Exit",
                HallArtPoint(roomSprite, new Vector2(836, 874)), new Vector2(1.3f, 0.55f), Color.clear, room);
            exit.Configure(WorldAreaProfile.TownHallExit, "광장으로 나가기", string.Empty);

            CreateNoticeEntrance("Town Hall Reception", HallArtPoint(roomSprite, new Vector2(836, 451)),
                "안내 데스크 살펴보기", "장터는 광장 동쪽, 항구는 해안길 끝에 있어요.", room);
            CreateInteractable<TownHallNoticeboardInteractable>("Town Hall Noticeboard",
                HallArtPoint(roomSprite, new Vector2(394, 649)), new Vector2(1.5f, 0.55f), Color.clear, room);
        }

        private static void AddHallBlocker(Transform room, Sprite sprite, string name, Rect artBounds)
        {
            var center = HallArtPoint(sprite, artBounds.center);
            CreateSolidCollider($"Town Hall {name}", room, center - (Vector2)room.position,
                artBounds.size / sprite.pixelsPerUnit);
        }

        private static Vector2 HallArtPoint(Sprite sprite, Vector2 artPoint)
        {
            var fromBottom = new Vector2(artPoint.x, sprite.rect.height - artPoint.y);
            return WorldAreaProfile.TownHallCenter + (fromBottom - sprite.pivot) / sprite.pixelsPerUnit;
        }
    }
}

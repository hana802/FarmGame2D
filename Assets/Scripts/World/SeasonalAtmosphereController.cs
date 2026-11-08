using FarmGame2D.Core;
using UnityEngine;

namespace FarmGame2D.World
{
    [RequireComponent(typeof(Camera))]
    public sealed class SeasonalAtmosphereController : MonoBehaviour
    {
        private const int WeatherSortingOrder = 31990;
        private const int LightingSortingOrder = 32000;

        private Camera worldCamera;
        private Transform player;
        private SpriteRenderer lightingRenderer;
        private ParticleSystem weatherParticles;
        private ParticleSystemRenderer weatherRenderer;
        private Texture2D lightingTexture;
        private Texture2D weatherTexture;
        private Material weatherMaterial;
        private Font koreanFont;
        private GUIStyle remarkStyle;
        private Season currentSeason = (Season)(-1);
        private SeasonalWeatherProfile currentWeather;
        private bool summerEntryPending;
        private int lastSummerRemarkDay = -1;
        private float remarkVisibleUntil;
        private string remarkText = string.Empty;

        public Season CurrentSeason => currentSeason;
        public SpriteRenderer LightingRenderer => lightingRenderer;
        public ParticleSystem WeatherParticles => weatherParticles;

        public void Configure(Transform playerTransform)
        {
            player = playerTransform;
        }

        private void Awake()
        {
            worldCamera = GetComponent<Camera>();
            CreateLightingOverlay();
            CreateWeatherSystem();
            koreanFont = Resources.Load<Font>("Fonts/NotoSansKR-VF")
                ?? Font.CreateDynamicFontFromOSFont(new[] { "Noto Sans KR", "Malgun Gothic", "Arial" }, 18);
        }

        private void Update()
        {
            var session = GameSession.Instance;
            if (session == null)
            {
                return;
            }

            UpdateLighting(session);
            UpdateWeather(session);
            UpdateSummerRemark(session);
        }

        private void OnDestroy()
        {
            if (weatherMaterial != null)
            {
                Destroy(weatherMaterial);
            }

            if (weatherTexture != null)
            {
                Destroy(weatherTexture);
            }

            if (lightingTexture != null)
            {
                Destroy(lightingTexture);
            }
        }

        private void CreateLightingOverlay()
        {
            lightingTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                name = "Runtime Time Lighting Texture",
                filterMode = FilterMode.Point
            };
            lightingTexture.SetPixel(0, 0, Color.white);
            lightingTexture.Apply();

            var sprite = Sprite.Create(lightingTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            sprite.name = "Runtime Time Lighting Sprite";
            var overlayObject = new GameObject("Time Lighting Overlay");
            overlayObject.transform.SetParent(transform, false);
            overlayObject.transform.localPosition = new Vector3(0f, 0f, 9f);
            lightingRenderer = overlayObject.AddComponent<SpriteRenderer>();
            lightingRenderer.sprite = sprite;
            lightingRenderer.sortingOrder = LightingSortingOrder;
            lightingRenderer.color = SeasonalAtmosphereProfile.GetLighting(6, 0);
            ResizeScreenEffects();
        }

        private void CreateWeatherSystem()
        {
            var weatherObject = new GameObject("Seasonal Weather Particles");
            weatherObject.transform.SetParent(transform, false);
            weatherObject.transform.localPosition = new Vector3(0f, worldCamera.orthographicSize + 1f, 8.5f);
            weatherParticles = weatherObject.AddComponent<ParticleSystem>();
            weatherRenderer = weatherObject.GetComponent<ParticleSystemRenderer>();
            weatherRenderer.sortingOrder = WeatherSortingOrder;

            var main = weatherParticles.main;
            main.loop = true;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.startSpeed = 0f;
            main.startLifetime = 12f;
            main.maxParticles = 500;

            var shape = weatherParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;

            var noise = weatherParticles.noise;
            noise.enabled = true;
            noise.strength = 0.22f;
            noise.frequency = 0.35f;
            noise.scrollSpeed = 0.18f;

            var rotation = weatherParticles.rotationOverLifetime;
            rotation.enabled = true;
            rotation.z = new ParticleSystem.MinMaxCurve(-1.2f, 1.2f);
        }

        private void UpdateLighting(GameSession session)
        {
            ResizeScreenEffects();
            var targetColor = SeasonalAtmosphereProfile.GetLighting(session.Data.hour, session.Data.minute);
            lightingRenderer.color = Color.Lerp(lightingRenderer.color, targetColor, Time.deltaTime * 1.8f);
        }

        private void ResizeScreenEffects()
        {
            var height = (worldCamera.orthographicSize * 2f) + 1f;
            var width = (height * worldCamera.aspect) + 1f;
            lightingRenderer.transform.localScale = new Vector3(width, height, 1f);

            if (weatherParticles == null)
            {
                return;
            }

            weatherParticles.transform.localPosition = new Vector3(0f, worldCamera.orthographicSize + 1f, 8.5f);
            var shape = weatherParticles.shape;
            shape.scale = new Vector3(width, 0.25f, 0.1f);
        }

        private void UpdateWeather(GameSession session)
        {
            var seasonChanged = currentSeason != session.CurrentSeason;
            if (seasonChanged)
            {
                currentSeason = session.CurrentSeason;
                summerEntryPending = currentSeason == Season.Summer;
                ConfigureWeather(currentSeason);
            }

            var outdoors = player == null || player.position.x < 30f;
            var emission = weatherParticles.emission;
            emission.rateOverTime = outdoors ? currentWeather.EmissionRate : 0f;
        }

        private void ConfigureWeather(Season season)
        {
            currentWeather = SeasonalAtmosphereProfile.GetWeather(season);
            weatherParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = weatherParticles.main;
            main.startColor = currentWeather.Color;
            main.startSize = new ParticleSystem.MinMaxCurve(currentWeather.SizeRange.x, currentWeather.SizeRange.y);

            var velocity = weatherParticles.velocityOverLifetime;
            velocity.enabled = currentWeather.EmissionRate > 0f;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.x = currentWeather.Velocity.x;
            velocity.y = currentWeather.Velocity.y;

            ApplyWeatherMaterial(season);
            if (currentWeather.EmissionRate > 0f)
            {
                weatherParticles.Play();
            }
        }

        private void ApplyWeatherMaterial(Season season)
        {
            if (weatherMaterial != null)
            {
                Destroy(weatherMaterial);
            }

            if (weatherTexture != null)
            {
                Destroy(weatherTexture);
            }

            weatherTexture = CreateWeatherTexture(season);
            var shader = Shader.Find("Sprites/Default");
            weatherMaterial = new Material(shader) { name = $"Runtime {season} Weather Material" };
            weatherMaterial.mainTexture = weatherTexture;
            weatherRenderer.material = weatherMaterial;
        }

        private static Texture2D CreateWeatherTexture(Season season)
        {
            const int size = 20;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = $"Runtime {season} Weather Texture",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var normalizedX = ((x + 0.5f) / size * 2f) - 1f;
                    var normalizedY = ((y + 0.5f) / size * 2f) - 1f;
                    texture.SetPixel(x, y, GetWeatherPixelColor(season, normalizedX, normalizedY));
                }
            }

            texture.Apply();
            return texture;
        }

        private static Color GetWeatherPixelColor(Season season, float x, float y)
        {
            return season switch
            {
                Season.Spring => GetCherryBlossomPixelColor(x, y),
                Season.Winter when (x * x) + (y * y) < 0.72f => Color.white,
                Season.Autumn when Mathf.Abs(x) + Mathf.Abs(y) < 0.86f => Color.white,
                _ => Color.clear
            };
        }

        private static Color GetCherryBlossomPixelColor(float x, float y)
        {
            var leftLobe = ((x + 0.28f) * (x + 0.28f)) + ((y - 0.18f) * (y - 0.18f)) < 0.32f;
            var rightLobe = ((x - 0.28f) * (x - 0.28f)) + ((y - 0.18f) * (y - 0.18f)) < 0.32f;
            var taperWidth = Mathf.Lerp(0.08f, 0.5f, Mathf.InverseLerp(-0.92f, 0.28f, y));
            var pointedBase = y is > -0.92f and < 0.28f && Mathf.Abs(x) < taperWidth;
            var topNotch = y > 0.48f && Mathf.Abs(x) < 0.11f;
            if ((!leftLobe && !rightLobe && !pointedBase) || topNotch)
            {
                return Color.clear;
            }

            var highlight = Mathf.InverseLerp(-0.85f, 0.65f, y);
            return Color.Lerp(
                new Color(1f, 0.48f, 0.68f, 1f),
                new Color(1f, 0.9f, 0.96f, 1f),
                highlight);
        }

        private void UpdateSummerRemark(GameSession session)
        {
            if (session.Data.characterChoice == CharacterChoice.None)
            {
                return;
            }

            if (!SeasonalAtmosphereProfile.ShouldShowSummerRemark(
                    currentSeason,
                    session.Data.day,
                    session.Data.hour,
                    lastSummerRemarkDay,
                    summerEntryPending))
            {
                return;
            }

            lastSummerRemarkDay = session.Data.day;
            summerEntryPending = false;
            remarkText = session.Data.day % 2 == 0
                ? "후아… 오늘 햇볕이 정말 뜨거워!"
                : "덥다… 시원한 그늘에서 잠깐 쉬고 싶어.";
            remarkVisibleUntil = Time.unscaledTime + 3.5f;
        }

        private void OnGUI()
        {
            if (player == null || worldCamera == null || Time.unscaledTime >= remarkVisibleUntil)
            {
                return;
            }

            remarkStyle ??= new GUIStyle(GUI.skin.label)
            {
                font = koreanFont,
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                normal = { textColor = new Color32(91, 52, 31, 255) }
            };

            var screenPoint = worldCamera.WorldToScreenPoint(player.position + (Vector3.up * 1.55f));
            var width = Mathf.Min(330f, Screen.width * 0.42f);
            var rect = new Rect(
                Mathf.Clamp(screenPoint.x - (width * 0.5f), 12f, Screen.width - width - 12f),
                Mathf.Clamp(Screen.height - screenPoint.y - 56f, 12f, Screen.height - 70f),
                width,
                52f);
            var previousColor = GUI.color;
            GUI.color = new Color(0.32f, 0.18f, 0.1f, 0.96f);
            GUI.DrawTexture(new Rect(rect.x - 3f, rect.y - 3f, rect.width + 6f, rect.height + 6f), lightingTexture);
            GUI.color = new Color(1f, 0.93f, 0.78f, 0.98f);
            GUI.DrawTexture(rect, lightingTexture);
            GUI.color = previousColor;
            GUI.Label(new Rect(rect.x + 12f, rect.y + 4f, rect.width - 24f, rect.height - 8f), remarkText, remarkStyle);
        }
    }
}

using UnityEngine;

namespace Farm.Data
{
    public enum WeatherParticles { None = 0, Rain = 1, Snow = 2, Wind = 3 }

    // One kind of weather, as data. Everything the game does differently in the rain (or snow, or fog from a module)
    // comes from these fields, so no system needs to know weather ids. The display name is the string key
    // "weather.<id>". Weather ids are stable once shipped (they are saved).
    [CreateAssetMenu(menuName = "Farm/Weather")]
    public sealed class WeatherDefinition : ScriptableObject
    {
        public const int SeasonCount = 4;

        [SerializeField] string _id;
        [SerializeField] Color _tint = Color.white;               // blended over the outdoor light
        [SerializeField, Range(0f, 1f)] float _tintStrength;      // 0 = no change
        [SerializeField] bool _wateringCrops;                     // waters every crop overnight into this weather
        [SerializeField] WeatherParticles _particles;
        [SerializeField, Range(0f, 1f)] float _particleDensity = 0.5f;
        [SerializeField, Range(-1f, 1f)] float _windSlant;        // sideways drift of the particles
        [SerializeField] bool _lightning;
        // Relative chance of being rolled on a day in each season (spring, summer, fall, winter). 0 = never.
        [SerializeField] float[] _seasonWeights = { 1f, 1f, 1f, 1f };

        public string Id => _id;
        public string NameKey => "weather." + _id;
        public Color Tint => _tint;
        public float TintStrength => _tintStrength;
        public bool WateringCrops => _wateringCrops;
        public WeatherParticles Particles => _particles;
        public float ParticleDensity => _particleDensity;
        public float WindSlant => _windSlant;
        public bool Lightning => _lightning;

        public float WeightFor(int seasonIndex) =>
            _seasonWeights != null && seasonIndex >= 0 && seasonIndex < _seasonWeights.Length
                ? Mathf.Max(0f, _seasonWeights[seasonIndex]) : 0f;

        public Color Apply(Color light) => _tintStrength <= 0f ? light : Color.Lerp(light, _tint, _tintStrength);

        public static WeatherDefinition Create(string id, Color tint, float tintStrength, bool wateringCrops,
            WeatherParticles particles, float density, float slant, bool lightning, params float[] seasonWeights)
        {
            var w = CreateInstance<WeatherDefinition>();
            w._id = id;
            w.name = id;
            w._tint = tint;
            w._tintStrength = tintStrength;
            w._wateringCrops = wateringCrops;
            w._particles = particles;
            w._particleDensity = density;
            w._windSlant = slant;
            w._lightning = lightning;
            w._seasonWeights = seasonWeights != null && seasonWeights.Length == SeasonCount
                ? (float[])seasonWeights.Clone() : new[] { 1f, 1f, 1f, 1f };
            return w;
        }

        // A weather id nobody defined (one a module sets, say): behaves like plain weather.
        public static WeatherDefinition Neutral(string id) =>
            Create(id ?? string.Empty, Color.white, 0f, false, WeatherParticles.None, 0f, 0f, false, 0f, 0f, 0f, 0f);
    }

    // The base game's weather. The editor content tool writes these out as assets so they can be tuned in the
    // inspector; tests and any project without the assets use them directly.
    public static class WeatherDefaults
    {
        public const string Sunny = "sunny";
        public const string Rain = "rain";
        public const string Storm = "storm";
        public const string Snow = "snow";
        public const string Wind = "wind";

        // Season weights are spring, summer, fall, winter.
        public static WeatherDefinition[] CreateAll() => new[]
        {
            WeatherDefinition.Create(Sunny, Color.white, 0f, false, WeatherParticles.None, 0f, 0f, false, 60f, 65f, 50f, 55f),
            WeatherDefinition.Create(Rain, new Color(0.55f, 0.60f, 0.70f), 0.45f, true, WeatherParticles.Rain, 0.6f, 0.1f, false, 25f, 18f, 25f, 0f),
            WeatherDefinition.Create(Storm, new Color(0.40f, 0.43f, 0.55f), 0.60f, true, WeatherParticles.Rain, 1f, 0.35f, true, 5f, 12f, 5f, 0f),
            WeatherDefinition.Create(Snow, new Color(0.85f, 0.90f, 1f), 0.25f, false, WeatherParticles.Snow, 0.5f, 0.05f, false, 0f, 0f, 0f, 40f),
            WeatherDefinition.Create(Wind, Color.white, 0f, false, WeatherParticles.Wind, 0.5f, 1f, false, 10f, 5f, 20f, 5f),
        };
    }
}

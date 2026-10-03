using UnityEngine;
using UniverseGeneration.Samples;

/// <summary>
/// Add this to any GameObject and press Play: the galaxy of the seed is written to the console. Change the seed in the
/// Inspector; the same seed gives the same galaxy on every platform.
/// </summary>
public class GalaxyPrinter : MonoBehaviour
{
    [SerializeField] private string seed = "my-seed";
    [SerializeField, Range(1, 2000)] private int systems = 60;

    private void Start()
    {
        foreach (var line in GalaxyReport.Lines(seed, systems))
        {
            Debug.Log(line);
        }
    }
}

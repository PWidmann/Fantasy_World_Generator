using UnityEngine;
using UnityEngine.UI;

public class PlasmaFractal : MonoBehaviour
{
    [SerializeField] private RawImage _rawImage;

    private Texture2D _texture;

    private const int Width = 256;
    private const int Height = 256;

    private void Start()
    {
        // Initialize the texture
        _texture = new Texture2D(Width, Height);
        _rawImage.texture = _texture;

        // Generate the plasma fractal
        GeneratePlasmaFractal();
    }

    private void GeneratePlasmaFractal()
    {
        // Initialize the color array
        Color[] colors = new Color[Width * Height];

        // Calculate the plasma fractal
        float[] values = new float[Width * Height];
        float roughness = 0.5f;
        int period = 16;

        for (int octave = 0; octave < 4; octave++)
        {
            float frequency = Mathf.Pow(2, octave);
            int frequencyPeriod = Mathf.FloorToInt(period * frequency);

            for (int y = 0; y < Height; y++)
            {
                int y0 = (y / frequencyPeriod) * frequencyPeriod;
                int y1 = (y0 + frequencyPeriod) % Height;
                float yf = (y - y0) / (float)frequencyPeriod;

                for (int x = 0; x < Width; x++)
                {
                    int x0 = (x / frequencyPeriod) * frequencyPeriod;
                    int x1 = (x0 + frequencyPeriod) % Width;
                    float xf = (x - x0) / (float)frequencyPeriod;

                    float v00 = values[y0 * Width + x0];
                    float v01 = values[y0 * Width + x1];
                    float v10 = values[y1 * Width + x0];
                    float v11 = values[y1 * Width + x1];

                    float i0 = Mathf.Lerp(v00, v01, xf);
                    float i1 = Mathf.Lerp(v10, v11, xf);
                    float val = Mathf.Lerp(i0, i1, yf);

                    values[y * Width + x] += val * roughness;
                }
            }

            roughness *= 0.5f;
        }

        // Normalize the values
        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        for (int i = 0; i < Width * Height; i++)
        {
            minValue = Mathf.Min(minValue, values[i]);
            maxValue = Mathf.Max(maxValue, values[i]);
        }

        for (int i = 0; i < Width * Height; i++)
        {
            values[i] = Mathf.InverseLerp(minValue, maxValue, values[i]);
        }

        // Set the colors based on the normalized values
        for (int i = 0; i < Width * Height; i++)
        {
            float value = values[i];
            Color color = new Color(value, value, value, 1);
            colors[i] = color;
        }

        // Apply the texture to the RawImage component
        _texture.SetPixels(colors);
        _texture.Apply();
    }
}

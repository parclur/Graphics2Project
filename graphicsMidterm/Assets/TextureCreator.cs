using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Reference: https://catlikecoding.com/unity/tutorials/noise/
// procedurally creates a noise texture
public class TextureCreator : MonoBehaviour {

    // customize the noise texture
    [Range(1, 8)]
    public int octaves = 1;

    [Range(1f, 4f)]
    public float lacunarity = 2f;

    [Range(0f, 1f)]
    public float persistence = 0.5f;

    [Range(1, 3)]
    public int dimensions = 3;

    [Range(2, 512)] // constrains the resolution to a range
    public int resolution = 256; // number of pixels

    public NoiseMethodType type;

    public Gradient coloring;

    private Texture2D texture;
    public float frequency = 1f;

    // creates the texture when the component is activated
    // can use Awake, but OnEnable allows changes to the texture to be applied after a recompile in play mode
    private void OnEnable()
    {
        if (texture == null) // makes sure a new texture isn't created each time the component is enabled
        {
            texture = new Texture2D(resolution, resolution, TextureFormat.RGB24, true); //public Texture2D(int width, int height, TextureFormat format, bool mipmap);
            texture.name = "Procedural Texture"; // give it a descriptive name
            texture.wrapMode = TextureWrapMode.Clamp; // keeps the default wrap mode of textures from repeating themselves
            texture.filterMode = FilterMode.Trilinear; // trilinear filtering - texture samples are averaged and also blended between mipmap levels
            texture.anisoLevel = 9; // ansiotropic filtering - imporves the texture from getting fuzzy quick when viewed at an angle; range is 1-9
            GetComponent<MeshRenderer>().material.mainTexture = texture; // get the component from the game object and directly assign the texture to its material
        }
        FillTexture();
    }

    // Fills the texture with the noise
    public void FillTexture()
    {
        // checks whether the resolution has changed, and if so resizes the texture
        if (texture.width != resolution)
        {
            texture.Resize(resolution, resolution);
        }

        // defines the local coordinates of the quad and transforms them into world space
        Vector3 point00 = transform.TransformPoint(new Vector3(-0.5f, -0.5f));
        Vector3 point10 = transform.TransformPoint(new Vector3(0.5f, -0.5f));
        Vector3 point01 = transform.TransformPoint(new Vector3(-0.5f, 0.5f));
        Vector3 point11 = transform.TransformPoint(new Vector3(0.5f, 0.5f));

        NoiseMethod method = Noise.noiseMethods[(int)type][dimensions - 1];

        float stepSize = 1f / resolution; // color channels are defined in a 0-1 range, divide by the resolution to maintain that
        //Random.seed = 42;

        // double for loop goes through each pixel to assign a color
        for (int y = 0; y < resolution; y++)
        {
            Vector3 point0 = Vector3.Lerp(point00, point01, (y + 0.5f) * stepSize);
            Vector3 point1 = Vector3.Lerp(point10, point11, (y + 0.5f) * stepSize);
            for (int x = 0; x < resolution; x++)
            {
                Vector3 point = Vector3.Lerp(point0, point1, (x + 0.5f) * stepSize);
                float sample = Noise.Sum(method, point, frequency, octaves, lacunarity, persistence);
                // Perlin noise can procude both positive and negative values (-1,1)
                // scale and offset the noise samples so -1 becomes 0, 0 becomes 0.5, and 1 stays 1
                if (type != NoiseMethodType.Value)
                {
                    sample = sample * 0.5f + 0.5f;
                }
                texture.SetPixel(x, y, coloring.Evaluate(sample));
            }
        }
        texture.Apply(); //applys the assigned pixel colors to the texture
    }


    // causes the texture to change if the quad is moved
    private void Update()
    {
        if (transform.hasChanged)
        {
            transform.hasChanged = false;
            FillTexture();
        }
    }
}

// This code is related to an answer I provided in the Unity forums at:
// http://forum.unity3d.com/threads/circular-fade-in-out-shader.344816/

using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Screen Transition")]
public class ScreenTransitionImageEffect : MonoBehaviour
{
    /// Provides a shader property that is set in the inspector
    /// and a material instantiated from the shader
    public Shader shader;

    [Range(0, 1.0f)]
    public float maskValue = .80f;
    public Color maskColor = Color.black;
    public Texture2D maskTexture;
    public bool maskInvert;

    private Material m_Material;
    private bool m_maskInvert;
    private bool flipYAxis = true;

    Material material
    {
        get
        {
            if (m_Material == null)
            {
                m_Material = new Material(shader);
                m_Material.hideFlags = HideFlags.HideAndDontSave;
            }
            return m_Material;
        }
    }

    void Start()
    {
        // Disable if we don't support image effects
        if (!SystemInfo.supportsImageEffects)
        {
            enabled = false;
            return;
        }

        shader = Shader.Find("Hidden/ScreenTransitionImageEffect");

        // Disable the image effect if the shader can't
        // run on the users graphics card
        if (shader == null || !shader.isSupported)
            enabled = false;

    }

    void OnDisable()
    {
        if (m_Material)
        {
            DestroyImmediate(m_Material);
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!enabled)
        {
            Graphics.Blit(source, destination);
            return;
        }

        material.SetColor("_MaskColor", maskColor);
        material.SetFloat("_MaskValue", maskValue);
        material.SetTexture("_MainTex", source);
        material.SetTexture("_MaskTex", maskTexture);

        if (material.IsKeywordEnabled("INVERT_MASK") != maskInvert)
        {
            if (maskInvert)
                material.EnableKeyword("INVERT_MASK");
            else
                material.DisableKeyword("INVERT_MASK");
        }

        //The following code was a modification from the original, such that it provides a fix to the error the flipped the axis of the image the camera rendering. 
        if(flipYAxis) {
            Camera.main.projectionMatrix = Camera.main.projectionMatrix * Matrix4x4.Scale(new Vector3(1, -1, 1));
            flipYAxis = false;
        }

        //RenderTexture mask = RenderTexture.GetTemporary(1920, 1080);
        //Wrap the texture.
        Graphics.Blit(source, destination, material);
        //Blit the wrapped texture onto the camera output.
        //Graphics.Blit(mask, destination);
        //Blit the camera output onto the texture for use on a model.
        //Graphics.Blit(mask, material);
        //RenderTexture.ReleaseTemporary(mask);


    }


}

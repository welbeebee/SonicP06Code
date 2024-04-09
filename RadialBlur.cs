using UnityEngine;

[ExecuteInEditMode]
public class RadialBlur : MonoBehaviour
{
	public Shader rbShader;

	public int Samples = 16;

	public float EffectAmount = 2f;

	public float Radius;

	internal Vector2 VelocityPred;

	private Material RBMaterial;

	private Material GetMaterial()
	{
		if (RBMaterial == null)
		{
			RBMaterial = new Material(rbShader);
			RBMaterial.hideFlags = HideFlags.HideAndDontSave;
		}
		return RBMaterial;
	}

	private void Start()
	{
		if (rbShader == null)
		{
			Debug.LogError("shader missing!", this);
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture dest)
	{
		Material material = GetMaterial();
		material.SetFloat("_Samples", Samples);
		material.SetFloat("_EffectAmount", EffectAmount);
		material.SetFloat("_Radius", Radius);
		material.SetVector("_VelocityPred", new Vector4(VelocityPred.x, VelocityPred.y, 0f, 0f));
		Graphics.Blit(source, dest, material);
	}
}

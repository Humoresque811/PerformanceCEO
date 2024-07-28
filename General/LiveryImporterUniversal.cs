using AirportCEOModLoader.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PerformanceCEO.General;

internal class LiveryImporterUniversal
{
	public static GameObject LoadLivery(string directoryPath, string airlineName, out LiveryData liveryData)
	{
		try
        {
            string[] files = Directory.GetFiles(directoryPath, "*.json");
            if (files.Length == 0)
            {
                PerformanceCEO.LogError($"No JSON Files at path? Error! Path {directoryPath}");
                liveryData = null;
                return null;
            }

            liveryData = JsonConvert.DeserializeObject<LiveryData>(Utils.ReadFile(files[0]));
            GameObject gameObject = new GameObject(liveryData.aircraftType + "_" + airlineName.ToUpper() + "_LIV");
            gameObject.transform.SetParent(SingletonNonDestroy<LiveryController>.Instance.modfolder);
            Livery livery = gameObject.AddComponent<Livery>();
            livery.airlineName = airlineName;
            livery.aircraftType = liveryData.aircraftType;
            livery.isSpecial = liveryData.isSpecial;
            string[] files2 = Directory.GetFiles(directoryPath, "*.png");

            if (files2.Length == 0)
            {
                PerformanceCEO.LogError("No PNG Files at path? Error!");
                return null;
            }

            byte[] data = File.ReadAllBytes(files2[0]);
            Texture2D texture2D = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            texture2D.LoadImage(data);

            float downscaleAmount = GetDownscaleFloat();

            if (PerformanceCEOConfig.DownscaleLevel.Value != DownCompressor.DownCompressionEnums.DownscaleLevel.Original)
            {
                int newX = Utils.RoundToIntLikeANormalPerson((float)texture2D.width / downscaleAmount);
                int newY = Utils.RoundToIntLikeANormalPerson((float)texture2D.height / downscaleAmount);

                texture2D = DownscaleTextureFastGPU(texture2D, newX, newY);
            }

            if (PerformanceCEOConfig.CompressTextures.Value)
            {
                texture2D.Compress(true);
            }

            if (PerformanceCEOConfig.RAMReductionModuleEnabled.Value && texture2D.isReadable)
            {
                texture2D.Apply(true, true);
            }

            LiveryComponent[] liveryComponents = liveryData.liveryComponent;
            Sprite[] array = new Sprite[liveryComponents.Length];
            Vector2 vector = Vector2.zero;
            Vector2 vector2 = Vector2.zero;

            for (int j = 0; j < liveryComponents.Length; j++)
            {
                LiveryComponent liveryComponent = liveryComponents[j];
                liveryComponent.slicePosition = RoundVecToInt(liveryComponent.slicePosition / downscaleAmount);
                liveryComponent.sliceSize = RoundVecToInt(liveryComponent.sliceSize / downscaleAmount);
                liveryComponent.scale *= downscaleAmount;
                liveryComponent.ClampValues(new Vector2(texture2D.width, texture2D.height));

                if (vector == Vector2.zero || vector2 == Vector2.zero || vector != liveryComponent.slicePosition || vector2 != liveryComponent.sliceSize)
                {
                    array[j] = Sprite.Create(texture2D, new Rect(liveryComponent.slicePosition.x, liveryComponent.slicePosition.y, liveryComponent.sliceSize.x, liveryComponent.sliceSize.y), liveryComponent.pivot, liveryData.pixelSize, 0u, SpriteMeshType.FullRect);
                    vector = liveryComponent.slicePosition;
                    vector2 = liveryComponent.sliceSize;
                }
                else
                {
                    array[j] = array[j - 1];
                }
                GameObject gameObject2 = new GameObject(liveryComponent.name);
                gameObject2.transform.SetParent(gameObject.transform);
                gameObject2.layer = LayerMask.NameToLayer("Aircraft");
                SpriteRenderer spriteRenderer = gameObject2.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = array[j];
                spriteRenderer.sortingLayerName = "Aircraft";
                spriteRenderer.sortingOrder = liveryComponent.layerOrder;
                spriteRenderer.material = SingletonNonDestroy<DataPlaceholderMaterials>.Instance.generalDiffuseMaterial;
                spriteRenderer.flipX = (int)liveryComponent.flip.x == 1;
                spriteRenderer.flipY = (int)liveryComponent.flip.y == 1;
                gameObject2.transform.localPosition = liveryComponent.position;
                gameObject2.transform.eulerAngles = new Vector3(0f, 0f, liveryComponent.rotation);
                gameObject2.transform.localScale = liveryComponent.scale;
            }
            if (liveryData.aircraftType.Equals("CONCORDE"))
            {
                gameObject.AddComponent<Animator>().runtimeAnimatorController = SingletonNonDestroy<DataPlaceholderAnimations>.Instance.concordeNoseAnimation;
            }
            else if (liveryData.aircraftType.Equals("TU144"))
            {
                gameObject.AddComponent<Animator>().runtimeAnimatorController = SingletonNonDestroy<DataPlaceholderAnimations>.Instance.tu144NoseAnimation;
            }

            return gameObject;
        }
        catch (Exception ex)
		{
			PerformanceCEO.LogError($"Failed to make livery for {airlineName} with path {directoryPath}. Error: {ExceptionUtils.ProccessException(ex)}");
			liveryData = null;
			return null;
		}
	}

    private static float GetDownscaleFloat()
    {
        float downscaleAmount = 1;
        switch (PerformanceCEOConfig.DownscaleLevel.Value)
        {
            case DownCompressor.DownCompressionEnums.DownscaleLevel.Original:
                downscaleAmount = 1;
                break;
            case DownCompressor.DownCompressionEnums.DownscaleLevel.Downscale2X:
                downscaleAmount = 2;
                break;
            case DownCompressor.DownCompressionEnums.DownscaleLevel.Downscale4X:
                downscaleAmount = 4;
                break;
        };
        return downscaleAmount;
    }

    private static Texture2D DownscaleTextureFastGPU(Texture2D source, int newWidth, int newHeight)
	{
		RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);

		RenderTexture.active = rt;

		Graphics.Blit(source, rt);
		source.Resize(newWidth, newHeight, TextureFormat.ARGB32, false);
		source.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0,0);
		source.Apply();
		RenderTexture.active = null;
		RenderTexture.ReleaseTemporary(rt);
		return source;
	}

	private static Vector2 RoundVecToInt(Vector2 vec)
	{
		return new Vector2(Utils.RoundToIntLikeANormalPerson(vec.x), Utils.RoundToIntLikeANormalPerson(vec.y));
	}
}

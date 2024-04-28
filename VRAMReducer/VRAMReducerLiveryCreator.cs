using PerformanceCEO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PerformanceCEO;
static class VRAMReducerLiveryCreator
{
	// Most of this is from the game, specific portions are marked as not, but not all modifications are noted.
    public static GameObject GetLivery(string filePath, string airlineName)
    {
		RAMReducerManager.LiveryImporterCall = true;

        string[] files = Directory.GetFiles(filePath, "*.json");
		PerformanceCEO.LogInfo(filePath);
		if (files.Length == 0)
		{
			PerformanceCEO.LogError("No JSON Files at path? Error!");
			return null;
		}

		LiveryData liveryData = Utils.CreateFromJSON<LiveryData>(Utils.ReadFile(files[0]));
		GameObject gameObject = new GameObject(liveryData.aircraftType + "_" + airlineName.ToUpper() + "_LIV");
		gameObject.transform.SetParent(SingletonNonDestroy<LiveryController>.Instance.modfolder);
		Livery livery = gameObject.AddComponent<Livery>();
		livery.airlineName = airlineName;
		livery.aircraftType = liveryData.aircraftType;
		livery.isSpecial = liveryData.isSpecial;
		string[] files2 = Directory.GetFiles(filePath, "*.png");

		if (files2.Length == 0)
		{
			PerformanceCEO.LogError("No PNG Files at path? Error!");
			return null;
		}

		byte[] data = File.ReadAllBytes(files2[0]);
		Texture2D texture2D = new Texture2D(2, 2);
		texture2D.LoadImage(data);
		if (GameSettingManager.CompressImages)
		{
			texture2D.Compress(true);
		}
		LiveryComponent[] liveryComponent = liveryData.liveryComponent;
		Sprite[] array = new Sprite[liveryComponent.Length];
		Vector2 lhs = Vector2.zero;
		Vector2 lhs2 = Vector2.zero;
		for (int j = 0; j < liveryComponent.Length; j++)
		{
			LiveryComponent liveryComponent2 = liveryComponent[j];
			liveryComponent2.ClampValues(new Vector2((float)texture2D.width, (float)texture2D.height));
			if (lhs == Vector2.zero || lhs2 == Vector2.zero || lhs != liveryComponent2.slicePosition || lhs2 != liveryComponent2.sliceSize)
			{
				array[j] = Sprite.Create(texture2D, new Rect(liveryComponent2.slicePosition.x, liveryComponent2.slicePosition.y, liveryComponent2.sliceSize.x, liveryComponent2.sliceSize.y), liveryComponent2.pivot, liveryData.pixelSize, 0U, SpriteMeshType.FullRect);
				lhs = liveryComponent2.slicePosition;
				lhs2 = liveryComponent2.sliceSize;
			}
			else
			{
				array[j] = array[j - 1];
			}
			GameObject gameObject2 = new GameObject(liveryComponent2.name);
			gameObject2.transform.SetParent(gameObject.transform);
			gameObject2.layer = LayerMask.NameToLayer("Aircraft");
			SpriteRenderer spriteRenderer = gameObject2.AddComponent<SpriteRenderer>();
			spriteRenderer.sprite = array[j];
			spriteRenderer.sortingLayerName = "Aircraft";
			spriteRenderer.sortingOrder = liveryComponent2.layerOrder;
			spriteRenderer.material = SingletonNonDestroy<DataPlaceholderMaterials>.Instance.generalDiffuseMaterial;
			spriteRenderer.flipX = ((int)liveryComponent2.flip.x == 1);
			spriteRenderer.flipY = ((int)liveryComponent2.flip.y == 1);
			gameObject2.transform.localPosition = liveryComponent2.position;
			gameObject2.transform.eulerAngles = new Vector3(0f, 0f, liveryComponent2.rotation);
			gameObject2.transform.localScale = liveryComponent2.scale;
		}
		if (liveryData.aircraftType.Equals("CONCORDE"))
		{
			gameObject.AddComponent<Animator>().runtimeAnimatorController = SingletonNonDestroy<DataPlaceholderAnimations>.Instance.concordeNoseAnimation;
		}
		else if (liveryData.aircraftType.Equals("TU144"))
		{
			gameObject.AddComponent<Animator>().runtimeAnimatorController = SingletonNonDestroy<DataPlaceholderAnimations>.Instance.tu144NoseAnimation;
		}

		RAMReducerManager.LiveryImporterCall = false;
		return  gameObject;
    }
}


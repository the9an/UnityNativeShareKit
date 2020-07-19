using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;

public class SampleScene : MonoBehaviour
{
	[SerializeField]
	private Image _sp = null;

	public void OnClickShare()
	{
		var tempPath = Path.Combine(Application.persistentDataPath, "temp");
		if (!Directory.Exists(tempPath))
		{
			Directory.CreateDirectory(tempPath);
		}

		var tex = Resources.Load<Texture2D>("test");
		_sp.sprite = GetSprite(tex);
		_sp.SetNativeSize();

		var tempFilePath = Path.Combine(tempPath, "temp.png");
		if (File.Exists(tempFilePath))
		{
			ShareKit.Open("", tempFilePath);
		}
		else
		{
			var bin = tex.EncodeToPNG();
			if (bin != null)
			{
				try
				{
					File.WriteAllBytes(tempFilePath, bin);
					ShareKit.Open("", tempFilePath);
				}
				catch (Exception e)
				{
					Debug.LogError("Failed To Save Image: " + e);
				}
			}
		}
	}

	private static Sprite GetSprite(Texture2D t)
	{
		return Sprite.Create(t, new Rect(0, 0, t.width, t.height), Vector2.zero);
	}
}

using UnityEngine;

#if UNITY_IPHONE
using System.Runtime.InteropServices;
#elif UNITY_ANDROID
using UnityEngine.Android;
#endif

public static class ShareKit
{
#if UNITY_IPHONE
	[DllImport ("__Internal")]
	private static extern void ShareKit_Open(string text, string url, string textureUri);
#elif UNITY_ANDROID
	private static readonly AndroidJavaObject unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
	private static readonly AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
	private static readonly AndroidJavaObject shareUtility = new AndroidJavaObject("com.kuan.sharekit16.ShareUtility");
#endif

#if UNITY_IPHONE

	private static void _Open(string text, string url, string textureUri)
	{
		ShareKit_Open(text, url, textureUri);
	}

#elif UNITY_ANDROID

	private static void _Open(string text, string url, string textureUri)
	{
		if (!string.IsNullOrEmpty(url))
		{
			text += " " + url;
		}
		object[] parameters = new object[6];
		parameters[0] = activity;
		parameters[1] = Application.identifier + ".fileprovider";
		parameters[2] = ""; // ShareKit's title
		parameters[3] = // Gallery title
			Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite)
				? "Album"
				: string.Empty; // If it is empty string then Gallery will not be show
		parameters[4] = text;
		parameters[5] = textureUri;
		var intent = shareUtility.Call<AndroidJavaObject>("makeChooserIntent", parameters);
		activity.Call("startActivity", intent);
	}

#endif

	/// <summary>
	/// Open Sharekit
	/// </summary>
	/// <param name="text">Text to Share</param>
	/// <param name="url">Url to Share</param>
	/// <param name="textureUri">Texture local path</param>
	public static void Open(string text = "", string url = "", string textureUri = "")
	{
#if UNITY_EDITOR
		Debug.LogWarning("Can not open share kit in Editor");
#elif UNITY_ANDROID || UNITY_IPHONE
		_Open(text, url, textureUri);
#endif
	}
}

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;

public static class  PostprocessBuild
{
	private static readonly Dictionary<string, string> InfoPlist = new Dictionary<string, string>
	{
		{"NSPhotoLibraryUsageDescription", "Use Photo"},
		{"NSPhotoLibraryAddUsageDescription", "Save Photo"}
	};

	[PostProcessBuild]
	public static void OnPostProcessBuild(BuildTarget buildTarget, string pathToBuiltProject)
	{
		if (buildTarget == BuildTarget.iOS)
		{
			AddElementInfoPlist(pathToBuiltProject);
		}
	}

	private static void AddElementInfoPlist(string pathToBuiltProject)
	{
#if UNITY_IOS
		// Get plist
		string plistPath = pathToBuiltProject + "/Info.plist";
		var plist = new UnityEditor.iOS.Xcode.PlistDocument();
		plist.ReadFromString(File.ReadAllText(plistPath));
		bool changed = false;

		// Get root
		UnityEditor.iOS.Xcode.PlistElementDict rootDict = plist.root;

		foreach (var p in InfoPlist)
		{
			if (!rootDict.values.ContainsKey(p.Key))
			{
				rootDict.CreateDict(p.Key);
			}
			rootDict.SetString(p.Key, p.Value);
			changed = true;
		}

		if (changed)
		{
			// Write to file
			File.WriteAllText(plistPath, plist.WriteToString());
		}
#endif
	}
}
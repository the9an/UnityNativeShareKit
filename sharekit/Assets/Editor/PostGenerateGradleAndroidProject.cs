using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEditor.Android;
using UnityEngine;

/// <summary>
/// AndroidManifest
/// </summary>
public class PostGenerateGradleAndroidProject : IPostGenerateGradleAndroidProject
{
	public int callbackOrder => 1;

	public void OnPostGenerateGradleAndroidProject(string path)
	{
		GenerateGradleProperties(path);

		var androidManifest = new AndroidManifest(GetManifestPath(path));

		if (EditManifest(androidManifest))
		{
			androidManifest.Save();
			Debug.Log("adjusted AndroidManifest.xml.");
		}
	}

	private static string GetManifestPath(string basePath)
	{
		return Path.Combine(basePath, "src", "main", "AndroidManifest.xml");
	}

	private static bool EditManifest(AndroidManifest androidManifest)
	{
		var changed = false;

		changed |= androidManifest.AddActivity("com.kuan.sharekit16.GalleryActivity",
			attributes: new Dictionary<string, string>());
		changed |= androidManifest.AddUsesPermission("android.permission.WRITE_EXTERNAL_STORAGE");

		var providerName = "androidx.core.content.FileProvider";
		changed |= androidManifest.AddProvider(providerName, attributes: new Dictionary<string, string>
		{
			{"authorities", Application.identifier + ".fileprovider"},
			{"exported", "false"},
			{"grantUriPermissions", "true"},
		});
		changed |= androidManifest.AddMetaData("provider", providerName, "android.support.FILE_PROVIDER_PATHS",
			attributes: new Dictionary<string, string>
			{
				{"resource", "@xml/filepaths"},
			});

		return changed;
	}

	private static void GenerateGradleProperties(string path)
	{
		var gradlePropertiesFile = Path.Combine(path, "gradle.properties");
		if (File.Exists(gradlePropertiesFile))
		{
			Debug.Log($"REMOVE FILE: {gradlePropertiesFile}");
			File.Delete(gradlePropertiesFile);
		}

		var writer = File.CreateText(gradlePropertiesFile);
		writer.WriteLine("org.gradle.jvmargs=-Xmx4096M");
		writer.WriteLine("android.useAndroidX=true");
		writer.WriteLine("android.enableJetifier=true");
		writer.Flush();
		writer.Close();
	}
}

internal class AndroidXmlDocument : XmlDocument
{
	private readonly string _path;
	protected readonly XmlNamespaceManager _nsMgr;
	protected readonly string AndroidXmlNamespace = "http://schemas.android.com/apk/res/android";


	protected AndroidXmlDocument(string path)
	{
		_path = path;
		using (var reader = new XmlTextReader(_path))
		{
			reader.Read();
			Load(reader);
		}
		_nsMgr = new XmlNamespaceManager(NameTable);
		_nsMgr.AddNamespace("android", AndroidXmlNamespace);
	}

	public void Save()
	{
		SaveAs(_path);
	}

	private void SaveAs(string path)
	{
		using (var writer = new XmlTextWriter(path, new UTF8Encoding(false)))
		{
			writer.Formatting = Formatting.Indented;
			Save(writer);
		}
	}
}

internal class AndroidManifest : AndroidXmlDocument
{
	private readonly XmlElement _applicationElement;
	private readonly XmlElement _manifestElement;

	public AndroidManifest(string path) : base(path)
	{
		_applicationElement = SelectSingleNode("/manifest/application") as XmlElement;
		_manifestElement = SelectSingleNode("/manifest") as XmlElement;
	}

	private XmlAttribute CreateAndroidAttribute(string key, string value)
	{
		XmlAttribute attr = CreateAttribute("android", key, AndroidXmlNamespace);
		attr.Value = value;
		return attr;
	}

	internal bool AddActivity(string name, Dictionary<string, string> attributes)
	{
		if (SelectNodes($"/manifest/application/activity[@android:name='{name}']", _nsMgr)?.Count == 0)
		{
			var elem = CreateElement("activity");
			elem.Attributes.Append(CreateAndroidAttribute("name", name));
			foreach (var attribute in attributes)
			{
				if (elem.Attributes[attribute.Key, AndroidXmlNamespace] != null)
				{
					continue;
				}
				elem.Attributes.Append(CreateAndroidAttribute(attribute.Key, attribute.Value));
			}

			_applicationElement.AppendChild(elem);
			return true;
		}

		return false;
	}

	internal bool AddUsesPermission(string name)
	{
		if (SelectNodes($"/manifest/uses-permission[@android:name='{name}']", _nsMgr)?.Count == 0)
		{
			var elem = CreateElement("uses-permission");
			elem.Attributes.Append(CreateAndroidAttribute("name", name));
			_manifestElement.AppendChild(elem);
			return true;
		}

		return false;
	}

	internal bool AddProvider(string name, Dictionary<string, string> attributes)
	{
		var providerElem =
			SelectSingleNode($"/manifest/application/provider[@android:name='{name}']", _nsMgr) as XmlElement;
		if (providerElem != null)
		{
			return false;
		}

		var elem = CreateElement("provider");
		elem.Attributes.Append(CreateAndroidAttribute("name", name));
		foreach (var attribute in attributes)
		{
			if (elem.Attributes[attribute.Key, AndroidXmlNamespace] != null)
			{
				continue;
			}
			elem.Attributes.Append(CreateAndroidAttribute(attribute.Key, attribute.Value));
		}

		_applicationElement.AppendChild(elem);
		return true;
	}

	internal bool AddMetaData(string parentTag, string parentName, string name, Dictionary<string, string> attributes)
	{
		var parentElem =
			SelectSingleNode($"/manifest/application/{parentTag}[@android:name='{parentName}']", _nsMgr) as XmlElement;
		if (parentElem == null)
		{
			return false;
		}

		XmlElement metaDataElem = null;
		foreach (var childNode in parentElem.ChildNodes)
		{
			var element = childNode as XmlElement;
			if (element != null &&
			    element.LocalName.Equals("meta-data") &&
			    element.Attributes["name", AndroidXmlNamespace]?.Value == name)
			{
				metaDataElem = childNode as XmlElement;
				break;
			}
		}

		if (metaDataElem != null)
		{
			return false;
		}

		metaDataElem = CreateElement("meta-data");
		metaDataElem.Attributes.Append(CreateAndroidAttribute("name", name));
		foreach (var attribute in attributes)
		{
			if (metaDataElem.Attributes[attribute.Key, AndroidXmlNamespace] != null)
			{
				continue;
			}
			metaDataElem.Attributes.Append(CreateAndroidAttribute(attribute.Key, attribute.Value));
		}
		parentElem.AppendChild(metaDataElem);
		return true;
	}
}

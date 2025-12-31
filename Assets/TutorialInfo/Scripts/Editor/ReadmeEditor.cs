using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor( typeof( Readme ) )]
[InitializeOnLoad]
public class ReadmeEditor : Editor
{
	static string S_SHOWED_README_SESSION_STATE_NAME = "ReadmeEditor.showedReadme";

	static string S_README_SOURCE_DIRECTORY = "Assets/TutorialInfo";

	const float C_SPACE = 16f;

	static ReadmeEditor()
	{
		EditorApplication.delayCall += SelectReadmeAutomatically;
	}

	static void RemoveTutorial()
	{
		if (EditorUtility.DisplayDialog( "Remove Readme Assets",

			$"All contents under {S_README_SOURCE_DIRECTORY} will be removed, are you sure you want to proceed?",
			"Proceed",
			"Cancel" ))
		{
			if (Directory.Exists( S_README_SOURCE_DIRECTORY ))
			{
				FileUtil.DeleteFileOrDirectory( S_README_SOURCE_DIRECTORY );
				FileUtil.DeleteFileOrDirectory( S_README_SOURCE_DIRECTORY + ".meta" );
			}
			else
			{
				Debug.Log( $"Could not find the Readme folder at {S_README_SOURCE_DIRECTORY}" );
			}

			var readmeAsset = SelectReadme();
			if (readmeAsset != null)
			{
				var path = AssetDatabase.GetAssetPath( readmeAsset );
				FileUtil.DeleteFileOrDirectory( path + ".meta" );
				FileUtil.DeleteFileOrDirectory( path );
			}

			AssetDatabase.Refresh();
		}
	}

	static void SelectReadmeAutomatically()
	{
		if (!SessionState.GetBool( S_SHOWED_README_SESSION_STATE_NAME, false ))
		{
			var readme = SelectReadme();
			SessionState.SetBool( S_SHOWED_README_SESSION_STATE_NAME, true );

			if (readme && !readme.loadedLayout)
			{
				LoadLayout();
				readme.loadedLayout = true;
			}
		}
	}

	static void LoadLayout()
	{
		var assembly = typeof( EditorApplication ).Assembly;
		var windowLayoutType = assembly.GetType( "UnityEditor.WindowLayout", true );
		var method = windowLayoutType.GetMethod( "LoadWindowLayout", BindingFlags.Public | BindingFlags.Static );
		method.Invoke( null, new object [] { Path.Combine( Application.dataPath, "TutorialInfo/Layout.wlt" ), false } );
	}

	static Readme SelectReadme()
	{
		var ids = AssetDatabase.FindAssets( "Readme t:Readme" );
		if (ids.Length == 1)
		{
			var readmeObject = AssetDatabase.LoadMainAssetAtPath( AssetDatabase.GUIDToAssetPath( ids [ 0 ] ) );

			Selection.objects = new UnityEngine.Object [] { readmeObject };

			return (Readme) readmeObject;
		}
		else
		{
			Debug.Log( "Couldn't find a readme" );
			return null;
		}
	}

	protected override void OnHeaderGUI()
	{
		var readme = (Readme) target;
		Init();

		var iconWidth = Mathf.Min( EditorGUIUtility.currentViewWidth / 3f - 20f, 128f );

		GUILayout.BeginHorizontal( "In BigTitle" );
		{
			if (readme.icon != null)
			{
				GUILayout.Space( C_SPACE );
				GUILayout.Label( readme.icon, GUILayout.Width( iconWidth ), GUILayout.Height( iconWidth ) );
			}
			GUILayout.Space( C_SPACE );
			GUILayout.BeginVertical();
			{

				GUILayout.FlexibleSpace();
				GUILayout.Label( readme.title, TitleStyle );
				GUILayout.FlexibleSpace();
			}
			GUILayout.EndVertical();
			GUILayout.FlexibleSpace();
		}
		GUILayout.EndHorizontal();
	}

	public override void OnInspectorGUI()
	{
		var readme = (Readme) target;
		Init();

		foreach (var section in readme.sections)
		{
			if (!string.IsNullOrEmpty( section.heading ))
			{
				GUILayout.Label( section.heading, HeadingStyle );
			}

			if (!string.IsNullOrEmpty( section.text ))
			{
				GUILayout.Label( section.text, BodyStyle );
			}

			if (!string.IsNullOrEmpty( section.linkText ))
			{
				if (LinkLabel( new GUIContent( section.linkText ) ))
				{
					Application.OpenURL( section.url );
				}
			}

			GUILayout.Space( C_SPACE );
		}

		if (GUILayout.Button( "Remove Readme Assets", ButtonStyle ))
		{
			RemoveTutorial();
		}
	}

	bool _initialized;

	GUIStyle LinkStyle => _linkStyle;

	[SerializeField]
	GUIStyle _linkStyle;

	GUIStyle TitleStyle => _titleStyle;

	[SerializeField]
	GUIStyle _titleStyle;

	GUIStyle HeadingStyle => _headingStyle;

	[SerializeField]
	GUIStyle _headingStyle;

	GUIStyle BodyStyle => _bodyStyle;

	[SerializeField]
	GUIStyle _bodyStyle;

	GUIStyle ButtonStyle => _buttonStyle;

	[SerializeField]
	GUIStyle _buttonStyle;

	void Init()
	{
		if (_initialized)
		{
			return;
		}

		_bodyStyle = new GUIStyle( EditorStyles.label );
		_bodyStyle.wordWrap = true;
		_bodyStyle.fontSize = 14;
		_bodyStyle.richText = true;

		_titleStyle = new GUIStyle( _bodyStyle );
		_titleStyle.fontSize = 26;

		_headingStyle = new GUIStyle( _bodyStyle );
		_headingStyle.fontStyle = FontStyle.Bold;
		_headingStyle.fontSize = 18;

		_linkStyle = new GUIStyle( _bodyStyle );
		_linkStyle.wordWrap = false;

		// Match selection color which works nicely for both light and dark skins
		_linkStyle.normal.textColor = new Color( 0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f );
		_linkStyle.stretchWidth = false;

		_buttonStyle = new GUIStyle( EditorStyles.miniButton );
		_buttonStyle.fontStyle = FontStyle.Bold;

		_initialized = true;
	}

	bool LinkLabel(GUIContent label, params GUILayoutOption [] options)
	{
		var position = GUILayoutUtility.GetRect( label, LinkStyle, options );

		Handles.BeginGUI();
		Handles.color = LinkStyle.normal.textColor;
		Handles.DrawLine( new Vector3( position.xMin, position.yMax ), new Vector3( position.xMax, position.yMax ) );
		Handles.color = Color.white;
		Handles.EndGUI();

		EditorGUIUtility.AddCursorRect( position, MouseCursor.Link );

		return GUI.Button( position, label, LinkStyle );
	}
}

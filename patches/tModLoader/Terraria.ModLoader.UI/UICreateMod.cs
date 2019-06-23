﻿using System;
using System.CodeDom.Compiler;
using System.IO;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	public class UICreateMod : UIState
	{
		UITextPanel<string> messagePanel;
		UIFocusInputTextField modName;
		UIFocusInputTextField modDiplayName;
		UIFocusInputTextField modAuthor;

		public override void OnInitialize() {
			var uIElement = new UIElement {
				Width = { Percent = 0.8f },
				MaxWidth = UICommon.MaxPanelWidth,
				Top = { Pixels = 220 },
				Height = { Pixels = -220, Percent = 1f },
				HAlign = 0.5f
			};
			Append(uIElement);

			var mainPanel = new UIPanel {
				Width = { Percent = 1f },
				Height = { Pixels = -110, Percent = 1f },
				BackgroundColor = UICommon.mainPanelBackground,
				PaddingTop = 0f
			};
			uIElement.Append(mainPanel);

			var uITextPanel = new UITextPanel<string>(Language.GetTextValue("tModLoader.MSCreateMod"), 0.8f, true) {
				HAlign = 0.5f,
				Top = { Pixels = -35 },
				BackgroundColor = UICommon.defaultUIBlue
			}.WithPadding(15);
			uIElement.Append(uITextPanel);

			messagePanel = new UITextPanel<string>(Language.GetTextValue("")) {
				Width = { Percent = 1f },
				Height = { Pixels = 25 },
				VAlign = 1f,
				Top = { Pixels = -20 }
			};
			uIElement.Append(messagePanel);

			var buttonBack = new UITextPanel<string>(Language.GetTextValue("UI.Back")) {
				Width = { Pixels = -10, Percent = 0.5f },
				Height = { Pixels = 25 },
				VAlign = 1f,
				Top = { Pixels = -65 }
			}.WithFadedMouseOver();
			buttonBack.OnClick += BackClick;
			uIElement.Append(buttonBack);

			var buttonCreate = new UITextPanel<string>(Language.GetTextValue("LegacyMenu.28")); // Create
			buttonCreate.CopyStyle(buttonBack);
			buttonCreate.HAlign = 1f;
			buttonCreate.WithFadedMouseOver();
			buttonCreate.OnClick += OKClick;
			uIElement.Append(buttonCreate);

			float top = 16;
			modName = createAndAppendTextInputWithLabel("ModName (no spaces)", "Type here");
			modName.OnTextChange += (a, b) => { modName.SetText(modName.currentString.Replace(" ", "")); };
			modDiplayName = createAndAppendTextInputWithLabel("Mod DisplayName", "Type here");
			modAuthor = createAndAppendTextInputWithLabel("Mod Author", "Type here");
			// TODO: OnTab
			// TODO: Starting Item checkbox

			UIFocusInputTextField createAndAppendTextInputWithLabel(string label, string hint) {
				var panel = new UIPanel();
				panel.SetPadding(0);
				panel.Width.Set(0, 1f);
				panel.Height.Set(40, 0f);
				panel.Top.Set(top, 0f);
				top += 46;

				var modNameText = new UIText(label) {
					Left = { Pixels = 10 },
					Top = { Pixels = 10 }
				};

				panel.Append(modNameText);

				var textBoxBackground = new UIPanel();
				textBoxBackground.SetPadding(0);
				textBoxBackground.Top.Set(6f, 0f);
				textBoxBackground.Left.Set(0, .5f);
				textBoxBackground.Width.Set(0, .5f);
				textBoxBackground.Height.Set(30, 0f);
				panel.Append(textBoxBackground);

				var uIInputTextField = new UIFocusInputTextField(hint);
				uIInputTextField.Top.Set(5, 0f);
				uIInputTextField.Left.Set(10, 0f);
				uIInputTextField.Width.Set(-20, 1f);
				uIInputTextField.Height.Set(20, 0);
				textBoxBackground.Append(uIInputTextField);

				mainPanel.Append(panel);

				return uIInputTextField;
			}
		}

		public override void OnActivate() {
			base.OnActivate();
			modName.SetText("");
			modDiplayName.SetText("");
			modAuthor.SetText("");
			messagePanel.SetText("");
		}

		private void BackClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuClose);
			Main.menuMode = Interface.modSourcesID;
		}

		private void OKClick(UIMouseEvent evt, UIElement listeningElement) {
			string modNameTrimmed = modName.currentString.Trim();
			string sourceFolder = Path.Combine(ModCompile.ModSourcePath, modNameTrimmed);
			var provider = CodeDomProvider.CreateProvider("C#");
			if (Directory.Exists(sourceFolder))
				messagePanel.SetText("Folder already exists");
			else if (!provider.IsValidIdentifier(modNameTrimmed))
				messagePanel.SetText("ModName is invalid C# identifier. Remove spaces.");
			else if (string.IsNullOrWhiteSpace(modDiplayName.currentString))
				messagePanel.SetText("DisplayName can't be empty");
			else if (string.IsNullOrWhiteSpace(modAuthor.currentString))
				messagePanel.SetText("Author can't be empty");
			else if (string.IsNullOrWhiteSpace(modAuthor.currentString))
				messagePanel.SetText("Author can't be empty");
			else {
				Main.PlaySound(SoundID.MenuOpen);
				Main.menuMode = Interface.modSourcesID;
				Directory.CreateDirectory(sourceFolder);

				// TODO: Simple ModItem and PNG, verbatim line endings, localization.
				File.WriteAllText(Path.Combine(sourceFolder, "build.txt"), GetModBuild());
				File.WriteAllText(Path.Combine(sourceFolder, "description.txt"), GetModDescription());
				File.WriteAllText(Path.Combine(sourceFolder, $"{modNameTrimmed}.cs"), GetModClass(modNameTrimmed));
				File.WriteAllText(Path.Combine(sourceFolder, $"{modNameTrimmed}.csproj"), GetModCsproj(modNameTrimmed));
				string propertiesFolder = sourceFolder + Path.DirectorySeparatorChar + "Properties";
				Directory.CreateDirectory(propertiesFolder);
				File.WriteAllText(Path.Combine(propertiesFolder, $"launchSettings.json"), GetLaunchSettings());
			}
		}

		private string GetModBuild() {
			return $"displayName = {modDiplayName.currentString}" +
				$"{Environment.NewLine}author = {modAuthor.currentString}" +
				$"{Environment.NewLine}version = 0.1";
		}

		private string GetModDescription() {
			return $"{modDiplayName.currentString} is a pretty cool mod, it does...this. Modify this file with a description of your mod.";
		}

		private string GetModClass(string modNameTrimmed) {
			return 
$@"using Terraria.ModLoader;

namespace {modNameTrimmed}
{{
	public class {modNameTrimmed} : Mod
	{{
		public {modNameTrimmed}()
		{{
		}}
	}}
}}";
		}

		private string GetModCsproj(string modNameTrimmed) {
			return
$@"<?xml version=""1.0"" encoding=""utf-8""?>
<Project Sdk=""Microsoft.NET.Sdk"">
  <Import Project=""..\..\references\tModLoader.targets"" />
  <PropertyGroup>
    <AssemblyName>{modNameTrimmed}</AssemblyName>
    <TargetFramework>net45</TargetFramework>
    <PlatformTarget>x86</PlatformTarget>
    <LangVersion>latest</LangVersion>
  </PropertyGroup>
  <Target Name=""BuildMod"" AfterTargets=""Build"">
    <Exec Command=""&quot;$(tMLBuildServerPath)&quot; -build $(ProjectDir) -eac $(TargetPath) -define $(DefineConstants) -unsafe $(AllowUnsafeBlocks)"" />
  </Target>
</Project>";
		}

		private string GetLaunchSettings() {
			return
$@"{{
  ""profiles"": {{
    ""Terraria"": {{
      ""commandName"": ""Executable"",
      ""executablePath"": ""$(tMLPath)"",
      ""workingDirectory"": ""$(TerrariaSteamPath)""
    }},
    ""TerrariaServer"": {{
      ""commandName"": ""Executable"",
      ""executablePath"": ""$(tMLServerPath)"",
      ""workingDirectory"": ""$(TerrariaSteamPath)""
    }}
  }}
}}";
		}
	}
}

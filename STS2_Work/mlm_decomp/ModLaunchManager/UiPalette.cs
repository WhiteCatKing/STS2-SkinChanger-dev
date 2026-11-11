using Godot;

namespace ModLaunchManager;

internal sealed class UiPalette
{
	internal required bool IsDark { get; init; }

	internal required bool IsFluent { get; init; }

	internal bool IsLiquidGlass { get; init; }

	internal required Color Canvas { get; init; }

	internal required Color Surface { get; init; }

	internal required Color SurfaceRaised { get; init; }

	internal required Color SurfaceInset { get; init; }

	internal required Color SurfaceHover { get; init; }

	internal required Color Border { get; init; }

	internal required Color BorderStrong { get; init; }

	internal required Color Text { get; init; }

	internal required Color TextMuted { get; init; }

	internal required Color TextDim { get; init; }

	internal required Color Accent { get; init; }

	internal required Color AccentStrong { get; init; }

	internal required Color AccentSoft { get; init; }

	internal required Color OnAccent { get; init; }

	internal required Color Success { get; init; }

	internal required Color Warning { get; init; }

	internal required Color Danger { get; init; }

	internal required Color PrimaryHover { get; init; }

	internal required Color PrimaryPressed { get; init; }

	internal required Color PrimaryHoverText { get; init; }

	internal required Color SecondaryFill { get; init; }

	internal required Color SecondaryPressed { get; init; }

	internal required Color ToolbarPressed { get; init; }

	internal required Color ChipHover { get; init; }

	internal required Color ChipPressed { get; init; }

	internal required Color ChipPressedHover { get; init; }

	internal required Color ChipPressedBorder { get; init; }

	internal required Color ChipPressedText { get; init; }

	internal required Color DisabledFill { get; init; }

	internal required Color DisabledBorder { get; init; }

	internal required Color FocusRing { get; init; }

	internal required Color DangerFill { get; init; }

	internal required Color DangerHover { get; init; }

	internal required Color DangerPressed { get; init; }

	internal required Color DangerText { get; init; }

	internal required Color DangerHoverText { get; init; }

	internal required Color HoverBorder { get; init; }

	internal required Color FieldFocusBorder { get; init; }

	internal required Color TreeFocusBorder { get; init; }

	internal required Color TreeHoverSelected { get; init; }

	internal required Color TreeCursor { get; init; }

	internal required Color ProgressFill { get; init; }

	internal required Color ProgressOutline { get; init; }

	internal required Color TooltipFill { get; init; }

	internal required Color TextShadow { get; init; }

	internal required Color SurfaceShadow { get; init; }

	internal required Color ScrollTrack { get; init; }

	internal required Color ScrollGrabber { get; init; }

	internal required Color ScrollGrabberHover { get; init; }

	internal required Color ScrollGrabberPressed { get; init; }

	internal static UiPalette Fluent { get; } = new UiPalette
	{
		IsDark = false,
		IsFluent = true,
		Canvas = UiKit.Html("#F3F3F3"),
		Surface = UiKit.Html("#FAFAFA"),
		SurfaceRaised = UiKit.Html("#FFFFFF"),
		SurfaceInset = UiKit.Html("#FFFFFF"),
		SurfaceHover = UiKit.Html("#F5F5F5"),
		Border = UiKit.Html("#E0E0E0"),
		BorderStrong = UiKit.Html("#D1D1D1"),
		Text = UiKit.Html("#242424"),
		TextMuted = UiKit.Html("#424242"),
		TextDim = UiKit.Html("#616161"),
		Accent = UiKit.Html("#0F6CBD"),
		AccentStrong = UiKit.Html("#0F6CBD"),
		AccentSoft = UiKit.Html("#EAEAEA"),
		OnAccent = UiKit.Html("#FFFFFF"),
		Success = UiKit.Html("#0E700E"),
		Warning = UiKit.Html("#BC4B09"),
		Danger = UiKit.Html("#B10E1C"),
		PrimaryHover = UiKit.Html("#115EA3"),
		PrimaryPressed = UiKit.Html("#0F548C"),
		PrimaryHoverText = UiKit.Html("#FFFFFF"),
		SecondaryFill = UiKit.Html("#FFFFFF"),
		SecondaryPressed = UiKit.Html("#E0E0E0"),
		ToolbarPressed = UiKit.Html("#E0E0E0"),
		ChipHover = UiKit.Html("#F5F5F5"),
		ChipPressed = UiKit.Html("#0F6CBD"),
		ChipPressedHover = UiKit.Html("#115EA3"),
		ChipPressedBorder = UiKit.Html("#0F6CBD"),
		ChipPressedText = UiKit.Html("#FFFFFF"),
		DisabledFill = UiKit.Html("#F0F0F0"),
		DisabledBorder = UiKit.Html("#E0E0E0"),
		FocusRing = UiKit.Html("#1A1A1A"),
		DangerFill = UiKit.Html("#FDF3F4"),
		DangerHover = UiKit.Html("#FDE7E9"),
		DangerPressed = UiKit.Html("#F1BBBC"),
		DangerText = UiKit.Html("#B10E1C"),
		DangerHoverText = UiKit.Html("#960B18"),
		HoverBorder = UiKit.Html("#C7C7C7"),
		FieldFocusBorder = UiKit.Html("#0F6CBD"),
		TreeFocusBorder = UiKit.Html("#0F6CBD"),
		TreeHoverSelected = UiKit.Html("#E0E0E0"),
		TreeCursor = UiKit.Html("#0F6CBD"),
		ProgressFill = UiKit.Html("#0F6CBD"),
		ProgressOutline = new Color(1f, 1f, 1f, 0.75f),
		TooltipFill = UiKit.Html("#FFFFFF"),
		TextShadow = UiKit.Clear,
		SurfaceShadow = new Color(0f, 0f, 0f, 0.14f),
		ScrollTrack = UiKit.Clear,
		ScrollGrabber = UiKit.Html("#A6A6A6"),
		ScrollGrabberHover = UiKit.Html("#8A8A8A"),
		ScrollGrabberPressed = UiKit.Html("#5D5D5D")
	};

	internal static UiPalette FluentDark { get; } = new UiPalette
	{
		IsDark = true,
		IsFluent = true,
		Canvas = UiKit.Html("#202020"),
		Surface = UiKit.Html("#1F1F1F"),
		SurfaceRaised = UiKit.Html("#292929"),
		SurfaceInset = UiKit.Html("#1F1F1F"),
		SurfaceHover = UiKit.Html("#3D3D3D"),
		Border = UiKit.Html("#3D3D3D"),
		BorderStrong = UiKit.Html("#666666"),
		Text = UiKit.Html("#FFFFFF"),
		TextMuted = UiKit.Html("#D6D6D6"),
		TextDim = UiKit.Html("#ADADAD"),
		Accent = UiKit.Html("#62ABF5"),
		AccentStrong = UiKit.Html("#115EA3"),
		AccentSoft = UiKit.Html("#3D3D3D"),
		OnAccent = UiKit.Html("#FFFFFF"),
		Success = UiKit.Html("#54B054"),
		Warning = UiKit.Html("#F98845"),
		Danger = UiKit.Html("#F37885"),
		PrimaryHover = UiKit.Html("#0F6CBD"),
		PrimaryPressed = UiKit.Html("#0C3B5E"),
		PrimaryHoverText = UiKit.Html("#FFFFFF"),
		SecondaryFill = UiKit.Html("#292929"),
		SecondaryPressed = UiKit.Html("#1F1F1F"),
		ToolbarPressed = UiKit.Html("#1F1F1F"),
		ChipHover = UiKit.Html("#3D3D3D"),
		ChipPressed = UiKit.Html("#115EA3"),
		ChipPressedHover = UiKit.Html("#0F6CBD"),
		ChipPressedBorder = UiKit.Html("#115EA3"),
		ChipPressedText = UiKit.Html("#FFFFFF"),
		DisabledFill = UiKit.Html("#242424"),
		DisabledBorder = UiKit.Html("#3D3D3D"),
		FocusRing = UiKit.Html("#FFFFFF"),
		DangerFill = UiKit.Html("#3B1219"),
		DangerHover = UiKit.Html("#541723"),
		DangerPressed = UiKit.Html("#2D0E13"),
		DangerText = UiKit.Html("#F37885"),
		DangerHoverText = UiKit.Html("#FFFFFF"),
		HoverBorder = UiKit.Html("#858585"),
		FieldFocusBorder = UiKit.Html("#62ABF5"),
		TreeFocusBorder = UiKit.Html("#62ABF5"),
		TreeHoverSelected = UiKit.Html("#454545"),
		TreeCursor = UiKit.Html("#62ABF5"),
		ProgressFill = UiKit.Html("#62ABF5"),
		ProgressOutline = new Color(0f, 0f, 0f, 0.75f),
		TooltipFill = UiKit.Html("#2B2B2B"),
		TextShadow = new Color(0f, 0f, 0f, 0.2f),
		SurfaceShadow = new Color(0f, 0f, 0f, 0.36f),
		ScrollTrack = UiKit.Clear,
		ScrollGrabber = UiKit.Html("#757575"),
		ScrollGrabberHover = UiKit.Html("#858585"),
		ScrollGrabberPressed = UiKit.Html("#ADADAD")
	};

	internal static UiPalette LiquidGlass { get; } = Glass(dark: false);

	internal static UiPalette LiquidGlassDark { get; } = Glass(dark: true);

	internal static UiPalette For(LauncherTheme theme)
	{
		return theme switch
		{
			LauncherTheme.Fluent => Fluent, 
			LauncherTheme.LiquidGlass => LiquidGlass, 
			LauncherTheme.LiquidGlassDark => LiquidGlassDark, 
			_ => FluentDark, 
		};
	}

	private static UiPalette Glass(bool dark)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_034e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0354: Unknown result type (might be due to invalid IL or missing references)
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0376: Unknown result type (might be due to invalid IL or missing references)
		//IL_0389: Unknown result type (might be due to invalid IL or missing references)
		//IL_0394: Unknown result type (might be due to invalid IL or missing references)
		//IL_039f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_040e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0425: Unknown result type (might be due to invalid IL or missing references)
		Color val = C("#0057B8", "#75B6FF");
		Color val2 = C("#1D1D1F", "#F5F5F7");
		Color textMuted = C("#424249", "#D0D0D7");
		Color textDim = C("#56565E", "#B6B6C0");
		Color border = C("#DADAE0", "#414149");
		Color val3 = C("#FCFCFE", "#26262B");
		return new UiPalette
		{
			IsDark = dark,
			IsFluent = false,
			IsLiquidGlass = true,
			Canvas = C("#EBECF0", "#17181D"),
			Surface = C("#F2F2F7", "#202126"),
			SurfaceRaised = val3,
			SurfaceInset = C("#F1F1F5", "#1C1C21"),
			SurfaceHover = C("#E8E8EE", "#37373E"),
			Border = border,
			BorderStrong = C("#BEBEC6", "#606069"),
			Text = val2,
			TextMuted = textMuted,
			TextDim = textDim,
			Accent = val,
			AccentStrong = C("#0067D8", "#0067D8"),
			AccentSoft = C("#E3EDFC", "#223045"),
			OnAccent = Colors.White,
			Success = C("#19672F", "#55D578"),
			Warning = C("#7A4A00", "#F4BA50"),
			Danger = C("#A92220", "#FF9B96"),
			PrimaryHover = C("#005EC7", "#0067D8"),
			PrimaryPressed = C("#0054B4", "#005ABD"),
			PrimaryHoverText = Colors.White,
			SecondaryFill = C("#FFFFFF", "#36363D"),
			SecondaryPressed = C("#DEDEE5", "#45454E"),
			ToolbarPressed = C("#DDDDE5", "#45454E"),
			ChipHover = C("#E8E8EF", "#3B3B44"),
			ChipPressed = C("#E1E1E8", "#3A3A42"),
			ChipPressedHover = C("#D8D8E0", "#45454E"),
			ChipPressedBorder = UiKit.Clear,
			ChipPressedText = val2,
			DisabledFill = C("#EDEDF1", "#29292F"),
			DisabledBorder = UiKit.Clear,
			FocusRing = UiKit.Alpha(val, 0.65f),
			DangerFill = C("#FCEAE9", "#452C2F"),
			DangerHover = C("#F9DBD9", "#553035"),
			DangerPressed = C("#F3CAC7", "#65383D"),
			DangerText = C("#BF2825", "#FF817B"),
			DangerHoverText = C("#A51D1A", "#FFA39D"),
			HoverBorder = C("#C5C5CD", "#62626D"),
			FieldFocusBorder = val,
			TreeFocusBorder = UiKit.Alpha(val, 0.5f),
			TreeHoverSelected = C("#D9E6F8", "#26354B"),
			TreeCursor = val,
			ProgressFill = C("#8CBFFF", "#25589C"),
			ProgressOutline = UiKit.Clear,
			TooltipFill = val3,
			TextShadow = UiKit.Clear,
			SurfaceShadow = new Color(0f, 0f, 0f, dark ? 0.24f : 0.08f),
			ScrollTrack = UiKit.Clear,
			ScrollGrabber = C("#A9A9B2", "#777782"),
			ScrollGrabberHover = C("#90909B", "#898996"),
			ScrollGrabberPressed = C("#73737F", "#AAAAB6")
		};
		Color C(string light, string night)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			return UiKit.Html(dark ? night : light);
		}
	}
}

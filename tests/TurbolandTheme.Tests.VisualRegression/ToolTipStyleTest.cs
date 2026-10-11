using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xunit;
using Theme = TurbolandTheme.Wpf.TurbolandTheme;
using ThemeMode = TurbolandTheme.Core.ThemeMode;

namespace TurbolandTheme.Tests.VisualRegression;

/// <summary>
/// Verifies the implicit tooltip style's requested palette, typography and standard
/// WPF customization contract.
/// </summary>
[Collection("Wpf")]
public class ToolTipStyleTest
{
    private readonly Application _app;

    public ToolTipStyleTest(WpfApplicationFixture fixture) => _app = fixture.App;

    /// <summary>
    /// The authentic default is white IBM VGA text on the VGA cyan surface, with the
    /// stock platform shadow disabled in favor of square retro chrome.
    /// </summary>
    [Fact]
    public void DefaultToolTip_UsesCyanWhiteIbmVgaSkin()
    {
        StaThread.Run(() =>
        {
            Theme.Apply(_app, ThemeMode.Authentic, 1);

            ToolTip toolTip = Realize(new ToolTip { Content = "Compile current project" });

            Assert.Equal(
                (Color)_app.Resources["Turboland.Color.Palette.3"],
                ((SolidColorBrush)toolTip.Background).Color);
            Assert.Equal(
                (Color)_app.Resources["Turboland.Color.Palette.15"],
                ((SolidColorBrush)toolTip.Foreground).Color);
            Assert.Equal(_app.Resources["Turboland.Font.Primary"], toolTip.FontFamily);
            Assert.Equal(_app.Resources["Turboland.Font.Size.Primary"], toolTip.FontSize);
            Assert.False(toolTip.HasDropShadow);
            Assert.True(toolTip.OverridesDefaultStyle);

            var face = (Border)toolTip.Template.FindName("face", toolTip);
            Assert.Same(toolTip.Background, face.Background);
            Assert.Same(toolTip.BorderBrush, face.BorderBrush);
            Assert.Equal(toolTip.BorderThickness, face.BorderThickness);
            Assert.Equal(toolTip.Padding, face.Padding);
        });
    }

    /// <summary>
    /// Applications can still customize the standard ToolTip properties because the
    /// template binds its face and content presenter to the templated parent.
    /// </summary>
    [Fact]
    public void Template_HonorsStandardToolTipProperties()
    {
        StaThread.Run(() =>
        {
            Theme.Apply(_app, ThemeMode.Authentic, 1);

            SolidColorBrush background = new(Colors.Magenta);
            SolidColorBrush foreground = new(Colors.Yellow);
            SolidColorBrush border = new(Colors.Lime);
            Thickness borderThickness = new(2);
            Thickness padding = new(18, 8, 9, 4);

            ToolTip toolTip = Realize(new ToolTip
            {
                Content = "Custom",
                Background = background,
                Foreground = foreground,
                BorderBrush = border,
                BorderThickness = borderThickness,
                Padding = padding,
                HorizontalContentAlignment = HorizontalAlignment.Right,
                VerticalContentAlignment = VerticalAlignment.Bottom,
            });

            var face = (Border)toolTip.Template.FindName("face", toolTip);
            var content = (ContentPresenter)toolTip.Template.FindName("content", toolTip);

            Assert.Same(background, face.Background);
            Assert.Same(foreground, toolTip.Foreground);
            Assert.Same(border, face.BorderBrush);
            Assert.Equal(borderThickness, face.BorderThickness);
            Assert.Equal(padding, face.Padding);
            Assert.Equal(HorizontalAlignment.Right, content.HorizontalAlignment);
            Assert.Equal(VerticalAlignment.Bottom, content.VerticalAlignment);
        });
    }

    private ToolTip Realize(ToolTip toolTip)
    {
        Style implicitStyle = Assert.IsType<Style>(_app.Resources[typeof(ToolTip)]);
        toolTip.Style = implicitStyle;
        toolTip.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        toolTip.Arrange(new Rect(toolTip.DesiredSize));
        toolTip.UpdateLayout();
        toolTip.ApplyTemplate();
        return toolTip;
    }
}

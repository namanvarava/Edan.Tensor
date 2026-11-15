using System.Threading;
using System.Windows;
using EDAN.Tensor.Desktop;
using EDAN.Tensor.Desktop.ViewModels;
using Xunit;

namespace EDAN.Tensor.Desktop.Tests;

/// <summary>실제 WPF 리소스·Window·Registry 화면 15개 구성이 모두 로드되는지 확인합니다.</summary>
public sealed class DesktopUiSmokeTests
{
    [Fact]
    public void MainWindowLoadsAllRegistryScreensOnStaThread()
    {
        Exception? failure = null;
        var completed = new ManualResetEventSlim(false);
        var thread = new Thread(() =>
        {
            try
            {
                var app = new App();
            app.InitializeComponent();
                var window = new MainWindow();
                var viewModel = Assert.IsType<MainWindowViewModel>(window.DataContext);
                Assert.Equal(15, viewModel.Screens.Count);
                Assert.Equal(15, viewModel.Screens.Select(x => x.ScreenId).Distinct(StringComparer.Ordinal).Count());
                Assert.Equal("PLT-TNT-001", viewModel.Screens[0].ScreenId);
                Assert.Equal("WRK-XDL-001", viewModel.Screens[^1].ScreenId);
                Assert.NotNull(window.Content);
                window.Close();
                app.Shutdown();
            }
            catch (Exception ex)
            {
                failure = ex;
            }
            finally
            {
                completed.Set();
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(completed.Wait(TimeSpan.FromSeconds(30)), "WPF smoke thread did not complete.");
        if (failure is not null) throw new Xunit.Sdk.XunitException("WPF UI smoke failed.", failure);
    }
}

using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;

namespace PhasmoPad {
    public static class MauiProgram {
        public static MauiApp CreateMauiApp() {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureLifecycleEvents(events => {
#if WINDOWS
                    events.AddWindows(windows => {
                        windows.OnWindowCreated(window => {
                            window.ExtendsContentIntoTitleBar=false;
                            var handle=WinRT.Interop.WindowNative.GetWindowHandle(window);
                            var id=Microsoft.UI.Win32Interop.GetWindowIdFromWindow(handle);
                            var appWindow=Microsoft.UI.Windowing.AppWindow.GetFromWindowId(id);
                            switch(appWindow.Presenter){
                                case Microsoft.UI.Windowing.OverlappedPresenter overlappedPresenter:
                                    overlappedPresenter.IsMaximizable=false;
                                    overlappedPresenter.IsResizable=false;
                                    overlappedPresenter.IsMinimizable=false;
                                    overlappedPresenter.SetBorderAndTitleBar(false,false);
                                    break;
                            }
                        });
                    });
#endif
                })
                .ConfigureFonts(fonts => {
                    fonts.AddFont("OpenSans-Regular.ttf","OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf","OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}

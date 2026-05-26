namespace PhasmoPad {
    public partial class MainPage:ContentPage {
        string text="";

        public MainPage() {
            InitializeComponent();
        }

        private void OnChangeClicked(object sender,EventArgs e) {
            if(text=="") {
                text="_g";
            }else if(text=="_g") {
                text="_r";
            }else if(text=="_r") {
                text="_s";
            }else {
                text="";
            }

            EMF.Source=$"evidence_emf{text}.png";
        }

        /*private void OnCounterClicked(object sender,EventArgs e) {
            count++;

            if(count==1)
                CounterBtn.Text=$"Clicked {count} time";
            else
                CounterBtn.Text=$"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }

#if WINDOWS
        private Microsoft.UI.Windowing.AppWindow GetAppWindow(MauiWinUIWindow window) {
            var handle=WinRT.Interop.WindowNative.GetWindowHandle(window);
            var id=Microsoft.UI.Win32Interop.GetWindowIdFromWindow(handle);
            var appWindow=Microsoft.UI.Windowing.AppWindow.GetFromWindowId(id);
            return appWindow;
        }
#endif

        private void OnCloseWindowClicked(object sender,EventArgs e){
#if WINDOWS
            var window=GetParentWindow().Handler.PlatformView as MauiWinUIWindow;
            window.Close();
#endif
        }

        private void OnToggleMinimizeClicked(object sender,EventArgs e){
#if WINDOWS
            var window =GetParentWindow().Handler.PlatformView as MauiWinUIWindow;
            var appWindow=GetAppWindow(window);
            switch(appWindow.Presenter){
                case Microsoft.UI.Windowing.OverlappedPresenter overlappedPresenter:
                    overlappedPresenter.Minimize();
                    break;
            }
#endif
        }*/
    }

}

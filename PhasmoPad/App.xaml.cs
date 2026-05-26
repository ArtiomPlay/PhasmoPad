
namespace PhasmoPad {
    public partial class App:Application {
        public App() {
            InitializeComponent();

            MainPage=new AppShell();
        }

        protected override Window CreateWindow(IActivationState? activationState) {
            var window=base.CreateWindow(activationState);
            var height=400;
            var width=400;

            window.Height=height;
            window.Width=width;

            return window;
        }
    }
}

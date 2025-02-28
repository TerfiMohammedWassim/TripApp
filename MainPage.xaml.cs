namespace TripApp
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }
        private void OnGenerateCitiesClicked(object sender , EventArgs e) 
        {
            Console.WriteLine(sender.ToString());
        }

        private void OnSolveClicked(object sender , EventArgs e)
        {

        }
    }

}

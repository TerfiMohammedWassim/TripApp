namespace TripApp
{
    public partial class MainPage : ContentPage
    {
        List<Cities> cities = [];

        public MainPage()
        {
            InitializeComponent();
        }
        private void OnGenerateCitiesClicked(object sender , EventArgs e) 
        {
            Console.WriteLine(e.ToString());
        }

        private void OnSolveClicked(object sender , EventArgs e)
        {

        }
    }

}

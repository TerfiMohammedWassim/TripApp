
namespace TripApp
{
    class Cities
    {
        private String name;
        private double coordination_x;
        private double coordination_y;
        private List<Cities> goingTo;
        private bool is_start_city = false;

        public Cities(string name, double coordination_x, double coordination_y, List<Cities> goingTo)
        {
            this.name = name;
            this.coordination_x = coordination_x;
            this.coordination_y = coordination_y;
            this.goingTo = goingTo;
        }

        public Cities (String name , double coordination_x , double coordination_y , List<Cities> goingTo , bool is_starting_city)
        {
            this.name = name;
            this.coordination_y = coordination_y;
            this.coordination_x = coordination_x;
            this.goingTo = goingTo;
            this.is_start_city = is_starting_city;
        }

        public bool is_Starting_City()
        {
            return this.is_start_city;
        }

        public String getName()
        {
            return this.name;
        }
        public double getCoordinationX()
        {
            return this.coordination_x;
        }

        public double getCoordinationY()
        {
            return this.coordination_y;
        }

        public List<Cities> getGoingTo()
        {
            return this.goingTo;
        }


    }
}

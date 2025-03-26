namespace GymBooking.Models.ViewModels
{
    public class CreateGymClassViewModel
    {
        public string Name { get; set; }

        public DateTime StartTime { get; set; }

        public TimeSpan Duration { get; set; }

        public string Description { get; set; }
    }
}

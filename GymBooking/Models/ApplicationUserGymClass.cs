namespace GymBooking.Models
{
    public class ApplicationUserGymClass
    {
        public int GymClassId { get; set; }
        public string ApplicationUserId { get; set; }

        public ApplicationUser User { get; set; }
        public GymClass GymClass { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace Avalon.Model
{
    public class AvatarModel
    {
        [StringLength(2, ErrorMessage = "Initials length cannot be more than 2 characters long.")]
        public string Initials { get; set; }
        public string InitialsColour { get; set; }
        public string CircleColour { get; set; }
    }
}

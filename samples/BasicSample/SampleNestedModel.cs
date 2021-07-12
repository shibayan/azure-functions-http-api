using System.ComponentModel.DataAnnotations;

namespace BasicSample
{
    public class SampleNestedModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public SampleChildModel Child { get; set; }
    }

    public class SampleChildModel
    {
        [Required]
        public string Name { get; set; }
    }
}

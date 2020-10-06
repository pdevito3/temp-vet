namespace Domain.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Pet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int PetId { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        // add-on property marker - Do Not Delete This Comment
    }
}
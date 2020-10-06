namespace Application.Dtos.Vet
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Domain.Common;

    public abstract class VetForManipulationDto 
    {
        public string Name { get; set; }
        public int Capacity { get; set; }
        public DateTime? OpenDate { get; set; }
        public bool HasSpayNeuter { get; set; }

        // add-on property marker - Do Not Delete This Comment
    }
}
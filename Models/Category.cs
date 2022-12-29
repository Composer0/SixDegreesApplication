using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace ContactPro.Models
{
    public class Category
    {
        public int Id { get; set; }
        
        [Required]
        public string? AppUserId { get; set; } //? at the end of string allows the AppUserId to be null. They are being done this way because of the way that .Net6 creates warnings for this.

        [Required]
        [Display(Name = "Category Name")]
        public string? Name { get; set; }

        //Virtual Property... tells application to create a foreign key to the AppUser model. FK is used as the identity in the migrations.
        public virtual AppUser? AppUser { get; set; }
        public virtual ICollection<Contact> Contacts { get; set; } = new HashSet<Contact>();
    }
}

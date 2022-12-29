using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
// Allows us to markup classes. Affects the way that the communications are processed.
using Microsoft.AspNetCore.Identity;
using NuGet.ContentModel;

namespace ContactPro.Models
{
    public class AppUser : IdentityUser //inherits from IdentityUser into our new class.
    {
        [Required]
        [Display(Name = "First Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and a max {1} characters long.", MinimumLength = 2)] //Max Length followed by error followed by Minimum Length.
        public string? FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and a max {1} characters long.", MinimumLength = 2)] //0 = The Name 2 = the second property min length 1 = the first property max length.
        public string? LastName { get; set; }

        [NotMapped]
        public string? FullName { get { return $"{FirstName} {LastName}"; } } // Similar to JavaScript $`... interpolates into document.

        //Below was needed to be added in order for our category/contact list items to be found when using AppUser model in the Contacts Controller.
        public virtual ICollection<Contact> Contacts { get; set; } = new HashSet<Contact>();
        public virtual ICollection<Category> Categories { get; set; } = new HashSet<Category>();
    }
}

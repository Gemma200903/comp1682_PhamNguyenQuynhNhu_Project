using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using RaWMVC.Data.Entities;

namespace RaWMVC.Areas.Identity.Data;

public class RaWMVCUser : IdentityUser
{
    [Display(Name = "First Name")]
    public string? FirstName { get; set; }
    [Display(Name = "Last Name")]
    public string? LastName { get; set; }
    [Display(Name = "Email")]
    public string? Email { get; set; }
    [Display(Name = "Introduction")]
    public string? Introduction { get; set; }
    public string? ProfilePicture { get; set; }
    public DateTime JoinedDate { get; set; }
    public string? AvatarUrl { get; set; }
    public Guid? CurrentListId { get; set; }

    [NotMapped]
    public IFormFile? ProfilePictureFile { get; set; }
}
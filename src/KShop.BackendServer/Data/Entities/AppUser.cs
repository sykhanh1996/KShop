using KShop.BackendServer.Data.Enums;
using KShop.BackendServer.Data.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KShop.BackendServer.Data.Entities
{
    public class AppUser : IdentityUser, IDateTracking
    {
        public AppUser()
        {
        }

        public AppUser(string id, string userName, string firstName, string lastName,
            string email, string phoneNumber, DateTime? dob)
        {
            Id = id;
            UserName = userName;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            PhoneNumber = phoneNumber;
            Dob = dob;
        }

        [MaxLength(50)]
        [Required]
        public string FirstName { get; set; }

        [MaxLength(50)]
        [Required]
        public string LastName { get; set; }

        [MaxLength(100)]
        public string FullName { get; set; }

        public DateTime? Dob { get; set; }
        public string Avatar { get; set; }
        public int? NumberOfVotes { get; set; }
        public int? NumberOfReports { get; set; }
        public Status Status { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string CreatedBy { get; set; }

        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string ModifiedBy { get; set; }
        public ICollection<ActivityLog> ActivityLogs { get; set; }
    }
}
    using Microsoft.AspNetCore.Identity;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;


    namespace Domain.Entities
    {
        public class ApplicationUser : IdentityUser
        {
            // Name and surname
            public string? FirstName { get; set; }
            public string? LastName { get; set; }

            // Registration date
            public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

            // (optional) profile photo
            public string? ProfilePhotoUrl { get; set; }

            // Adoptions
            public ICollection<AdoptionApplication> AdoptionApplications { get; set; } = [];

            public ICollection<AdoptionContract> SignedContracts { get; set; } = [];

        }
    }

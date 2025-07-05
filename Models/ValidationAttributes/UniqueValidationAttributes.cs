using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using FEENALOoFINALE.Data;
using Microsoft.EntityFrameworkCore;

namespace FEENALOoFINALE.Models.ValidationAttributes
{
    public class UniqueEmailAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success;
            }

            var email = value.ToString()!;
            var userManager = validationContext.GetService(typeof(UserManager<User>)) as UserManager<User>;
            
            if (userManager != null)
            {
                var existingUser = userManager.FindByEmailAsync(email).Result;
                if (existingUser != null)
                {
                    return new ValidationResult("This email address is already registered.");
                }
            }

            return ValidationResult.Success;
        }
    }

    public class UniqueUsernameAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success;
            }

            var username = value.ToString()!;
            var userManager = validationContext.GetService(typeof(UserManager<User>)) as UserManager<User>;
            
            if (userManager != null)
            {
                var existingUser = userManager.FindByNameAsync(username).Result;
                if (existingUser != null)
                {
                    return new ValidationResult("This username is already taken.");
                }
            }

            return ValidationResult.Success;
        }
    }

    public class UniqueWorkerIdAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success;
            }

            var workerId = value.ToString()!;
            var context = validationContext.GetService(typeof(ApplicationDbContext)) as ApplicationDbContext;
            
            if (context != null)
            {
                var existingUser = context.Users.FirstOrDefault(u => u.WorkerId == workerId);
                if (existingUser != null)
                {
                    return new ValidationResult("This Worker ID is already in use.");
                }
            }

            return ValidationResult.Success;
        }
    }
}

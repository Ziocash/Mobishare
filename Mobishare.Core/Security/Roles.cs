using System;

namespace Mobishare.Core.Security
{
    /// <summary>
    /// Contains the claim names used for security management.
    /// </summary>
    public static class ClaimNames
    {
        /// <summary>
        /// The claim name for user roles.
        /// Currently set to "Admin".
        /// </summary>
        public const string Role = "Role";
    }

    /// <summary>
    /// Defines the available roles in the system.
    /// </summary>
    public enum Roles
    {
        /// <summary>
        /// Standard user with limited access.
        /// </summary>
        User,

        /// <summary>
        /// Technician with specific maintenance permissions.
        /// </summary>
        Technician,

        /// <summary>
        /// Staff member with management-level access.
        /// </summary>
        Staff,

        /// <summary>
        /// Administrator with full privileges.
        /// </summary>
        Admin
    }
}

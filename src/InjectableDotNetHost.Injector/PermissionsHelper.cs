using System.Security.AccessControl;

namespace InjectableDotNetHost.Injector
{
    /// <summary>
    /// Helpers for setting file permissions.
    /// </summary>
    public static class PermissionsHelper
    {
        /// <summary>
        /// Adds a DACL record for a given file.
        /// </summary>
        /// <param name="filePath">File path.</param>
        /// <param name="account">Account to add permissions to.</param>
        /// <param name="rights">Permissions to add.</param>
        /// <param name="controlType">Control Type.</param>
        public static void AddFileSecurity(string filePath, string account, FileSystemRights rights, AccessControlType controlType)
        {
            FileInfo dInfo = new FileInfo(filePath);
            FileSecurity dSecurity = dInfo.GetAccessControl();

            PropagationFlags pFlags = PropagationFlags.None;
            dSecurity.AddAccessRule(new FileSystemAccessRule(account, rights, InheritanceFlags.None, pFlags, controlType));
            dInfo.SetAccessControl(dSecurity);
        }

        public static void AddDirectorySecurity(string DirPath, string Account, FileSystemRights Rights, AccessControlType ControlType)
        {
            DirectoryInfo dInfo = new DirectoryInfo(DirPath);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();

            InheritanceFlags iFlags = InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit;
            PropagationFlags pFlags = PropagationFlags.None;
            dSecurity.AddAccessRule(new FileSystemAccessRule(Account, Rights, iFlags, pFlags, ControlType));
            dInfo.SetAccessControl(dSecurity);
        }

        /// <summary>
        /// Make a certain filesystem node UWP-injectable by adding the right permissions for  "ALL APPLICATION PACKAGES".
        /// For directories, it adds the permission recursively.
        /// </summary>
        /// <param name="absolutePath">Either a file or directory path.</param>
        public static void MakeUwpInjectable(string absolutePath)
        {
            if (File.Exists(absolutePath))
            {
                AddFileSecurity(absolutePath, "ALL APPLICATION PACKAGES",
                    FileSystemRights.Read | FileSystemRights.ReadAndExecute, AccessControlType.Allow);
            }
            else
            {
                AddDirectorySecurity(absolutePath, "ALL APPLICATION PACKAGES",
                    FileSystemRights.Read | FileSystemRights.ReadAndExecute, AccessControlType.Allow);
                
            }
        }
    }
}

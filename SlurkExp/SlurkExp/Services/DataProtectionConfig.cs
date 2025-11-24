using Microsoft.AspNetCore.DataProtection;

#nullable disable

namespace SlurkExp.Services
{
    public static class DataProtectionConfig
    {
        public static IServiceCollection AddCustomDataProtection(this IServiceCollection services)
        {
            if (GetKeyRingDirInfo() != null)
            {
                services.AddDataProtection()
                .PersistKeysToFileSystem(GetKeyRingDirInfo())
                .SetApplicationName("SlurkExp");
            }

            return services;
        }

        public static DirectoryInfo GetKeyRingDirInfo()
        {
            var startupAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            var applicationBasePath = System.AppContext.BaseDirectory;
            var directoryInfo = new DirectoryInfo(applicationBasePath);
            do
            {
                var keyRingDirectoryInfo = new DirectoryInfo(Path.Combine(directoryInfo.FullName, "data", "keyring"));
                if (keyRingDirectoryInfo.Exists)
                {
                    return keyRingDirectoryInfo;
                }
                directoryInfo = directoryInfo.Parent;
            }
            while (directoryInfo.Parent != null);
            return null;
        }
    }
}

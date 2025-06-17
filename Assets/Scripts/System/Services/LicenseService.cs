using SyskenTLib.LicenseMaster;

public class LicenseService : ILicenseService
{
    public string GetLicenseText(LicenseManager licenseManager)
    {
        if (licenseManager == null) return string.Empty;
        
        var licenses = licenseManager.GetLicenseConfigsTxt();
        licenses = "\n\n\n\n" + licenses;
        
        return licenses;
    }
}
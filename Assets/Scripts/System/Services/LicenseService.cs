using SyskenTLib.LicenseMaster;

public class LicenseService : ILicenseService
{
    private readonly LicenseManager _licenseManager;
    public LicenseService(LicenseManager licenseManager)
    {
        this._licenseManager = licenseManager;
    }
    
    public string GetLicenseText()
    {
        var licenses = _licenseManager.GetLicenseConfigsTxt();
        licenses = "\n\n\n\n" + licenses;
        
        return licenses;
    }
}
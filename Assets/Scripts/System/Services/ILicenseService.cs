using SyskenTLib.LicenseMaster;

public interface ILicenseService
{
    string GetLicenseText(LicenseManager licenseManager);
}
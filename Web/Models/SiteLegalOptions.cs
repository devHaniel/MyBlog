namespace Web.Models;

public sealed class SiteLegalOptions
{
    public string ResponsibleName { get; set; } =
        "PENDIENTE: nombre completo o razón social del responsable";

    public string PrivacyEmail { get; set; } =
        "PENDIENTE: correo de contacto para privacidad";

    public string BusinessAddress { get; set; } =
        "PENDIENTE: domicilio de contacto del responsable";

    public string Domain { get; set; } = "PENDIENTE: dominio del sitio";
    public string HostingProvider { get; set; } = "PENDIENTE: proveedor de alojamiento";
    public string EmailProvider { get; set; } = "PENDIENTE: proveedor de correo SMTP";
    public string Jurisdiction { get; set; } = "Tegucigalpa, Honduras";
    public string LastUpdated { get; set; } = "11 de noviembre de 2026";
}

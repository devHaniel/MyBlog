namespace Web.Models;

public sealed class SiteContentOptions
{
    public string SubscriptionTitle { get; set; } =
        "Si hay algo que todavía no te han explicado está aquí";

    public string SubscriptionDescription { get; set; } =
        "Explora tutoriales, ejemplos y recursos para aprender C# y .NET, paso a paso.";

    public string SubscriptionButtonText { get; set; } = "Suscríbete";
    public string SubscriptionButtonUrl { get; set; } = "/Account/Register";
    public string SiteName { get; set; } = "Mi Blog";
    public string FooterSlogan { get; set; } =
        "Construye tu negocio online y empieza a generar beneficios de verdad.";
    public string FooterDomain { get; set; } = "tudominio.com";

    public List<SiteLinkOptions> ResourceLinks { get; set; } =
    [
        new() { Text = "Suscríbete", Url = "/Account/Register" },
        new() { Text = "Cursos", Url = "https://learn.microsoft.com/es-es/training/" },
        new() { Text = "Blog", Url = "/#publicaciones" }
    ];

    public List<SiteLinkOptions> FeaturedLinks { get; set; } =
    [
        new() { Text = "Colaboraciones", Url = "mailto:contacto@example.com" },
        new() { Text = "Contacto", Url = "mailto:contacto@example.com" }
    ];

    public List<SiteSocialLinkOptions> SocialLinks { get; set; } =
    [
        new() { Name = "YouTube", Url = "https://www.youtube.com/" },
        new() { Name = "Instagram", Url = "https://www.instagram.com/" },
        new() { Name = "X", Url = "https://x.com/" }
    ];

    public List<SiteLinkOptions> LegalLinks { get; set; } =
    [
        new() { Text = "Política de Privacidad", Url = "/privacidad" },
        new() { Text = "Política de Cookies", Url = "/cookies" },
        new() { Text = "Aviso Legal", Url = "/aviso-legal" }
    ];
}

public sealed class SiteLinkOptions
{
    public string Text { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public sealed class SiteSocialLinkOptions
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

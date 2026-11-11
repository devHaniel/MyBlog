# Mi Blog Personal

Un espacio donde comparto los conocimientos que voy adquiriendo día a día, tanto en el ámbito de la programación como en mi formación en Ingeniería en Informática.

Mi objetivo es documentar mi aprendizaje, compartir soluciones a problemas que encuentro durante el desarrollo de software y aportar conocimientos a otras personas mediante artículos, ejemplos de código, proyectos y herramientas.

## Desarrollo

**Tecnologías utilizadas:**
- .NET 10
- ASP.NET Core MVC
- PostgreSQL
- Entity Framework Core
- C#
- HTML, CSS y JavaScript

## Características

- Publicación y administración de artículos.
- Organización de contenido mediante categorías.
- Artículos técnicos con ejemplos de código.
- Lectura adaptable de publicaciones con contenido Markdown/HTML sanitizado.
- Comentarios moderados con respuestas anidadas.
- Documentación de aprendizajes y experiencias.
- Diseño adaptable a diferentes dispositivos.

## Configuración de la vista pública

Personaliza el banner de suscripción y los enlaces, redes sociales, nombre, eslogan y dominio del footer en la sección `SiteContent` de `Web/appsettings.json`. Reemplaza los enlaces de ejemplo por los destinos de tu sitio.

Los artículos usan la URL pública `/posts/{slug}` (por ejemplo, `/posts/que-es-csharp-guia-ejemplo`). La ruta anterior `/Home/Detail?slug=...` se conserva como alias, y las vistas enlazan a la ruta canónica.

Las páginas públicas `/privacidad`, `/cookies` y `/aviso-legal` son borradores informativos en español. Completa los campos `PENDIENTE` de `SiteLegal` en `Web/appsettings.json` (titular, correo de privacidad, domicilio, dominio, hosting y SMTP), verifica qué cookies y proveedores se usan en producción y solicita una revisión legal local antes de publicar. No se ha configurado analítica ni publicidad en la aplicación actual.

Para habilitar respuestas anidadas en una base de datos existente, aplica la nueva migración:

```sh
dotnet ef database update --project Infrastructure/Infrastructure.csproj --startup-project Web/Web.csproj
```

## Objetivo del proyecto

Más que un blog, este proyecto representa un espacio para documentar mi crecimiento como desarrollador, poner en práctica nuevos conocimientos y compartir lo que voy aprendiendo durante mi carrera.
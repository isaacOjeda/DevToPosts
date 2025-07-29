# Autorización Avanzada en ASP.NET Core - Guía de Implementación

Este proyecto demuestra conceptos avanzados de autorización en ASP.NET Core, incluyendo autorización basada en roles, claims y políticas personalizadas.

## 🎯 Objetivos del Proyecto

- **Autorización basada en Roles**: Implementación tradicional con roles de usuario
- **Autorización basada en Claims**: Uso de claims para decisiones de autorización más granulares
- **Autorización basada en Políticas**: Enfoque principal del proyecto
  - Custom Requirements y Authorization Handlers
  - Policy Builders para Minimal APIs
  - Sistema de permisos granulares

## 📋 Plan de Ejecución

### Fase 1: Configuración Base y Dependencias

- [x] **1.1** Agregar paquetes NuGet necesarios
  - `Microsoft.AspNetCore.Authentication.JwtBearer` ✅
  - `System.IdentityModel.Tokens.Jwt` ✅
- [x] **1.2** Configurar servicios de autenticación y autorización ✅
- [x] **1.3** Crear estructura de carpetas del proyecto ✅

### Fase 2: Modelos y Enums Base

- [x] **2.1** Crear enum `Permission` con permisos del sistema ✅
- [x] **2.2** Crear enum `Role` con roles base ✅
- [x] **2.3** Crear modelo `User` para representar usuarios ✅
- [x] **2.4** Crear constantes para Claims y Policies ✅

### Fase 3: Sistema de Autenticación Mock

- [x] **3.1** Implementar servicio mock para generar tokens JWT ✅
- [x] **3.2** Crear endpoint de login que genere tokens con roles y permisos ✅
- [x] **3.3** Configurar middleware de autenticación JWT ✅

### Fase 4: Autorización Basada en Roles

- [ ] **4.1** Crear endpoints protegidos con `[Authorize(Roles = "...")]`
- [ ] **4.2** Demostrar autorización por roles simples
- [ ] **4.3** Implementar endpoints para Admin, Manager y User

### Fase 5: Autorización Basada en Claims

- [ ] **5.1** Crear endpoints que validen claims específicos
- [ ] **5.2** Implementar validación de claims personalizados
- [ ] **5.3** Demostrar diferencia entre roles y claims

### Fase 6: Sistema de Permisos Personalizado (Foco Principal)

- [ ] **6.1** Crear `PermissionRequirement` implementando `IAuthorizationRequirement`
- [ ] **6.2** Implementar `PermissionAuthorizationHandler` heredando de `AuthorizationHandler<PermissionRequirement>`
- [ ] **6.3** Crear extension methods para configurar políticas en Minimal APIs
- [ ] **6.4** Registrar requirements, handlers y políticas en el contenedor DI
- [ ] **6.5** Implementar múltiples handlers para demostrar evaluación OR

### Fase 7: Endpoints de Demostración

- [ ] **7.1** Crear endpoints que requieran permisos específicos:
  - `users.read` - Leer usuarios
  - `users.write` - Crear/editar usuarios
  - `users.delete` - Eliminar usuarios
  - `reports.read` - Leer reportes
  - `reports.write` - Crear reportes
  - `admin.access` - Acceso administrativo
- [ ] **7.2** Implementar endpoints con diferentes combinaciones de permisos
- [ ] **7.3** Crear endpoints que requieran múltiples permisos

### Fase 8: Utilidades y Helpers

- [ ] **8.1** Crear extension methods para facilitar el uso (`RequirePermission`, `RequireAnyPermission`)
- [ ] **8.2** Implementar middleware de logging de autorización usando `IAuthorizationService`
- [ ] **8.3** Crear helper para verificar permisos programáticamente con `IAuthorizationService.AuthorizeAsync`
- [ ] **8.4** Demostrar uso de `AuthorizationHandlerContext.Resource` para autorización basada en recursos

### Fase 9: Testing y Documentación

- [ ] **9.1** Crear archivo `.http` con ejemplos de uso
- [ ] **9.2** Documentar todos los endpoints en OpenAPI
- [ ] **9.3** Agregar ejemplos de tokens JWT para diferentes usuarios
- [ ] **9.4** Crear documentación de uso en README

### Fase 10: Casos de Uso Avanzados

- [ ] **10.1** Implementar autorización basada en recursos usando `AuthorizationHandlerContext.Resource`
- [ ] **10.2** Crear combinación de múltiples requirements (evaluación AND)
- [ ] **10.3** Demostrar handlers que manejan múltiples tipos de requirements
- [ ] **10.4** Implementar requirement con handler integrado (`IAuthorizationRequirement` + `IAuthorizationHandler`)
- [ ] **10.5** Configurar `InvokeHandlersAfterFailure` para demostrar side effects

## 🏗️ Estructura del Proyecto Final

```
/
├── Program.cs                              # Configuración principal
├── README.md                               # Este archivo
├── AdvancedAuthorization.http             # Ejemplos de pruebas
├── /Models/
│   ├── User.cs                            # Modelo de usuario
│   ├── Permission.cs                      # Enum de permisos
│   ├── Role.cs                           # Enum de roles
│   └── Constants/
│       └── AuthConstants.cs              # Constantes de autorización
├── /Services/
│   ├── ITokenService.cs                  # Interface del servicio de tokens
│   ├── TokenService.cs                   # Servicio mock de tokens JWT
│   └── IUserService.cs                   # Interface del servicio de usuarios
├── /Authorization/
│   ├── Requirements/
│   │   ├── PermissionRequirement.cs      # Custom requirement para permisos
│   │   └── MinimumAgeRequirement.cs      # Ejemplo de requirement con parámetros
│   ├── Handlers/
│   │   ├── PermissionAuthorizationHandler.cs  # Handler principal de permisos
│   │   ├── AdminBypassHandler.cs         # Handler alternativo (evaluación OR)
│   │   └── MinimumAgeHandler.cs          # Handler de ejemplo
│   ├── Policies/
│   │   └── AuthorizationPolicies.cs      # Configuración centralizada de políticas
│   └── Extensions/
│       └── AuthorizationExtensions.cs    # Extension methods para Minimal APIs
├── /Endpoints/
│   ├── AuthEndpoints.cs                  # Endpoints de autenticación
│   ├── UserEndpoints.cs                  # Endpoints de usuarios
│   ├── ReportEndpoints.cs               # Endpoints de reportes
│   └── AdminEndpoints.cs                # Endpoints administrativos
└── /Middleware/
    └── AuthorizationLoggingMiddleware.cs # Middleware de logging
```

## 🔑 Conceptos Clave a Demostrar

### 1. **Autorización Basada en Roles**

```csharp
// Ejemplo tradicional
app.MapGet("/admin", () => "Admin content")
   .RequireAuthorization("Admin");
```

### 2. **Autorización Basada en Claims**

```csharp
// Verificación de claims específicos
app.MapGet("/manager", () => "Manager content")
   .RequireAuthorization(policy => policy.RequireClaim("department", "IT"));
```

### 3. **Autorización Basada en Permisos (Foco Principal)**

```csharp
// Usando extension methods personalizados
app.MapGet("/users", GetUsers)
   .RequirePermission(Permission.UsersRead);

app.MapPost("/users", CreateUser)
   .RequirePermission(Permission.UsersWrite);

app.MapDelete("/users/{id}", DeleteUser)
   .RequirePermission(Permission.UsersDelete);

// Usando políticas nombradas
app.MapGet("/reports", GetReports)
   .RequireAuthorization("CanReadReports");
```

### 4. **Custom Requirements y Handlers**

```csharp
// Requirement personalizado
public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }
    public PermissionRequirement(string permission) => Permission = permission;
}

// Handler que evalúa el requirement
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.HasClaim("permissions", requirement.Permission))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
```

### 5. **Evaluación OR con Múltiples Handlers**

```csharp
// Múltiples formas de cumplir el mismo requirement
// Handler 1: Verificar permiso directo
// Handler 2: Verificar si es administrador (bypass)
// Si cualquiera de los dos handlers llama context.Succeed(),
// la autorización es exitosa
```

## 🎨 Características Especiales

1. **Sistema de Permisos Granulares**: Cada acción tiene un permiso específico
2. **Composición de Permisos**: Posibilidad de requerir múltiples permisos
3. **Extension Methods**: API limpia y fácil de usar
4. **Logging de Autorización**: Rastreo de decisiones de autorización
5. **Tokens JWT Mock**: Simulación realista sin dependencias externas

## 🚀 Cómo Ejecutar

1. Ejecutar el proyecto: `dotnet run`
2. Hacer login: `POST /auth/login` con credentials
3. Usar el token en los headers: `Authorization: Bearer {token}`
4. Probar diferentes endpoints según los permisos del usuario

## 📝 Notas para el Blog Post

### Conceptos Técnicos Clave

- **IAuthorizationRequirement**: Interface marcador sin métodos que define un requirement
- **IAuthorizationHandler**: Interface para handlers que evalúan requirements
- **AuthorizationHandler<T>**: Clase base genérica para handlers de un tipo específico
- **AuthorizationHandlerContext**: Contexto que contiene User, Resource y métodos Succeed/Fail
- **IAuthorizationService**: Servicio principal que orquesta la evaluación de autorización

### Patrones de Autorización

- **Evaluación AND**: Múltiples requirements en una política - todos deben cumplirse
- **Evaluación OR**: Múltiples handlers para un requirement - cualquiera puede autorizar
- **Resource-based**: Usar AuthorizationHandlerContext.Resource para decisiones contextuales
- **Side Effects**: Handlers que ejecutan logging incluso después de fallos

### Best Practices Demostradas

- **Separación de responsabilidades**: Requirements vs Handlers vs Policies
- **Registro correcto en DI**: Handlers como Singleton/Scoped según necesidad
- **Extension methods**: API limpia para desarrolladores
- **Manejo de fallos**: Cuándo usar context.Fail() vs simplemente no llamar Succeed()
- **Performance**: Consideraciones sobre múltiples handlers y evaluación temprana

---

**Objetivo**: Este proyecto sirve como ejemplo completo y funcional para entender la autorización avanzada en ASP.NET Core, con énfasis especial en sistemas de permisos granulares usando Custom Requirements y Authorization Handlers.

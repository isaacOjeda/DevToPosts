namespace StrategyFactoryPattern.Application.Notifications;


public interface INotificationsFactory
{
    /// <summary>
    /// Crea un servicio de notificaciones según el proveedor configurado en el archivo de configuración.
    /// </summary>
    /// <returns></returns>
    INotificationsStrategy CreateNotificationService();

    /// <summary>
    /// Crear un servicio de notificaciones según el proveedor especificado.
    /// </summary>
    /// <param name="provider"></param>
    /// <returns></returns>
    INotificationsStrategy CreateNotificationService(NotificationProvider provider);
}
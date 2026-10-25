# Despliegue en IIS (VM servidor del laboratorio Azure UMG)

Reemplaza 10.0.0.4 por la IP privada real del servidor (ipconfig). Puertos usados: API 5000, Web 80.

1. Instalar **.NET 10 Hosting Bundle**, MySQL 8 y, en IIS, las caracteristicas **Web Server** y **WebSocket Protocol**.
2. Zona horaria de la VM: `tzutil /s "Central America Standard Time"` (si no, las fechas salen en UTC y una venta de la tarde queda con fecha de manana).
3. Ejecutar `PuntoVenta.sql` UNA sola vez (hace DROP DATABASE). Crear un usuario MySQL propio (no root) con permisos sobre `punto_venta`.
4. Publicar: `dotnet publish PuntoVenta.Api -c Release -o C:\inetpub\pv-api` y `dotnet publish PuntoVenta.Web -c Release -o C:\inetpub\pv-web`.
5. Copiar `appsettings.Production.Api.json` -> `C:\inetpub\pv-api\appsettings.Production.json` y
   `appsettings.Production.Web.json` -> `C:\inetpub\pv-web\appsettings.Production.json`, completando passwords y clave JWT.
6. IIS: crear dos sitios (API puerto 5000, Web puerto 80), cada uno con su Application Pool en **Sin codigo administrado**.
   No poner la API como aplicacion hija `/api` (quedaria /api/api/...).
7. Permisos: dar **Modificar** a `IIS AppPool\<pool de la API>` sobre `C:\inetpub\pv-api\wwwroot\images\productos`.
8. Firewall de Windows: abrir TCP 5000 y 80 de entrada. En Azure (NSG) verificar que la red virtual los permita entre VMs.
9. Probar desde otra VM: `http://10.0.0.4` (usuario admin / Admin123*) y `http://10.0.0.4:5000/images/productos/` para las imagenes.
10. Cambiar la contrasena de `admin` despues de la demo.

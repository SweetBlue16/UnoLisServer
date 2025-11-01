# 🃏 UNO-LIS Server
Bienvenido al backend de **UNO-LIS**. Este repositorio contiene el código fuente del servidor que gestiona la lógica del juego, las conexiones de los jugadores y la persistencia de datos para el juego **UnoLisClient**. Desarrollado en C# / .NET Framework 4.7.2 con WCF, Entity Framework 6.5.1 y log4net.

---

## 🧩 Descripción general
El servidor UNO-LIS implementa toda la lógica central del juego, el manejo de sesiones, la comunicación entre clientes y el acceso a la base de datos. Su arquitectura está basada en **Windows Communication Foundation (WCF)** y **Entity Framework** para garantizar rendimiento y modularidad.

## ⚙️ Tecnologías principales
| Componente | Tecnología |
| -------------- | -------------- |
| Lenguaje | C# 7.3 |
| Framework | .NET Framework 4.7.2 |
| Comunicación | Windows Communication Foundation (WCF) |
| ORM | Entity Framework 6.5.1 |
| Logging | log4net 3.2.0 |
| Análisis | SonarQube |

## 🧠 Funcionalidades principales
- Gestión de usuarios y autenticación.
- Administración de partidas, lobbies y sesiones.
- Comunicación cliente-servidor vía WCF.
- Registro de eventos con log4net.
- Validaciones y códigos de respuesta unificados.
- Seguridad basada en hashing y consultas parametrizadas.

## 🚀 Ejecución local
1. Clona el repositorio:
```bash
git clone https://github.com/SweetBlue16/UnoLisServer.git
```
2. Abre la solución en **Visual Studio 2022** (modo administrador recomendado).
3. Establece **UnoLisServer.Host** como proyecto de inicio.
4. Ejecuta la solución (iniciará el host WCF).

## 👥 Autores
- Mauricio
- Erickmel

## 🏫 Licencia
Proyecto académico para la experiencia educativa de **Tecnologías para la Construcción de Software** en la Universidad Veracruzana, Facultad de Estadística e Informática. Uso educativo sin fines comerciales.

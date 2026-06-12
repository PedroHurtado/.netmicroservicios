# Módulo 5 — Seguridad en Microservicios

> **Duración:** ~3h
> **Proyecto hilo conductor:** ShopMicro
> **Enfoque:** lo que el desarrollador toca con las manos, no lo que monta el equipo de plataforma

---

## De qué va este módulo

La seguridad en un sistema distribuido tiene dos mitades muy distintas.

Una mitad es **infraestructura**: levantar el proveedor de identidad, emitir tokens,
rotar claves, configurar el IdP. Eso lo hace el equipo de plataforma y, en tu día a día
como desarrollador, **te viene dado**.

La otra mitad la programas tú: **validar** el token que llega, **leer** quién es el usuario
activo, y —lo más delicado en distribuido— **propagar** esa identidad cuando tu servicio
llama a otro. Un servicio que recibe una petición autenticada y luego llama a un tercero
sin reenviar la identidad acaba de perder al usuario por el camino.

Este módulo sigue **el viaje del Principal** a través de ShopMicro: cómo entra, cómo se
materializa, y cómo se propaga en llamada síncrona (HTTP y gRPC) y asíncrona (mensajería).

---

## Lo que viene dado vs. lo que programas tú

| Viene dado (plataforma) | Lo programas tú (desarrollador) |
|---|---|
| El IdP y la emisión de tokens | La validación del JWT en tu servicio |
| Las claves de firma (JWKS) | Leer el `ClaimsPrincipal` y sus claims |
| La rotación de secretos | Proteger tus endpoints (roles, políticas) |
| La configuración del realm | Propagar la identidad a otros servicios |

---

## Lecciones

- **5.1** — El modelo de identidad en un sistema distribuido
- **5.2** — El JWT que llega: validación y materialización del Principal
- **5.3** — Proteger los endpoints: roles y políticas
- **5.4** — Propagar el Principal en llamada SÍNCRONA (HTTP)
- **5.5** — Propagar el Principal en llamada SÍNCRONA (gRPC)
- **5.6** — Identidad en llamada ASÍNCRONA (mensajería)
- **5.7** — Secretos y configuración sensible

---

## Al terminar serás capaz de

- Explicar por qué cada microservicio valida el token por su cuenta y no confía en el vecino.
- Configurar JWT Bearer en un servicio y leer el usuario activo desde tu código.
- Proteger endpoints con roles y políticas.
- Reenviar la identidad del usuario en llamadas HTTP y gRPC sin repetir código en cada llamada.
- Decidir qué viaja —y qué no— en un mensaje asíncrono cuando el token ya no sirve.
- Gestionar secretos en desarrollo y entender cómo llegan a producción.

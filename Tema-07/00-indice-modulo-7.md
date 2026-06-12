# Módulo 7 — Despliegue y Observabilidad

> **Duración:** ~4h
> **Proyecto hilo conductor:** ShopMicro
> **Enfoque:** lo que el desarrollador empaqueta e instrumenta, no lo que opera plataforma

---

## De qué va este módulo

Igual que en seguridad, el despliegue y la operación de microservicios tienen una mitad que
**viene dada** (la monta el equipo de plataforma / devops) y otra que **programa el dev**.

Lo que viene dado: el clúster de Kubernetes, la red, el escalado, los pipelines de CI/CD, el
colector de telemetría. No vas a montar eso, y este módulo no pretende convertirte en devops.

Lo que sí tocas tú: **empaquetar tu servicio** (el Dockerfile), **levantar el sistema en local
para desarrollar** (docker-compose), **entender cómo tu configuración llega al pod** (el Secret
de K8s), y —lo más importante para el día a día en distribuido— **instrumentar tu servicio**
para que se pueda observar, propagando el contexto de traza entre micros y cuidando qué
información **no** debe cruzar la frontera de tu aplicación.

El centro de gravedad del módulo es la **observabilidad accionable**: cuando un pedido falla a
través de cinco servicios, ¿cómo sigues su rastro?

---

## Lo que viene dado vs. lo que programas tú

| Viene dado (plataforma / devops) | Lo programas tú (desarrollador) |
|---|---|
| El clúster de K8s, red, escalado | El Dockerfile de tu servicio |
| Los pipelines de CI/CD | El docker-compose para desarrollar en local |
| El colector de telemetría | La instrumentación OpenTelemetry de tu código |
| La rotación de Secrets | Saber qué configura tu servicio y qué no debe salir |

---

## Lecciones

- **7.1** — Empaquetar el servicio: Dockerfile multi-stage
- **7.2** — Orquestación local: docker-compose
- **7.3** — Configuración en el clúster: el Secret de K8s
- **7.4** — Los tres pilares: logs, métricas y trazas
- **7.5** — Instrumentar con OpenTelemetry
- **7.6** — Propagación de contexto entre microservicios
- **7.7** — La frontera de la app: qué NO debe salir
- **7.8** — Leer una traza en Jaeger

---

## Al terminar serás capaz de

- Escribir un Dockerfile multi-stage para un servicio .NET y explicar cada etapa.
- Levantar ShopMicro entero en local con docker-compose para desarrollar contra el sistema real.
- Entender cómo tu configuración sensible llega al pod a través de un Secret de K8s.
- Instrumentar tu servicio con OpenTelemetry y entender qué genera y quién lo consume.
- Explicar cómo el contexto de traza se propaga entre servicios para correlacionar una petición.
- Decidir qué datos enriquecen una traza interna y cuáles no deben cruzar la frontera externa.
- Seguir una petición real a través de ShopMicro en Jaeger y localizar dónde falla.

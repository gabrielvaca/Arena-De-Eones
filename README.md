# ⚔️ Arena de Eones ⚔️
## Estrategia en Tiempo Real 1v1 y Defensa de Torres

![ade-logo](https://github.com/user-attachments/assets/e49cb72d-0c7b-4b33-8e76-1ec023c5f32a)

**Versión del Documento:** 0.0 (Fase de Configuración Inicial)
**Fecha de Publicación:** 16/11/2025
**Escrito por:** Equipo Arena de Eones
**Contacto:** [Su correo electrónico de contacto principal]
**Copyright:** © 2025 Equipo Arena de Eones. Todos los derechos reservados.

---

## 💡 1. Resumen del Proyecto

**Arena de Eones** es un proyecto de juego móvil (RTS/Tower Defense) 1 contra 1, diseñado para partidas rápidas (3-5 minutos). El concepto central es un *duelo de Invocadores* que utilizan cartas (unidades y hechizos) para destruir las torres enemigas.

Este repositorio documenta el desarrollo de un **Prototipo Mínimo Viable (Vertical Slice)** de 4 semanas. El enfoque es validar la jugabilidad central: la gestión del recurso **Eón (Maná)** y la sincronización multijugador.

### Core Loop (Prototipo v0.1)

* **Género:** RTS Competitivo / Defensa de Torres.
* **Partidas:** 3 minutos con fase de **Doble Maná** al final.
* **Progreso:** Sistema de **Trofeos** simple para escalar rangos.
* **Objetivo:** Destruir las Torres de Arconte y la Torre del Rey.

---

## 🛠️ 2. Estructura y Planificación (Sprint de 4 Semanas)

Este proyecto se gestiona bajo la metodología **Agile/Scrum**, con todas las tareas definidas como *Issues* (tarjetas) en GitHub.

| Módulo | Estado Actual (v0.0) | Próximo Hito (Semana 1) |
| :--- | :--- | :--- |
| **Arquitectura Base** | **CONFIGURADO** | Implementación del **Sistema de Maná** (Issue #4). |
| **Gameplay Core** | **PLANIFICADO** | Implementación de la Lógica de **Torres** y el *Spawning* de Unidades (Issues #3, #6). |
| **Integración de Assets** | **BUSCANDO FUENTES** | **N/A** (Integración fuerte en Semana 3). |
| **Multijugador** | **PLANIFICADO** | Implementación de la solución de **Red** (Semana 2). |

> ➡️ **Para ver el detalle de tareas, asignaciones y progreso, consulte la pestaña [Projects] en este repositorio.**

---

## ⚙️ 3. Herramientas y Requerimientos

### Requisitos de Hardware para el Desarrollo

* **Mínimo:** Procesador Dual Core, 8 GB RAM, GPU compatible con DirectX 10.
* **Recomendado:** Procesador Quad Core (o superior), 16 GB RAM, SSD, GPU dedicada.

### Requisitos de Software y Stack Tecnológico

| Componente | Herramienta Elegida | Propósito |
| :--- | :--- | :--- |
| **Motor Principal** | Unity (LTS) | Entorno de desarrollo multiplataforma. |
| **Lenguaje** | C# | Lenguaje de programación principal del *Gameplay*. |
| **Control de Versiones** | Git / GitHub | Gestión de código en equipo y seguimiento de tarjetas. |
| **Gestión de Assets** | GitHub LFS (Large File Storage) | Manejo de modelos 3D y texturas. |
| **Networking** | [Por definir, pendiente de Issue #7] | Sincronización 1v1 en tiempo real. |

---

## 👥 4. Equipo de Desarrollo

| Nombre | Rol Profesional | Enfoque Principal en el Sprint |
| :--- | :--- | :--- |
| **Vaca Vega, Gabriel Enrique** | Game Director / Lead Game Designer | GDD, Balance, Reporte de Pruebas. |
| **García Aldama, Axel Adrián** | Lead Programmer / Core Systems Engineer | Arquitectura, Sistemas Centrales, Red. |
| **Álvarez Félix, Ángel Daniel** | Gameplay Programmer | Unidades, Comportamientos, Colisiones. |
| **Sánchez Nava, Jaime Israel** | UI Programmer / Front-End Developer | Interfaz de Usuario, Puntaje (Trofeos), Flujo. |
| **Arvizu Sandoval, Karolina** | Technical Artist / QA Tester | Integración de Assets, Pruebas de Calidad, Build Final. |

---

## 💾 5. Instalación y Ejecución

*El proyecto se encuentra en la **Fase de Configuración Inicial**. Los enlaces de descarga y ejecución estarán disponibles al completar el prototipo (Semana 4).*

* **Build Final (APK):** N/A
* **Jugar en Línea (Web GL):** N/A

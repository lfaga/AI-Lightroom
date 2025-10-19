# AI Image Manager ("AI Lightroom") - A WPF Digital Asset Manager for Generative AI

This repository contains the source code for a work-in-progress desktop application designed to streamline the workflow for anyone working with AI image generation services. It acts as both a universal client and a local library/asset manager, similar in concept to Adobe Lightroom.

### The Problem

Working with multiple AI image generation APIs is a chaotic process. Each has different parameters, and managing thousands of generated images—along with their prompts, models, and settings—quickly becomes a significant organizational challenge, often requiring manual folder structures and text files.

### The Solution

This application provides a unified interface to manage this entire workflow. It uses a provider-based model where each API service can be defined in a simple XML file. The application then dynamically builds the UI for that service, allowing the user to generate, save, and organize images from multiple sources in one place.

### Key Architectural Features:

*   **Schema-Driven UI:** The user interface for each API is generated dynamically at runtime based on an XML schema that defines the required parameters, types (string, choice, slider), and default values.
*   **Provider-Based Model:** The application is completely decoupled from any specific API, allowing for easy extension to new services without recompiling the code.
*   **Digital Asset Management (DAM):** Every generated image is saved with a corresponding `.json` sidecar file containing all the metadata (prompt, model, seed, etc.) required for **100% reproducibility.**
*   **Workflow Tools:** The library allows users to tag, filter, and sort images, as well as generate new variations by easily reusing and modifying the parameters from previous creations.
*   **Technology Stack:** C# with Windows Presentation Foundation (WPF) for the UI, demonstrating a robust desktop application architecture.

### Current Status

This is a functional but incomplete personal project. The core systems (provider model, dynamic UI, library management) are in place.

---

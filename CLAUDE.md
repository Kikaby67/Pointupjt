# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Pointu-PJT** is a Twitch mini-game and learning project built in C#. Viewers interact with a live streamer via Twitch chat commands, which register them as players in a narrative RPG world called **Arbonet**. Commands are executed through **Streamer.bot**, which runs C# scripts triggered by Twitch chat events.

## Build & Run

```bash
# Build the .NET console app
cd Apprentissage/Pointu-pjt
dotnet build

# Run
dotnet run
```

The `Apprentissage/Pointu-pjt/` project is a .NET 10.0 console application used for learning and prototyping C# logic before integrating it into Streamer.bot.

## Architecture

```
Pointu-PJT/
├── Apprentissage/Pointu-pjt/   # .NET 10.0 console app (learning/prototyping)
├── Streamerbot/                  # C# scripts for Streamer.bot chat commands
│   ├── Bonjour/                  # !bonjour — greets viewers
│   └── Rejoindre/                # !rejoindre — registers a new player
├── Donnees/joueurs/              # JSON player profiles (persisted per user)
└── Lore/                         # Narrative documentation for the game world
```

### Streamer.bot Commands

The files under `Streamerbot/` are **not compiled as a .NET project** — they are pasted directly into Streamer.bot's "C# Execute Code" sub-action editor. Each file contains a class with an `Execute` method that Streamer.bot calls at runtime.

- `Commande_bonjour.cs` — responds to `!bonjour` with a greeting and instructions
- `Commande_rejoindre.cs` — responds to `!rejoindre` by creating a JSON player file in `Donnees/joueurs/` if the player isn't already registered

### Player Data Format

Each player file is stored at `Donnees/joueurs/{username}.json`:
```json
{
  "nomJoueur": "username",
  "niveau": 1,
  "xp": 0,
  "ram": 10,
  "sac": []
}
```

### Game World

The narrative takes place in **Arbonet** — a world where nature and technology coexist. The player is guided by **Pointu**, an ancient turtle guardian. The antagonist is **Hector-Pierre Castor**, who corrupts the world's memories. Full lore is in `Lore/LA_LEGENDE_DE_POINTU_V2.md`.

## Tech Stack

- **C# / .NET 10.0** — game logic
- **Streamer.bot** — Twitch bot platform that executes C# scripts on chat commands
- **JSON** — player data persistence
- **Twitch + OBS** — streaming integration

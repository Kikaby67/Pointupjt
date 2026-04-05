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

## Discord Bot (DiscordBot/)

Un bot Discord Python déployé sur **Discloud** (plan gratuit) qui permet aux joueurs de consulter leur profil Arbonet depuis Discord.

```
DiscordBot/
├── bot_discord.py      # Bot principal (discord.py)
└── discloud.config     # Config de déploiement Discloud
```

### Commandes Discord

- `!profil` — affiche le profil du joueur (niveau, XP, RAM, sac)
- `!arbonet` — présente l'univers du jeu
- `!aide` — liste les commandes

Les commandes sont restreintes au channel `CHANNEL_ID = 1490232175382102016`.

### Données joueurs

Les profils sont lus depuis le repo GitHub **Kikaby67/Pointupjt** (repo public). Pas de token GitHub nécessaire pour la lecture — les appels API se font sans authentification.

### Déploiement Discloud

- `LANG=python` obligatoire dans `discloud.config`
- `DISCORD_TOKEN` hardcodé dans `bot_discord.py` (repo public GitHub → **ne pas pusher ce fichier avec le token dedans**)
- Pour redéployer : zipper `bot_discord.py` + `discloud.config` + `requirements.txt` et uploader sur discloud.app

### Sécurité tokens

- Le repo GitHub **Kikaby67/Pointupjt** est **public** → pas de `GITHUB_TOKEN` nécessaire
- Le `DISCORD_TOKEN` est dans le code, **ne jamais pusher `bot_discord.py` sur GitHub**

## Tech Stack

- **C# / .NET 10.0** — game logic
- **Streamer.bot** — Twitch bot platform that executes C# scripts on chat commands
- **Python / discord.py** — Discord bot
- **Discloud** — hébergement du bot Discord (plan gratuit)
- **JSON** — player data persistence
- **Twitch + OBS** — streaming integration

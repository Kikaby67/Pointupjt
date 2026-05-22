# CLAUDE.md
> Guide technique complet pour Claude Code (VS Code / claude.ai/code)
> Projet Twitch bot game — Streamer.bot + C# inline

---

## Vue d'ensemble du projet

**Pointu-PJT** est un mini-jeu RPG textuel dans le chat Twitch.
Les viewers tapent des commandes (`!rejoindre`, `!quete`, `!attaquer`...).
Chaque joueur a un fichier JSON sur le disque local. Pas de base de données.

**Machine** : Windows 11, AMD Ryzen 5 5600X, 32 Go RAM
**Stack** : Streamer.bot (C# inline), JSON fichiers plats, .NET 10.0 (prototypage)
**Repo GitHub** : https://github.com/Kikaby67/Pointupjt (public)

---

## Structure des dossiers

```
Pointu-PJT/
├── Apprentissage/Pointu-pjt/   # .NET 10.0 console app (prototypage C# uniquement)
├── Streamerbot/                 # Fichiers C# collés dans Streamer.bot
│   ├── Bonjour/
│   ├── Rejoindre/
│   └── ...
├── Donnees/
│   ├── joueurs/                 # Un .json par joueur (ex: kikabygaming.json)
│   └── etat_global.json         # Rencontre manuelle active (streamer)
├── Lore/
│   ├── LA_LEGENDE_DE_POINTU_V2.md
│   ├── FICHES_CLASSES.md
│   └── BESTIAIRE.md
└── DiscordBot/
    ├── bot_discord.py           # Bot Python discord.py (Discloud)
    └── discloud.config
```

---

## Build & Run (prototypage)

```bash
cd Apprentissage/Pointu-pjt
dotnet build
dotnet run
```

> Les fichiers `Streamerbot/` ne sont PAS compilés comme projet .NET.
> Ils sont collés directement dans Streamer.bot → Execute C# Code.

---

## Conventions de code CRITIQUES — Streamer.bot

### Règles absolues
- Chaque fichier est **autonome** — pas de partage de méthodes entre fichiers
- `using System;` et `using System.IO;` en tête de chaque fichier
- Classe toujours `CPHInline`, méthode `Execute()` retourne `bool`
- `args["user"]` pour le pseudo viewer (JAMAIS `args["nomJoueur"]`)
- `CPH.SendMessage(string)` pour envoyer dans le chat
- `CPH.Wait(int ms)` pour attendre
- `CPH.LogWarn(string)` pour les logs
- **Jamais** `Newtonsoft.Json` ni `System.Text.Json` → parser manuel uniquement
- Chemins avec `@"..."` pour éviter les doubles backslashes

### Les 3 méthodes utilitaires (copier dans CHAQUE fichier)

```csharp
// Lit une valeur dans le JSON — retourne "0" si clé introuvable
private string LireValeur(string json, string cle)
{
    string marqueur = "\"" + cle + "\": ";
    int posDebut    = json.IndexOf(marqueur);
    if (posDebut == -1) return "0";
    posDebut       += marqueur.Length;
    int posFin      = json.IndexOfAny(new char[] { ',', '\n', '}' }, posDebut);
    return json.Substring(posDebut, posFin - posDebut).Trim().Trim('"');
}

// Remplace une valeur dans le JSON
// estTexte = true  → "Firewaller" (avec guillemets)
// estTexte = false → 10 / true / false (sans guillemets)
private string ModifierValeur(string json, string cle, string val, bool estTexte)
{
    string marqueur = "\"" + cle + "\": ";
    int posDebut    = json.IndexOf(marqueur);
    if (posDebut == -1) return json;
    posDebut       += marqueur.Length;
    int posFin      = json.IndexOfAny(new char[] { ',', '\n', '}' }, posDebut);
    string ancienne = json.Substring(posDebut, posFin - posDebut);
    string nouvelle = estTexte ? "\"" + val + "\"" : val;
    return json.Substring(0, posDebut) + nouvelle + json.Substring(posDebut + ancienne.Length);
}

// Lit un entier et ajoute un montant
private string AjouterValeur(string json, string cle, int montant)
{
    int val = int.Parse(LireValeur(json, cle));
    return ModifierValeur(json, cle, (val + montant).ToString(), false);
}
```

### Quand utiliser estTexte
```
estTexte = false → nombres (ram, pvMax, experience...)
                 → booléens (true, false)
estTexte = true  → chaînes de texte (classe, queteId, ennemiNom...)
```

---

## JSON joueur — structure complète

Fichier : `Donnees/joueurs/{nomJoueur.ToLower()}.json`

```json
{
  "nomJoueur": "kikabygaming",
  "ram": 10,
  "niveau": 1,
  "experience": 0,
  "classeChoisie": false,
  "classe": "",
  "sousClasseChoisie": false,
  "sousClasse": "",
  "typeArme": "",
  "pvMax": 0,
  "pvActuels": 0,
  "classeArmure": 0,
  "bonusAttaque": 0,
  "manaMax": 0,
  "manaActuels": 0,
  "charisme": 0,
  "enCombat": false,
  "enQuete": false,
  "queteId": "",
  "queteTicksRestants": 0,
  "queteDernierTick": 0,
  "combatActuel": {
    "ennemiNom": "",
    "ennemiPVActuels": 0,
    "buffActif": false,
    "protectionActive": false,
    "tourCombat": 0
  },
  "inventaire": [],
  "statistiques": {
    "combatsGagnes": 0,
    "combatsPerdus": 0,
    "quetesTerminees": 0
  }
}
```

> ⚠️ L'ancien format (`xp`, `sac`) est obsolète. Le champ XP s'appelle `experience`.

---

## `etat_global.json`

```json
{
  "rencontreActive": false,
  "ennemiNom": "",
  "ennemiPVBase": 0
}
```

Utilisé uniquement pour les rencontres manuelles lancées par le streamer.
Les rencontres de quête sont dans le JSON joueur (`combatActuel`).

---

## Les 5 classes

| Classe | PV | CA | Mana | Charisme | Arme | Dé dégâts |
|--------|----|----|------|---------|------|-----------|
| Hexadécimeur | 25 | 14 | 5 | 8 | Épée | 1d8 |
| Cryptolame | 16 | 13 | 5 | 11 | Dual-Dagues | 1d6+1d6 |
| Hackmancien | 14 | 10 | 30 | 10 | Bâton-Magique | 1d10 |
| Firewaller | 22 | 15 | 25 | 13 | Marteau-Rune | 1d8 |
| Algorythmien | 16 | 11 | 20 | 16 | Luth-Code | 1d6 |

**Jets de création** :
- PV final = pvBase + `rng.Next(0, 4)` (0 à +3)
- CA finale = caBase + `rng.Next(0, 4)` (0 à +3)
- bonusAttaque = `rng.Next(0, 4)` (0 à +3)

```csharp
// Bases de classe pour les Channel Point rewards
private int[] GetClasseBase(string classe)
{
    // { pvBase, caBase, manaBase, charismeBase }
    switch (classe)
    {
        case "Hexadécimeur": return new int[] { 25, 14,  5,  8 };
        case "Cryptolame":   return new int[] { 16, 13,  5, 11 };
        case "Hackmancien":  return new int[] { 14, 10, 30, 10 };
        case "Firewaller":   return new int[] { 22, 15, 25, 13 };
        case "Algorythmien": return new int[] { 16, 11, 20, 16 };
        default:             return new int[] { 10, 10,  0,  0 };
    }
}
```

---

## Sous-classes (niveau 5) — À implémenter

| Classe | Sous-classe A | Sous-classe B |
|--------|--------------|--------------|
| Hexadécimeur | **Colosse** : +8 PV, Protection alliée | **Berserk-Octet** : 2 attaques, -2 CA |
| Cryptolame | **Lame-Fantôme** : 3 attaques, critique+ | **Arc-Traqueur** : 1d10, typeArme="Arc" |
| Hackmancien | **Archimage-Null** : 1d12, zone | **Tisserand** : buff UN allié +2 attaque |
| Firewaller | **Bouclier-Sacré** : aura -1 dégât allié | **Serment-Binaire** : Smite +1d8 |
| Algorythmien | **Virtuose** : 1d8 + buff TOUS | **Guérisseur-Fréquence** : 1d8+3, !revive |

---

## Tableau de niveaux

| Niveau | XP requis | Bonus |
|--------|-----------|-------|
| 1 | 0 | — |
| 2 | 300 | +3 PV max |
| 3 | 900 | +1 CA, +1 PV max |
| 4 | 2 700 | +3 PV max |
| 5 | 6 500 | Sous-classe débloquée |
| 6 | 14 000 | +3 PV max |
| 7 | 23 000 | +1 CA, +1 PV max |
| 8 | 34 000 | +100 Ram |
| 9 | 48 000 | +3 PV max |
| 10 | 64 000 | +2 Charisme |

```csharp
private static readonly int[] XP_SEUILS =
    { 0, 0, 300, 900, 2700, 6500, 14000, 23000, 34000, 48000, 64000 };

private int CalculerNiveau(int xp)
{
    for (int i = XP_SEUILS.Length - 1; i >= 1; i--)
        if (xp >= XP_SEUILS[i]) return i;
    return 1;
}

private string AppliquerBonusNiveau(string json, int niveau)
{
    switch (niveau)
    {
        case 2: case 4: case 6: case 9:
            json = AjouterValeur(json, "pvMax",     3);
            json = AjouterValeur(json, "pvActuels", 3); break;
        case 3: case 7:
            json = AjouterValeur(json, "classeArmure", 1);
            json = AjouterValeur(json, "pvMax",        1);
            json = AjouterValeur(json, "pvActuels",    1); break;
        case 8:  json = AjouterValeur(json, "ram",      100); break;
        case 10: json = AjouterValeur(json, "charisme", 2);   break;
    }
    return json;
}
```

---

## Ennemis

```csharp
// { pvMax, classeArmure, desDegats, xpRecompense, ramRecompense }
private int[] GetEnnemi(string nom)
{
    switch (nom)
    {
        case "insecte-bug":    return new int[] {  5,  8, 4, 10,  2 };
        case "corbeau-daemon": return new int[] { 10, 14, 6, 25,  5 };
        case "castor-rootkit": return new int[] { 15, 16, 6, 40,  8 };
        case "loup-firewall":  return new int[] { 18, 15, 8, 60, 12 };
        default:               return new int[] {  5,  8, 4,  5,  1 };
    }
}

private string NomAffiche(string nom)
{
    switch (nom)
    {
        case "insecte-bug":    return "🦟 Insecte-Bug";
        case "corbeau-daemon": return "🦅 Corbeau-Daemon";
        case "castor-rootkit": return "🦫 Castor-Rootkit";
        case "loup-firewall":  return "🐺 Loup-Firewall";
        default:               return nom;
    }
}
```

---

## Commandes implémentées ✅

### `!bonjour` → `Commande_Bonjour.cs`
Accueil du viewer. Indique `!rejoindre`.
```
Trigger : Command Triggered → !bonjour
```

### `!rejoindre` → `Commande_Rejoindre.cs`
Crée le JSON joueur avec tous les champs. Indique `!choisirclasse`.
```
Trigger : Command Triggered → !rejoindre
```

### `!profil` → `Commande_Profil.cs`
Affiche les stats en 4 messages :
1. Nom | Niv | XP | Ram
2. Classe | Sous-classe | Arme (ou invite à choisir)
3. PV/pvMax | CA | Atq | Mana (si > 0) | Charisme (si > 0)
4. Combats V/D | Quêtes | 🔴 En combat / 🟡 En quête si actif
```
Trigger : Command Triggered → !profil
```

### `!choisirclasse [nom]` → `Commande_ChoisirClasse.cs`
Initialise les stats + jets de dés. Bloque si classe déjà choisie.
`args["rawInput"]` contient le nom de la classe en minuscules.
```
Trigger : Command Triggered → !choisirclasse
```

### `!quete` → `Commande_Quete.cs`
Version CPH.Wait() :
- Quête aléatoire parmi 13 (artefact_01-05, service_01-05, entretien_01-03)
- `CPH.Wait(int.Parse(data[1]) * 300000)` — durée proportionnelle
- Relit le fichier après attente
- 80% succès (+XP +Ram) / 20% échec
- Met `enQuete = false` à la fin

**Quêtes** (`GetQueteData`) — Format `{ description, ticks, xp, ram }` :
```
artefact_01 : 6 ticks → 100 XP · 10 Ram
artefact_02 : 5 ticks → 80 XP  · 8 Ram
artefact_03 : 3 ticks → 50 XP  · 5 Ram
artefact_04 : 2 ticks → 30 XP  · 3 Ram
artefact_05 : 1 tick  → 10 XP  · 1 Ram
service_01  : 3 ticks → 50 XP  · 5 Ram
service_02  : 4 ticks → 70 XP  · 7 Ram
service_03  : 5 ticks → 90 XP  · 9 Ram
service_04  : 6 ticks → 120 XP · 12 Ram
service_05  : 2 ticks → 40 XP  · 4 Ram
entretien_01: 3 ticks → 50 XP  · 5 Ram
entretien_02: 4 ticks → 70 XP  · 7 Ram
entretien_03: 5 ticks → 90 XP  · 9 Ram
```
```
Trigger : Command Triggered → !quete
```

### `!attaquer` → `Commande_Attaquer.cs`
Combat D&D5 simplifié, tour par tour :
1. Si `enCombat = false` → initialise depuis `etat_global.json`
2. Si `enCombat = true` → continue depuis `combatActuel`
3. Joueur attaque : d20 + bonusAttaque vs CA ennemi
4. Si touché → dégâts selon classe, ennemiPV -= dégâts
5. ennemiPV ≤ 0 → victoire (+XP +Ram +combatsGagnes, reset, check niveau)
6. Ennemi riposte : d20 vs CA joueur (+3 si protectionActive)
7. pvActuels ≤ 0 → défaite (pvActuels = 1, reset, si enQuete → quête perdue)

```csharp
// Dégâts joueur par classe
private int CalculerDegatsJoueur(string classe, Random rng)
{
    switch (classe)
    {
        case "Hexadécimeur": return rng.Next(1, 9);
        case "Cryptolame":   return rng.Next(1, 7) + rng.Next(1, 7); // dual
        case "Hackmancien":  return rng.Next(1, 11);
        case "Firewaller":   return rng.Next(1, 9);
        case "Algorythmien": return rng.Next(1, 7);
        default:             return rng.Next(1, 7);
    }
}
```
```
Trigger : Command Triggered → !attaquer
```

### Channel Point Rewards

| Fichier | Reward Twitch | Coût | Logique |
|---------|--------------|------|---------|
| `RewardChaine_JetDe_PV.cs` | 🎲 Jet de dé — PV | 2 000 | pvMax = baseClasse[PV] + 1d6 |
| `RewardChaine_JetDe_CA.cs` | 🎲 Jet de dé — CA | 2 000 | CA = baseClasse[CA] + 1d6 |
| `RewardChaine_Boost_PV.cs` | ⭐ Boost +2 — PV | 20 000 | pvMax += 2 (stack permanent) |
| `RewardChaine_Boost_CA.cs` | ⭐ Boost +2 — CA | 20 000 | CA += 2 |
| `RewardChaine_Boost_Attaque.cs` | ⭐ Boost +2 — Attaque | 20 000 | bonusAttaque += 2 |

> Message boost : `"Bravo, {nomJoueur} tu as gagné +2 dans {nomStat} !"`
> Jet de dé : repart de la BASE classe, ne stack pas avec lui-même.
> Boost +2 : s'empile sur tout.

```
Trigger : Channel Point Reward (un fichier par reward)
```

### `Timer_XP_Visionnage.cs` — Timer 15 min
Parcourt tous les fichiers joueurs. Pour chaque joueur avec classe : +5 XP.
Vérifie montée de niveau. Message si level up.
```
Trigger : Timed Action → Timer_XP_Visionnage (900s, repeat)
Activé par : Stream Online
Désactivé par : Stream Offline
```

### `Action_Rencontre.cs` — Rencontre manuelle streamer
Le streamer clique depuis Streamer.bot UI.
4 actions séparées (une par ennemi) avec Set Argument → `ennemi = "castor-rootkit"`.
Met à jour `etat_global.json` + annonce dans le chat.
```
Trigger : Manuel (bouton Streamer.bot)
```

---

## Commandes À IMPLÉMENTER ❌

### `!soigner` → `Commande_Soigner.cs`
- Coûte 5 mana (`manaActuels -= 5`)
- Soins par classe :
  - Hexadécimeur / Cryptolame : 1d4
  - Hackmancien / Algorythmien : 1d6
  - Firewaller : 1d8+3
- Bloque si manaActuels < 5

### `!fuir` → `Commande_Fuir.cs`
- d20 ≥ 12 → fuite réussie → reset combat
- Cryptolame : seuil 8 au lieu de 12
- Bloque si pas en combat

### `!proteger` → `Commande_Proteger.cs`
- Met `protectionActive = true` dans `combatActuel`
- Dans `!attaquer` : si protectionActive → CA effective +3, reset après riposte ennemi
- Pas d'attaque le tour où on protège

### `!choisirSousClasse [nom]` → `Commande_ChoisirSousClasse.cs`
- Disponible uniquement si `niveau >= 5` et `sousClasseChoisie = false`
- Applique les bonus de la sous-classe choisie
- Met `sousClasseChoisie = true`

---

## Checklist pour écrire un nouveau fichier

1. `using System;` + `using System.IO;` en tête
2. `private const string DOSSIER_JOUEURS = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\joueurs";`
3. Vérifier `File.Exists(cheminFichier)` en premier → message + `return true`
4. Vérifier `classeChoisie == "true"` avant toute action de jeu
5. Vérifier `enCombat` et `enQuete` selon la logique de la commande
6. Toujours `File.WriteAllText(cheminFichier, json)` après modification
7. Après modification de `experience` → vérifier `CalculerNiveau` + `AppliquerBonusNiveau`
8. Copier les 3 méthodes utilitaires en bas du fichier
9. Utiliser `nomJoueur.ToLower()` pour le nom du fichier

---

## Discord Bot

Bot Python `discord.py` déployé sur **Discloud** (plan gratuit).
Lit les profils depuis le repo GitHub public (pas de token nécessaire).

**Commandes** : `!profil`, `!arbonet`, `!aide`
**Channel** : `CHANNEL_ID = 1490232175382102016`

> ⚠️ `DISCORD_TOKEN` hardcodé dans `bot_discord.py` — **ne jamais pusher ce fichier sur GitHub**

**Déploiement** : zipper `bot_discord.py` + `discloud.config` + `requirements.txt` → uploader sur discloud.app

---

## Lore (résumé)

- **Arbonet** : monde nature + technologie hybride (chênes-serveurs, créatures cyber)
- **Pointu** : tortue ancienne, gardienne de l'Antre, NPC principal
- **Hector-Pierre Castor** : antagoniste, détruit les chênes-serveurs
- **Ram** : monnaie du jeu (mémoire vive d'Arbonet)
- **Fragment de Carapace** : artefact qui sauvegarde le profil joueur (lore du JSON)

---

*Projet Pointu © Florian alias kikaby67 — 2026*

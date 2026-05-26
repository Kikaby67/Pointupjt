# CLAUDE.md
> Guide technique complet pour Claude Code (VS Code / claude.ai/code)
> Projet Twitch bot game — Streamer.bot + C# inline

---

## Vue d'ensemble du projet

**Pointu-PJT** est un mini-jeu RPG textuel dans le chat Twitch.
Les viewers tapent des commandes (`!rejoindre`, `!quete`, `!attaque`...).
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
│   ├── Commandes/
│   │   ├── Profil/
│   │   ├── Choisirclasse/
│   │   ├── ChoisirSousClasse/
│   │   ├── InfoClasses/         # !hexadécimeur, !cryptolame, etc.
│   │   ├── Repos/               # !repos
│   │   └── Arbonet/             # !arbonet
│   ├── quetes/
│   │   ├── quest_system.cs      # !quete (lancer / consulter)
│   │   └── quest_timer.cs       # Timer QuestCheck (30s, auto-résolution + rencontres)
│   ├── combat/
│   │   ├── combat_attaque.cs    # !attaque
│   │   ├── combat_soin.cs       # !soin
│   │   ├── combat_defense.cs    # !defense
│   │   └── combat_fuir.cs       # !fuir
│   ├── Timer_Xp/
│   │   └── Timer_XP_visionnage.cs
│   └── Reward/
│       ├── Jet de dé/           # 1d6_PV.cs, 1d4_CA.cs
│       └── Bonus +2/            # +2_PV.cs, +2_CA.cs, +2_Attaque.cs
├── Donnees/
│   ├── joueurs/                 # Un .json par joueur (ex: kikabygaming.json)
│   ├── config_classes.json      # ★ Source unique : stats classes + sous-classes
│   ├── config_ennemis.json      # ★ Source unique : stats tous les ennemis
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
- `CPH.Wait(int ms)` pour attendre (ne PAS utiliser dans les quêtes — bloque Streamer.bot)
- `CPH.LogWarn(string)` pour les logs
- **Jamais** `Newtonsoft.Json` ni `System.Text.Json` → parser manuel uniquement
- Chemins avec `@"..."` pour éviter les doubles backslashes

### Les 3 méthodes utilitaires (copier dans CHAQUE fichier)

```csharp
private string LireValeur(string json, string cle)
{
    string marqueur = "\"" + cle + "\": ";
    int posDebut    = json.IndexOf(marqueur);
    if (posDebut == -1) return "0";
    posDebut       += marqueur.Length;
    int posFin      = json.IndexOfAny(new char[] { ',', '\n', '}' }, posDebut);
    return json.Substring(posDebut, posFin - posDebut).Trim().Trim('"');
}

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

## Architecture config — Source unique de vérité

Toutes les stats de classe, sous-classe et ennemi vivent dans **deux fichiers JSON**.
Le code ne contient aucun `switch (classe)` ni `if (sousClasse == "X")` pour les valeurs numériques.
Pour rééquilibrer : modifier le JSON uniquement.

### `config_classes.json`

Clés format plat : `"NomClasse_stat": valeur`

**Stats de classe** (utilisées à la création + combat) :
```
_pvBase, _caBase, _manaBase, _charisme, _typeArme
_degatsMax, _nbDes          ← dés de dégâts en combat
_nbAttaques                 ← nombre d'attaques (si > 1 → multi-attaque)
_soinMax, _soinBonus        ← dés de soin (!soin)
```

**Stats de sous-classe** (deux catégories) :

*Bonus de sélection* — appliqués UNE FOIS à la sélection, stockés dans le JSON joueur :
```
_pvMaxBonus   ← ajout permanent à pvMax et pvActuels
_caModif      ← modificateur permanent de classeArmure (peut être négatif)
_typeArme     ← remplacement de l'arme
```

*Comportements combat* — lus à l'exécution, affecter le config change tous les joueurs actifs :
```
_degatsMax, _nbDes, _nbAttaques   ← dégâts et nombre d'attaques
_soinMax, _soinBonus              ← soin (Patch-Mélodique)
_buffAttaque                      ← buff allié (Compilateur)
_auraDefense                      ← aura défensive (Protocole-Sacré)
```

**Pattern de lookup (sous-classe prioritaire sur classe) :**
```csharp
string key = sousClasse != "" && LireValeur(cfg, sousClasse + "_degatsMax") != "0"
             ? sousClasse : classe;
```

> ⚠️ `Pointeur-Null_nbAttaques: 1` est explicite pour éviter l'héritage de `Cryptolame_nbAttaques: 2`.

### `config_ennemis.json`

Clés format plat : `"NomEnnemi_stat": valeur`
```
_pv, _ca, _degatsMax, _xp, _ram
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
  "enRencontre": false,
  "rencontreType": "",
  "quetePauseDebut": 0,
  "queteTotalPause": 0,
  "queteCooldownFin": 0,
  "dernierCheckRencontre": 0,
  "reposCooldownFin": 0,
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
>
> Les champs combat (`ennemiNom`, `ennemiPVActuels`, `buffActif`, `tourCombat`) sont dans `combatActuel` (objet imbriqué). `LireValeur` les trouve par recherche de chaîne.
>
> `reposCooldownFin` : absent des anciens fichiers joueurs, à ajouter manuellement (valeur 0).

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

---

## Les 5 classes

| Classe | PV | CA | Mana | Charisme | Arme | Dégâts | Soin |
|--------|----|----|------|---------|------|--------|------|
| Hexadécimeur | 25 | 14 | 5 | 8 | Épée | 1d8 | 1d4 |
| Cryptolame | 16 | 13 | 5 | 11 | Dual-Dagues | 1d6 ×2 att. | 1d4 |
| Hackmancien | 14 | 10 | 30 | 10 | Bâton-Magique | 1d12 | 1d6 |
| Firewaller | 22 | 15 | 25 | 13 | Marteau-Rune | 1d8 | 1d8+3 |
| Algorythmancien | 16 | 11 | 20 | 16 | Luth-Code | 1d8 | 1d6 |

> ⚠️ Le nom exact en code est `"Algorythmancien"` (pas `"Algorythmien"`).

Toutes ces valeurs sont dans `config_classes.json`. Le code ne les duplique pas.

**Jets de création** :
- PV final = pvBase + `rng.Next(1, 7)` (1d6 → +1 à +6)
- CA finale = caBase + `rng.Next(1, 5)` (1d4 → +1 à +4)
- bonusAttaque = `rng.Next(1, 5)` (1d4 → +1 à +4)

---

## Sous-classes (niveau 5) ✅

| Classe | Sous-classe A | Sous-classe B |
|--------|--------------|--------------|
| Hexadécimeur | **Bloc-Hex** : +8 PV max | **Surcharge** : 2 attaques 1d8, -2 CA |
| Cryptolame | **Byte-Fantôme** : 3 attaques 1d6 | **Pointeur-Null** : 1d10, Arc |
| Hackmancien | **Faille-Zéro** : 2d8 | **Compilateur** : buff allié +2 attaque |
| Firewaller | **Protocole-Sacré** : aura défense | **Serment-Binaire** : Smite +1d8 (2d8 total) |
| Algorythmancien | **Barde-Binaire** : 1d10 + buff TOUS | **Patch-Mélodique** : soin 1d8+3 |

Toutes les valeurs sont dans `config_classes.json` (voir section Architecture config).

Les **bonus de sélection** (pvMaxBonus, caModif, typeArme) sont appliqués une fois et stockés dans le JSON joueur — changer le config n'affecte pas les joueurs existants.

Les **comportements combat** (degatsMax, nbAttaques, soinMax…) sont lus à l'exécution — changer le config affecte immédiatement tous les joueurs actifs.

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

Toutes les stats ennemis sont dans `config_ennemis.json`. Les méthodes de lookup lisent le fichier :

```csharp
private int[] GetEnnemiStats(string nom)
{
    string cfg    = File.ReadAllText(CONFIG_ENNEMIS);
    int ca        = int.Parse(LireValeur(cfg, nom + "_ca"));
    int degatsMax = int.Parse(LireValeur(cfg, nom + "_degatsMax"));
    return new int[] { ca != 0 ? ca : 12, degatsMax != 0 ? degatsMax : 6 };
}

private int[] GetRecompensesEnnemi(string nom)   // combat_attaque.cs uniquement
{
    string cfg = File.ReadAllText(CONFIG_ENNEMIS);
    int xp  = int.Parse(LireValeur(cfg, nom + "_xp"));
    int ram = int.Parse(LireValeur(cfg, nom + "_ram"));
    return new int[] { xp != 0 ? xp : 15, ram != 0 ? ram : 3 };
}
```

### Ennemis de rencontre de quête

| Nom | PV | CA | Dégâts | XP | RAM |
|-----|----|----|--------|-----|-----|
| Martre-Trojan | 30 | 12 | 1d6 | 20 | 4 |
| Sentinelle du Castor | 25 | 14 | 1d6 | 30 | 6 |
| Ombre de la mémoire | 20 | 11 | 1d8 | 25 | 5 |
| Drone-racine | 15 | 10 | 1d4 | 15 | 3 |
| Parasite de données | 18 | 12 | 1d4 | 18 | 4 |
| Sanglier-Crash | 35 | 9 | 1d8 | 22 | 5 |
| Taupe-Malware | 22 | 13 | 1d6 | 20 | 4 |

### Ennemis manuels (streamer)

| Nom | CA | Dégâts | XP | RAM |
|-----|-----|--------|-----|-----|
| Insecte-Bug | 8 | 1d4 | 10 | 2 |
| Corbeau-Daemon | 14 | 1d6 | 25 | 5 |
| Castor-Rootkit | 16 | 1d6 | 40 | 8 |
| Loup-Firewall | 15 | 1d8 | 60 | 12 |

---

## Commandes implémentées ✅

### `!bonjour` → `Commande_Bonjour.cs`
Accueil du viewer. Indique `!rejoindre`.
```
Trigger : Command Triggered → !bonjour
```

### `!rejoindre` → `Commande_Rejoindre.cs`
Crée le JSON joueur avec tous les champs (y compris `reposCooldownFin`). Indique `!choisirclasse`.
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
Lit les stats depuis `config_classes.json`. Jets de dés à la création.
`args["rawInput"]` contient le nom de la classe en minuscules.
```
Trigger : Command Triggered → !choisirclasse
```

### `!sousclasse [nom]` → `commande_sousclasse.cs`
Disponible si `niveau >= 5` et `sousClasseChoisie = false`.
Lit les bonus depuis `config_classes.json` : `_pvMaxBonus`, `_caModif`, `_typeArme`.
Met `sousClasseChoisie = true`.
```
Trigger : Command Triggered → !sousclasse
```

### `!repos` → `Commandes/Repos/commande_repos.cs`
Restauration complète hors combat + hors quête.
- Restaure `pvActuels = pvMax` et `manaActuels = manaMax`
- Cooldown 30 minutes via `reposCooldownFin` (timestamp Unix)
```
Trigger : Command Triggered → !repos
```

### `!arbonet` → `Commandes/Arbonet/commande_arbonet.cs`
Affiche le synopsis du lore d'Arbonet en 4 messages. Termine par `!rejoindre`.
```
Trigger : Command Triggered → !arbonet
```

### `!quete` → `quetes/quest_system.cs`
Système basé sur timestamps Unix — **pas de `CPH.Wait()`**.

**Flux :**
1. Vérifie `queteCooldownFin` → si en cooldown, annonce les minutes restantes
2. Vérifie `classeChoisie`, `enCombat`
3. Si `enQuete == true` :
   - Si `enRencontre == true` → "tu es en pleine rencontre !"
   - Sinon : calcule `secondesEcoulees = (maintenant - queteDernierTick) - queteTotalPause`, si terminé → résoudre (80% succès)
4. Sinon : choisit une quête aléatoire, initialise tous les champs, `CPH.EnableTimer("QuestCheck")`

**Quêtes** — Format `{ description, ticks, xp, ram }` :
```
artefact_01 : 6 ticks (30 min) → 100 XP · 10 Ram
artefact_02 : 5 ticks (25 min) → 80 XP  · 8 Ram
artefact_03 : 3 ticks (15 min) → 50 XP  · 5 Ram
artefact_04 : 2 ticks (10 min) → 30 XP  · 3 Ram
artefact_05 : 1 tick  (5 min)  → 10 XP  · 1 Ram
service_01  : 3 ticks (15 min) → 50 XP  · 5 Ram
service_02  : 4 ticks (20 min) → 70 XP  · 7 Ram
service_03  : 5 ticks (25 min) → 90 XP  · 9 Ram
service_04  : 6 ticks (30 min) → 120 XP · 12 Ram
service_05  : 2 ticks (10 min) → 40 XP  · 4 Ram
entretien_01: 3 ticks (15 min) → 50 XP  · 5 Ram
entretien_02: 4 ticks (20 min) → 70 XP  · 7 Ram
entretien_03: 5 ticks (25 min) → 90 XP  · 9 Ram
```
> 1 tick = 5 minutes réelles
```
Trigger : Command Triggered → !quete
```

### `quest_timer.cs` — Timer QuestCheck (30s)
Parcourt tous les fichiers joueurs. Pour chaque joueur `enQuete == true` :

**CAS 1** — `enRencontre == true` et `enCombat == false` :
- `pvActuels > 0` → victoire : reprend la quête, `quetesTerminees += 1`
- `pvActuels <= 0` → défaite : `enQuete = false`, `queteCooldownFin = maintenant + 1200` (20 min)

**CAS 2** — Check rencontre toutes les 3 min :
- 40% chance, parmi 3 types (Combat / Événement / Marchand)
- PV ennemi lu depuis `config_ennemis.json`

**CAS 3** — Fin de quête : 80% succès / 20% échec
```
Trigger : Timed Action → QuestCheck (30s, repeat)
```

### `!attaque` → `combat/combat_attaque.cs`
Combat tour par tour — **seulement si `enCombat == true`**.

**Logique d'attaque entièrement config-driven :**
1. Lit `_nbAttaques` depuis config (sous-classe en priorité, puis classe, défaut 1)
2. Si `nbAttaques > 1` : loop multi-attaque, chaque coup appelle `RollDegats`
3. Sinon : attaque unique avec `RollDegats`
4. `RollDegats` lit `_degatsMax` et `_nbDes` depuis config (sous-classe si elle a une entrée, sinon classe)

```csharp
private int RollDegats(string classe, string sousClasse, Random rng)
{
    string cfg = File.ReadAllText(CONFIG_CLASSES);
    string key = sousClasse != "" && LireValeur(cfg, sousClasse + "_degatsMax") != "0"
                 ? sousClasse : classe;
    int degatsMax = int.Parse(LireValeur(cfg, key + "_degatsMax"));
    int nbDes     = int.Parse(LireValeur(cfg, key + "_nbDes"));
    if (degatsMax == 0) degatsMax = 8;
    if (nbDes     == 0) nbDes     = 1;
    int total = 0;
    for (int i = 0; i < nbDes; i++) total += rng.Next(1, degatsMax + 1);
    return total;
}
```

- `buffActif` ajoute +2 au bonusAttaque ce tour
- Victoire : `combatsGagnes += 1`, +XP +RAM depuis config_ennemis
- Défaite : `combatsPerdus += 1`
```
Trigger : Command Triggered → !attaque
```

### `!soin` → `combat/combat_soin.cs`
Soin pendant le combat — coûte 5 mana.
- Si `manaActuels < 5` : soin échoue mais l'ennemi riposte quand même
- `RollSoin` lit `_soinMax` et `_soinBonus` depuis config (sous-classe si elle en a, sinon classe)
- Soin plafonné à `pvMax`
- L'ennemi riposte toujours après

```csharp
private int RollSoin(string classe, string sousClasse, Random rng)
{
    string cfg    = File.ReadAllText(CONFIG_CLASSES);
    string key    = (sousClasse != "" && LireValeur(cfg, sousClasse + "_soinMax") != "0") ? sousClasse : classe;
    int soinMax   = int.Parse(LireValeur(cfg, key + "_soinMax"));
    int soinBonus = int.Parse(LireValeur(cfg, key + "_soinBonus"));
    if (soinMax == 0) soinMax = 4;
    return rng.Next(1, soinMax + 1) + soinBonus;
}
```
```
Trigger : Command Triggered → !soin
```

### `!defense` → `combat/combat_defense.cs`
Posture défensive — pas d'attaque joueur ce tour.
- CA temporaire = `classeArmure + 3` pour ce tour uniquement
- L'ennemi attaque contre cette CA augmentée
```
Trigger : Command Triggered → !defense
```

### `!fuir` → `combat/combat_fuir.cs`
Tentative de fuite du combat.
- Seuil : **Cryptolame** d20 ≥ 8 · **Autres** d20 ≥ 12
- **Fuite réussie** : `enCombat = false` · si `enRencontre == true` → quête reprend
- **Fuite échouée** : l'ennemi riposte · si `pvActuels <= 0` → `combatsPerdus += 1`
```
Trigger : Command Triggered → !fuir
```

### Channel Point Rewards

| Fichier | Reward Twitch | Coût | Logique |
|---------|--------------|------|---------|
| `1d6_PV.cs` | 🎲 Jet de dé — PV | 2 000 | pvMax = pvBase (config) + 1d6 |
| `1d4_CA.cs` | 🎲 Jet de dé — CA | 2 000 | CA = caBase (config) + 1d4 |
| `+2_PV.cs` | ⭐ Boost +2 — PV | 20 000 | pvMax += 2 (stack permanent) |
| `+2_CA.cs` | ⭐ Boost +2 — CA | 20 000 | CA += 2 |
| `+2_Attaque.cs` | ⭐ Boost +2 — Attaque | 20 000 | bonusAttaque += 2 |

> Jet de dé : repart de la BASE classe depuis config, ne stack pas.
> Boost +2 : s'empile sur tout.
```
Trigger : Channel Point Reward (un fichier par reward)
```

### `Timer_XP_Visionnage.cs` — Timer 15 min
Pour chaque joueur avec classe : +5 XP, vérifie montée de niveau.
Régénération passive si `enCombat != true` : +2 PV (plafonné pvMax) + +3 Mana (plafonné manaMax).
```
Trigger : Timed Action → Timer_XP_Visionnage (900s, repeat)
```

### `Action_Rencontre.cs` — Rencontre manuelle streamer
4 actions séparées (une par ennemi). Met à jour `etat_global.json` + chat.
```
Trigger : Manuel (bouton Streamer.bot)
```

---

## Checklist pour écrire un nouveau fichier

1. `using System;` + `using System.IO;` en tête
2. Constantes en tête :
   ```csharp
   private const string DOSSIER_JOUEURS = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\joueurs";
   private const string CONFIG_CLASSES  = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\config_classes.json";
   private const string CONFIG_ENNEMIS  = @"C:\Users\Florian\pjt\Pointu-PJT\Donnees\config_ennemis.json";
   ```
3. Vérifier `File.Exists(cheminFichier)` en premier → message + `return true`
4. Vérifier `classeChoisie == "true"` avant toute action de jeu
5. Vérifier `enCombat` et `enQuete` selon la logique
6. **Ne jamais hardcoder de stats de classe/sous-classe/ennemi** → lire depuis config
7. Toujours `File.WriteAllText(cheminFichier, json)` après modification
8. Après modification de `experience` → vérifier `CalculerNiveau` + `AppliquerBonusNiveau`
9. Copier les 3 méthodes utilitaires en bas du fichier
10. Utiliser `nomJoueur.ToLower()` pour le nom du fichier
11. Ne jamais utiliser `CPH.Wait()` dans un flux de quête — bloque Streamer.bot

---

## Discord Bot

Bot Python `discord.py` déployé sur **Discloud** (plan gratuit).
Lit les profils depuis le repo GitHub public.

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

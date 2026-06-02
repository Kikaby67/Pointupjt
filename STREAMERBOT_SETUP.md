# Guide d'intégration Streamer.bot — Pointu-PJT
> Copier chaque fichier CS dans Streamer.bot → Actions → Add Action → Execute C# Code

---

## 1. PRÉREQUIS

- Dossier `Donnees/joueurs/` créé et vide
- Tous les fichiers JSON de config présents dans `Donnees/`
- Streamer.bot version récente (.NET inline C#)

---

## 2. ACTIONS À CRÉER

Pour chaque action : `Add Action` → coller le contenu du fichier `.cs` → `Save`

### COMMANDES JOUEURS (Trigger : Command Triggered)

| Commande | Fichier CS | Notes |
|----------|-----------|-------|
| `!bonjour` | `Streamerbot/Commandes/Bonjour/Commande_bonjour.cs` | |
| `!rejoindre` | `Streamerbot/Commandes/Rejoindre/Commande_rejoindre.cs` | |
| `!profil` | `Streamerbot/Commandes/Profil/commande_!profil.cs` | |
| `!choisirclasse` | `Streamerbot/Commandes/Choisirclasse/commande_choisirclasse.cs` | |
| `!sousclasse` | `Streamerbot/Commandes/ChoisirSousClasse/commande_sousclasse.cs` | Niv. 5+ |
| `!quete` | `Streamerbot/quetes/quest_system.cs` | |
| `!abandon` | `Streamerbot/Commandes/Abandon/commande_abandon.cs` | |
| `!accepter` | `Streamerbot/Commandes/Accepter/commande_accepter.cs` | Offres en quête |
| `!refuser` | `Streamerbot/Commandes/Refuser/commande_refuser.cs` | Offres en quête |
| `!attaque` | `Streamerbot/combat/combat_attaque.cs` | |
| `!soin` | `Streamerbot/combat/combat_soin.cs` | |
| `!defense` | `Streamerbot/combat/combat_defense.cs` | |
| `!fuir` | `Streamerbot/combat/combat_fuir.cs` | |
| `!repos` | `Streamerbot/Commandes/Repos/commande_repos.cs` | |
| `!inventaire` | `Streamerbot/Commandes/Inventaire/commande_inventaire.cs` | |
| `!equiper` | `Streamerbot/Commandes/Equiper/commande_equiper.cs` | |
| `!equipement` | `Streamerbot/Commandes/Equipement/commande_equipement.cs` | |
| `!vendre` | `Streamerbot/Commandes/Vendre/commande_vendre.cs` | |
| `!utiliser` | `Streamerbot/Commandes/Utiliser/commande_utiliser.cs` | |
| `!arbonet` | `Streamerbot/Commandes/Arbonet/commande_arbonet.cs` | |
| `!racine` | `Streamerbot/Commandes/Secret/commande_secret.cs` | Secret — ne pas publier |

### COMMANDES INFO CLASSES (Trigger : Command Triggered)

| Commande | Fichier CS |
|----------|-----------|
| `!hexadecimeur` | `Streamerbot/Commandes/InfoClasses/commande_hexadecimeur.cs` |
| `!cryptolame` | `Streamerbot/Commandes/InfoClasses/commande_cryptolame.cs` |
| `!hackmancien` | `Streamerbot/Commandes/InfoClasses/commande_hackmancien.cs` |
| `!firewaller` | `Streamerbot/Commandes/InfoClasses/commande_firewaller.cs` |
| `!algorythmancien` | `Streamerbot/Commandes/InfoClasses/commande_algorythmancien.cs` |

### COMMANDES BROADCASTER UNIQUEMENT (Trigger : Command Triggered)

| Commande | Fichier CS | Restriction |
|----------|-----------|-------------|
| `!classement` | `Streamerbot/Commandes/Classement/commande_classement.cs` | `broadcaster` dans config_global.json |

### CHANNEL POINT REWARDS (Trigger : Channel Point Reward)

| Reward Twitch | Fichier CS | Coût suggéré |
|--------------|-----------|-------------|
| 🎲 Jet de dé — PV | `Streamerbot/Reward/Jet de dé/1d6_PV.cs` | 2 000 pts |
| 🎲 Jet de dé — CA | `Streamerbot/Reward/Jet de dé/1d4_CA.cs` | 2 000 pts |
| ⭐ Boost +2 PV max | `Streamerbot/Reward/Bonus +2/+2_PV.cs` | 20 000 pts |
| ⭐ Boost +2 CA | `Streamerbot/Reward/Bonus +2/+2_CA.cs` | 20 000 pts |
| ⭐ Boost +2 Attaque | `Streamerbot/Reward/Bonus +2/+2_Attaque.cs` | 20 000 pts |

---

## 3. TIMERS À CRÉER

### Timer QuestCheck
- **Fichier** : `Streamerbot/quetes/quest_timer.cs`
- **Nom dans SB** : `QuestCheck`
- **Intervalle** : 30 secondes
- **Repeat** : ✅ Oui
- **Activé au démarrage** : ❌ Non (activé par `!quete`, désactivé automatiquement)

### Timer XP Visionnage
- **Fichier** : `Streamerbot/Timer_Xp/Timer_XP_visionnage.cs`
- **Intervalle** : 900 secondes (15 min)
- **Repeat** : ✅ Oui
- **Activé au démarrage** : ✅ Oui

---

## 4. ACTIONS MANUELLES STREAMER (Trigger : Manuel / Bouton)

Rencontres manuelles déclenchées par le streamer depuis le tableau de bord Streamer.bot.

> Ces actions modifient `Donnees/etat_global.json` et lancent un combat pour tous les joueurs actifs.

---

## 5. CONFIG À VÉRIFIER AVANT LE LANCEMENT

### `Donnees/config_global.json`
```json
{
  "broadcaster": "TON_PSEUDO_TWITCH_EN_MINUSCULES",
  ...
}
```
⚠️ **Changer `broadcaster`** par ton pseudo Twitch en minuscules.

### `Donnees/joueurs/`
- Dossier doit exister et être vide avant le premier stream
- Les fichiers sont créés automatiquement via `!rejoindre`

---

## 6. ORDRE DE TEST RECOMMANDÉ

1. Taper `!rejoindre` dans le chat
2. Taper `!choisirclasse hexadecimeur`
3. Taper `!profil` → vérifier les stats
4. Taper `!quete` → vérifier qu'une quête se lance
5. Attendre 5 min → vérifier une rencontre ou fin de quête
6. Taper `!attaque` si un combat se déclenche
7. Taper `!repos` après le combat
8. Taper `!classement` (broadcaster) → vérifier l'affichage

---

## 7. FICHIERS DE CONFIG — RÉFÉRENCE RAPIDE

| Fichier | Rôle | Modifier pour... |
|---------|------|-----------------|
| `config_global.json` | Constantes de jeu | Rééquilibrer cooldowns, %, seuils |
| `config_classes.json` | Stats des 5 classes + sous-classes | Modifier une classe |
| `config_ennemis.json` | Stats des ennemis | Modifier un ennemi |
| `config_items.json` | Stats des items | Modifier un item |
| `config_quetes.json` | Quêtes disponibles (format quete001_*) | Ajouter / modifier une quête |
| `config_level.json` | Seuils XP et bonus de niveau | Modifier la progression |
| `config_allies.json` | Paramètres marchands / alliés en quête | Modifier les rencontres |
| `etat_global.json` | Rencontre manuelle active | Géré automatiquement |

---

*Projet Pointu-PJT © Florian alias kikaby67 — 2026*

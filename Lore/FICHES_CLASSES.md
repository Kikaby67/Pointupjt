# Fiches des Classes — Arbonet
### *La Légende de Pointu — Cycle 404*

---

> Les classes d'Arbonet ne sont pas de simples vocations.
> Elles sont des mutations — des façons dont le réseau et la nature se sont fondus dans un être.

---

## ⚔️ Hexadécimeur
**Équivalent D&D : Guerrier | Rôle : DPS corps à corps / Tank**

### Lore
*Forgés dans les couches profondes du réseau, les Hexadécimeurs ont absorbé les protocoles bruts d'Arbonet jusqu'à en faire leur armure. Leur corps est parcouru de séquences hexadécimales gravées dans la chair et le code. Ils ne calculent pas — ils frappent. Et chaque frappe est une instruction.*

### Stats de base
| Stat | Valeur de base | Jet de création |
|------|---------------|-----------------|
| PV | 25 | + 1d6 |
| CA | 14 | + 1d4 (0 à 3) |
| Mana | 5 | fixe |
| Charisme | 8 | fixe |
| Bonus Attaque | 0 | + 1d4 (0 à 3) |
| Arme | Épée | fixe |
| Dé de dégâts | 1d8 | — |

### Actions disponibles
| Commande | Coût | Effet |
|----------|------|-------|
| `!attaquer` | — | d20 + bonus vs CA ennemi → 1d8 dégâts |
| `!soigner` | 5 mana | Récupère 1d4 PV |
| `!proteger` | — | CA +3 ce tour, pas d'attaque |
| `!fuir` | — | d20 ≥ 12 → fuite réussie |

### Progression
| Niveau | XP requis | Gain |
|--------|-----------|------|
| 2 | 100 | +3 PV max |
| 3 | 250 | +3 PV max |
| 4 | 500 | +3 PV max, +1 CA |
| **5** | **900** | **Sous-classe** |
| 6 | 1 400 | +3 PV max |
| 7 | 2 000 | +3 PV max, +1 CA |
| 8 | 2 700 | +3 PV max |
| 9 | 3 500 | +3 PV max, +1 CA |
| 10 | 5 000 | Niveau maximum |

### Sous-classes — Niveau 5
#### 🏔️ Colosse
*La mutation va plus loin. L'Hexadécimeur Colosse est une forteresse ambulante.*
- +8 PV maximum
- +1 CA permanente
- La commande `!proteger` protège également un allié ciblé

#### ⚡ Berserk-Octet
*Le réseau déborde. La rage prend le contrôle.*
- -2 CA permanente
- +1d6 dégâts supplémentaires par attaque
- `!attaquer` déclenche 2 attaques consécutives

---

## 🗡️ Cryptolame
**Équivalent D&D : Voleur | Rôle : DPS / Fuite**

### Lore
*Ni tout à fait dans le réseau, ni tout à fait dans la nature — le Cryptolame existe dans l'interstice. Il chiffre ses mouvements, rend ses intentions illisibles. Ses deux lames sont des clés de déchiffrement : l'une ouvre la défense, l'autre exploite la brèche. Il ne bat pas ses ennemis — il les rend obsolètes.*

### Stats de base
| Stat | Valeur de base | Jet de création |
|------|---------------|-----------------|
| PV | 16 | + 1d6 |
| CA | 13 | + 1d4 (0 à 3) |
| Mana | 5 | fixe |
| Charisme | 11 | fixe |
| Bonus Attaque | 0 | + 1d4 (0 à 3) |
| Arme | Dual-Dagues | fixe |
| Dé de dégâts | 1d6 + 1d6 | 2 attaques par tour |

### Actions disponibles
| Commande | Coût | Effet |
|----------|------|-------|
| `!attaquer` | — | 2 jets d20 → 1d6 + 1d6 dégâts |
| `!soigner` | 5 mana | Récupère 1d4 PV |
| `!fuir` | — | d20 ≥ **8** → fuite réussie (facilité) |
| `!proteger` | — | CA +3 ce tour, pas d'attaque |

> *Le Cryptolame fuit plus facilement que les autres classes — seuil abaissé à 8.*

### Progression
*(Même tableau que l'Hexadécimeur)*

### Sous-classes — Niveau 5
#### 👻 Lame-Fantôme
*Les deux lames ne font plus qu'une ombre.*
- Une 3ème attaque sournoise s'ajoute au tour (1d6)
- Critique automatique si d20 = 20 (dégâts doublés)

#### 🏹 Arc-Traqueur
*Le Cryptolame prend du recul. Et de la distance.*
- Arme passe de Dual-Dagues à Arc
- Dé de dégâts : 1d10 (une seule attaque par tour)
- Bonus de +2 aux récompenses découvertes en quête

---

## ⚡ Hackmancien
**Équivalent D&D : Mage | Rôle : DPS magie / Buff ciblé**

### Lore
*Là où d'autres voient un chêne-serveur, le Hackmancien voit une faille. Son art est une nécromancie inversée : au lieu de ressusciter les morts, il injecte de la corruption ciblée dans les vivants. Ses sorts ne sont pas des incantations — ce sont des commits mal formés, envoyés directement dans le système nerveux de ses cibles.*

### Stats de base
| Stat | Valeur de base | Jet de création |
|------|---------------|-----------------|
| PV | 14 | + 1d6 |
| CA | 10 | + 1d4 (0 à 3) |
| Mana | 30 | fixe |
| Charisme | 10 | fixe |
| Bonus Attaque | 0 | + 1d4 (0 à 3) |
| Arme | Bâton-Magique | fixe |
| Dé de dégâts | 1d10 | magique |

### Actions disponibles
| Commande | Coût | Effet |
|----------|------|-------|
| `!attaquer` | — | d20 vs CA ennemi → 1d10 dégâts magiques |
| `!soigner` | 5 mana | Récupère 1d6 PV |
| `!buff @allié` | 5 mana | Cible UN allié → +2 bonus attaque (3 tours) |
| `!fuir` | — | d20 ≥ 12 → fuite réussie |
| `!proteger` | — | CA +3 ce tour, pas d'attaque |

### Sous-classes — Niveau 5
#### 💀 Archimage-Null
*La faille devient un gouffre.*
- Dé de dégâts passe à 1d12
- Déblocage d'un sort de zone : toutes les cibles en combat subissent 1d6 dégâts

#### 🕸️ Tisserand-Collectif
*Le code se tisse entre les alliés.*
- `!buff` peut cibler jusqu'à 2 alliés simultanément
- Buff passe à +3 bonus attaque (au lieu de +2)
- Mana max +10

---

## 🛡️ Firewaller
**Équivalent D&D : Paladin | Rôle : Tank / Heal**

### Lore
*Avant les Firewallers, les zones corrompues ne pouvaient être traversées. Ces gardiens sont des murs vivants — leur corps filtre la corruption comme un pare-feu filtre les paquets suspects. Là où ils se tiennent, rien ne passe sans leur accord. Et quand ils soignent, ce n'est pas de la magie — c'est une restauration de système, ligne par ligne.*

### Stats de base
| Stat | Valeur de base | Jet de création |
|------|---------------|-----------------|
| PV | 22 | + 1d6 |
| CA | 15 | + 1d4 (0 à 3) |
| Mana | 25 | fixe |
| Charisme | 13 | fixe |
| Bonus Attaque | 0 | + 1d4 (0 à 3) |
| Arme | Marteau-Rune | fixe |
| Dé de dégâts | 1d8 | — |

### Actions disponibles
| Commande | Coût | Effet |
|----------|------|-------|
| `!attaquer` | — | d20 vs CA ennemi → 1d8 dégâts |
| `!soigner` | 5 mana | Récupère **1d8+3 PV** |
| `!discuter` | — | Charisme 13 → d20 + bonus vs seuil 14 |
| `!proteger` | — | CA +3 ce tour, pas d'attaque |
| `!fuir` | — | d20 ≥ 12 → fuite réussie |

> *Le Firewaller a le soin le plus puissant avec l'Éclaireur.*

### Sous-classes — Niveau 5
#### 🔰 Bouclier-Sacré
*La protection devient une aura.*
- +2 CA permanente
- Aura passive : tous les alliés en combat subissent -1 dégât par attaque

#### ⚡ Serment-Binaire
*L'offensive comme forme de protection.*
- Smite : chaque attaque inflige +1d8 dégâts sacrés supplémentaires
- Soin critique : si d20 = 20 lors d'un soin → PV restaurés doublés

---

## 🎵 Algorythmien
**Équivalent D&D : Barde | Rôle : Buff collectif / DPS magie**

### Lore
*Arbonet a une fréquence. La plupart ne l'entendent pas. L'Algorythmien, lui, la compose. Sa musique n'est pas de l'art — c'est du code sonique, des algorithmes traduits en vibrations. Quand il joue, les circuits s'alignent, les erreurs se résolvent, et ses alliés sentent quelque chose d'étrange : la certitude que tout va bien se passer. Il peut aussi parler à n'importe qui — y compris aux ennemis.*

### Stats de base
| Stat | Valeur de base | Jet de création |
|------|---------------|-----------------|
| PV | 16 | + 1d6 |
| CA | 11 | + 1d4 (0 à 3) |
| Mana | 20 | fixe |
| Charisme | 16 | fixe |
| Bonus Attaque | 0 | + 1d4 (0 à 3) |
| Arme | Luth-Code | fixe |
| Dé de dégâts | 1d6 | magique |

### Actions disponibles
| Commande | Coût | Effet |
|----------|------|-------|
| `!attaquer` | — | d20 vs CA ennemi → 1d6 dégâts |
| `!soigner` | 5 mana | Récupère 1d6 PV |
| `!buff` | 5 mana | **TOUS** les alliés → +2 attaque (3 tours) |
| `!discuter` | — | Charisme 16 → seuil abaissé à **8** |
| `!fuir` | — | d20 ≥ 12 → fuite réussie |

> *L'Algorythmien est le seul à pouvoir buff toute l'équipe simultanément.*
> *Son Charisme 16 lui permet de tenter de convaincre un ennemi sans jet difficile.*

### Sous-classes — Niveau 5
#### 🎸 Virtuose-Offensif
*La musique devient une arme.*
- Dé de dégâts passe à 1d8
- `!buff` ajoute également +1d4 dégâts temporaires à tous les alliés

#### 💚 Guérisseur-Fréquence
*La fréquence soigne.*
- `!soigner` passe à 1d8+3 PV
- Déblocage de `!revive @allié` : 15 mana, 1 fois par quête → revive un allié avec 50% PV

---

*Document vivant — mis à jour au fil du Cycle 404.*
*Projet Pointu © Florian alias kikaby67 — 2026*

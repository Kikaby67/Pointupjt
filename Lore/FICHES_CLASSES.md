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
| PV | 25 | + 1d6 (+1 à +6) |
| CA | 14 | + 1d4 (+1 à +4) |
| Mana | 5 | fixe |
| Charisme | 8 | fixe |
| Bonus Attaque | 0 | + 1d4 (+1 à +4) |
| Arme | Épée | fixe |
| Dé de dégâts | 1d8 | — |

### Actions disponibles
| Commande | Coût | Effet |
|----------|------|-------|
| `!attaque` | — | d20 + bonus vs CA ennemi → 1d8 dégâts |
| `!soin` | 5 mana | Récupère 1d4 PV |
| `!defense` | — | CA +3 ce tour, pas d'attaque |
| `!fuir` | — | d20 ≥ 12 → fuite réussie |

### Progression
| Niveau | XP requis | Gain |
|--------|-----------|------|
| 2 | 300 | +3 PV max |
| 3 | 900 | +1 CA, +1 PV max |
| 4 | 2 700 | +3 PV max |
| **5** | **6 500** | **Sous-classe débloquée** |
| 6 | 14 000 | +3 PV max |
| 7 | 23 000 | +1 CA, +1 PV max |
| 8 | 34 000 | +100 RAM |
| 9 | 48 000 | +3 PV max |
| 10 | 64 000 | +2 Charisme |

### Sous-classes — Niveau 5
#### 🧱 Bloc-Hex
*Dans les profondeurs d'Arbonet, certains Hexadécimeurs cessent d'être des combattants pour devenir des structures. Le Bloc-Hex est un nœud du réseau incarné — un bloc de données si dense qu'il ne peut être corrompu. Il ne recule jamais. 0xFF : valeur maximale, immuable.*
- **+8 PV maximum** à la sélection
- Frappe toujours avec une seule attaque puissante

#### ⚡ Surcharge
*Quand la mémoire déborde, les systèmes s'emballent. La Surcharge est cet état — l'Hexadécimeur qui dépasse ses propres limites d'exécution. Deux frappes par tour, une CA sacrifiée. L'overflow comme philosophie de combat.*
- **-2 CA permanente**
- `!attaque` déclenche **2 attaques 1d8** consécutives

---

## 🗡️ Cryptolame
**Équivalent D&D : Voleur | Rôle : DPS / Fuite**

### Lore
*Ni tout à fait dans le réseau, ni tout à fait dans la nature — le Cryptolame existe dans l'interstice. Il chiffre ses mouvements, rend ses intentions illisibles. Ses deux lames sont des clés de déchiffrement : l'une ouvre la défense, l'autre exploite la brèche. Il ne bat pas ses ennemis — il les rend obsolètes.*

### Stats de base
| Stat | Valeur de base | Jet de création |
|------|---------------|-----------------|
| PV | 16 | + 1d6 (+1 à +6) |
| CA | 13 | + 1d4 (+1 à +4) |
| Mana | 5 | fixe |
| Charisme | 11 | fixe |
| Bonus Attaque | 0 | + 1d4 (+1 à +4) |
| Arme | Double-Dagues | fixe |
| Dé de dégâts | 1d6 + 1d6 | 2 attaques par tour |

### Actions disponibles
| Commande | Coût | Effet |
|----------|------|-------|
| `!attaque` | — | 2 jets d20 → 1d6 + 1d6 dégâts |
| `!soin` | 5 mana | Récupère 1d4 PV |
| `!defense` | — | CA +3 ce tour, pas d'attaque |
| `!fuir` | — | d20 ≥ **8** → fuite réussie ⭐ |

> *Le Cryptolame fuit plus facilement que les autres classes — seuil abaissé à 8.*

### Progression
*(Même tableau que l'Hexadécimeur)*

### Sous-classes — Niveau 5
#### 👻 Byte-Fantôme
*Un Byte-Fantôme n'est pas dans les logs. Il n'a pas de signature. Ses trois lames frappent dans des couches de temps différentes — avant, pendant, et après que l'ennemi ait compris qu'il était déjà touché. Introuvable. Inarrêtable.*
- **3 attaques 1d6** par tour au lieu de 2

#### 🎯 Pointeur-Null
*Le Pointeur-Null pointe vers ce qui n'existe pas — et provoque un crash. Arc en main, le Cryptolame efface ses traces et tire depuis les nœuds du réseau que personne ne surveille. Chaque flèche est une référence non résolue, fatale à l'impact.*
- Arme passe à **Arc-Binaire** (typeArme = Arc-Binaire)
- Dé de dégâts : **1d10** (une seule attaque par tour)

---

## 💻 Hackmancien
**Équivalent D&D : Mage | Rôle : DPS magie / Buff ciblé**

### Lore
*Là où d'autres voient un chêne-serveur, le Hackmancien voit une faille. Son art est une nécromancie inversée : au lieu de ressusciter les morts, il injecte de la corruption ciblée dans les vivants. Ses sorts ne sont pas des incantations — ce sont des commits mal formés, envoyés directement dans le système nerveux de ses cibles.*

### Stats de base
| Stat | Valeur de base | Jet de création |
|------|---------------|-----------------|
| PV | 14 | + 1d6 (+1 à +6) |
| CA | 10 | + 1d4 (+1 à +4) |
| Mana | 30 | fixe |
| Charisme | 10 | fixe |
| Bonus Attaque | 0 | + 1d4 (+1 à +4) |
| Arme | Bâton-Magique | fixe |
| Dé de dégâts | 1d12 | magique |

### Actions disponibles
| Commande | Coût | Effet |
|----------|------|-------|
| `!attaque` | — | d20 vs CA ennemi → 1d12 dégâts magiques |
| `!soin` | 5 mana | Récupère 1d6 PV |
| `!defense` | — | CA +3 ce tour, pas d'attaque |
| `!fuir` | — | d20 ≥ 12 → fuite réussie |

### Sous-classes — Niveau 5
#### 🕳️ Faille-Zéro
*La Faille-Zéro est l'exploit que personne n'a encore patché. Le Hackmancien qui la maîtrise frappe là où le système ne peut se défendre — 2d8 dégâts bruts, directs dans le cœur du code. Aucun pare-feu connu n'y résiste.*
- Dé de dégâts passe à **2d8** (max 16)

#### 🔧 Compilateur
*Le Compilateur ne combat pas seul — il assemble. Comme un programme qui orchestre des modules indépendants, il transforme la force brute de ses alliés en un exécutable commun. Il ne frappe pas plus fort. Il fait frapper les autres mieux.*
- Peut cibler un allié pour lui donner **+2 bonus attaque**

---

## 🛡️ Firewaller
**Équivalent D&D : Paladin | Rôle : Tank / Soin**

### Lore
*Avant les Firewallers, les zones corrompues ne pouvaient être traversées. Ces gardiens sont des murs vivants — leur corps filtre la corruption comme un pare-feu filtre les paquets suspects. Là où ils se tiennent, rien ne passe sans leur accord. Et quand ils soignent, ce n'est pas de la magie — c'est une restauration de système, ligne par ligne.*

### Stats de base
| Stat | Valeur de base | Jet de création |
|------|---------------|-----------------|
| PV | 22 | + 1d6 (+1 à +6) |
| CA | 15 | + 1d4 (+1 à +4) |
| Mana | 25 | fixe |
| Charisme | 13 | fixe |
| Bonus Attaque | 0 | + 1d4 (+1 à +4) |
| Arme | Marteau-Rune | fixe |
| Dé de dégâts | 1d8 | — |

### Actions disponibles
| Commande | Coût | Effet |
|----------|------|-------|
| `!attaque` | — | d20 vs CA ennemi → 1d8 dégâts |
| `!soin` | 5 mana | Récupère **1d8+3 PV** ⭐ |
| `!defense` | — | CA +3 ce tour, pas d'attaque |
| `!fuir` | — | d20 ≥ 12 → fuite réussie |

> *Le Firewaller a le soin le plus puissant de toutes les classes de base.*

### Sous-classes — Niveau 5
#### 🔰 Protocole-Sacré
*Le Protocole-Sacré est une règle absolue gravée dans les racines d'Arbonet : rien ne passe. Le Firewaller qui l'a intégré devient une aura vivante — sa présence seule réduit les dégâts subis par ses alliés. Un rempart que le réseau lui-même respecte.*
- Aura passive : tous les alliés en combat subissent **-1 dégât** par attaque

#### ⚡ Serment-Binaire
*Le Serment-Binaire n'a pas de milieu. Zéro ou un. Vivre ou brûler. Chaque frappe du Firewaller est une assertion divine — le Smite est le moment où le code se prononce, sans appel ni recours.*
- Chaque attaque inflige **+1d8 dégâts** supplémentaires (Smite)

---

## 🎵 Algorythmancien
**Équivalent D&D : Barde | Rôle : Buff collectif / Soin**

### Lore
*Arbonet a une fréquence. La plupart ne l'entendent pas. L'Algorythmancien, lui, la compose. Sa musique n'est pas de l'art — c'est du code sonique, des algorithmes traduits en vibrations. Quand il joue, les circuits s'alignent, les erreurs se résolvent, et ses alliés sentent quelque chose d'étrange : la certitude que tout va bien se passer.*

### Stats de base
| Stat | Valeur de base | Jet de création |
|------|---------------|-----------------|
| PV | 16 | + 1d6 (+1 à +6) |
| CA | 11 | + 1d4 (+1 à +4) |
| Mana | 20 | fixe |
| Charisme | 16 | fixe |
| Bonus Attaque | 0 | + 1d4 (+1 à +4) |
| Arme | Luth-Code | fixe |
| Dé de dégâts | 1d8 | magique |

### Actions disponibles
| Commande | Coût | Effet |
|----------|------|-------|
| `!attaque` | — | d20 vs CA ennemi → 1d8 dégâts |
| `!soin` | 5 mana | Récupère 1d6 PV |
| `!defense` | — | CA +3 ce tour, pas d'attaque |
| `!fuir` | — | d20 ≥ 12 → fuite réussie |

### Sous-classes — Niveau 5
#### 🎸 Barde-Binaire
*Le Barde-Binaire a compris que la musique d'Arbonet est binaire par nature — des signaux qui s'enchaînent, des ondes qui portent des instructions. Sa mélodie en 1d10 résonne dans tous les circuits alliés à la fois, les synchronisant dans un même tempo offensif. Un seul accord. Tous touchés.*
- Dé de dégâts passe à **1d10**
- Buff offensif disponible pour **toute l'équipe simultanément**

#### 💚 Patch-Mélodique
*Arbonet est plein d'erreurs. Le Patch-Mélodique les corrige en jouant. Sa fréquence de soin est une mise à jour en temps réel — chaque note referme une blessure, chaque mesure restaure un processus corrompu. 1d8+3 PV : le meilleur soin du réseau.*
- `!soin` passe à **1d8+3 PV**

---

*Document vivant — mis à jour au fil du Cycle 404.*
*Projet Pointu © Florian alias kikaby67 — 2026*

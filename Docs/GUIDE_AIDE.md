# 🐢 Guide de l'Aventurier — L'Antre de Pointu

> Contenu destiné au Google Doc lié à la commande `!aide`. Tout se joue dans le chat Twitch.

Bienvenue dans **Arbonet**, un monde où la nature et la technologie s'entremêlent. Les chênes-serveurs
abritent la vie, mais **Hector-Pierre Castor** et ses créatures-cyber corrompent le réseau. **Pointu**, la
tortue gardienne, recrute des Aventuriers (toi !) pour défendre l'Antre.

---

## 1. Bien démarrer

1. `!rejoindre` — crée ton personnage (ton **Fragment de Carapace**).
2. `!choisirclasse <classe>` — choisis ta voie. Un jet de dés détermine tes stats de départ.
3. `!profil` — vérifie tes caractéristiques à tout moment.

**Les 5 classes** (tape la commande d'info pour les détails) :

| Classe | Points forts | Info |
|--------|--------------|------|
| **Hexadécimeur** | PV élevés, équilibré | `!hexadécimeur` |
| **Cryptolame** | Très agile (fuite facile) | `!cryptolame` |
| **Hackmancien** | Mana énorme, gros dégâts | `!hackmancien` |
| **Firewaller** | Armure (CA) et soins solides | `!firewaller` |
| **Algorythmancien** | Charisme max (discussion/recrutement) | `!algorythmancien` |

> À **niveau 5**, tu débloques une **sous-classe** : `!sousclasse <nom>`.

---

## 2. Tes caractéristiques

- **PV** — tes points de vie. À 0, tu es **à terre** (voir §5).
- **CA** (Classe d'Armure) — ta défense.
- **Atq** — ta puissance d'attaque.
- **Mana** — sert à te soigner (`!soin`).
- **Charisme** — réussite de `!discuter` (parlementer / recruter).
- **Agilité** — réussite de `!fuir`.
- **RAM** — la monnaie d'Arbonet (achats, ventes).

Tes objets équipés améliorent ces stats. Une **armure lourde** augmente la CA mais **réduit l'agilité** (plus dur de fuir).

---

## 3. Les quêtes

`!quete` — pars en mission (durée **5 à 30 min** selon la quête). Tu n'as rien à faire pendant ce temps,
mais des **rencontres** peuvent surgir en route (voir §4).

- À la fin : **succès** (XP + RAM, parfois un objet) ou **échec**.
- `!abandon` — quitter une quête en cours (pénalité de cooldown).

---

## 4. Les rencontres ⚔️ (le cœur du jeu)

Pendant une quête, une créature peut surgir. La quête se met en **pause** et tu as **3 choix** (tu as ~2 min
pour répondre, sinon la créature s'en va) :

### `!combat` — te battre
Ta **chance de victoire** dépend de tes stats (PV, CA, Atq) et de ton équipement. Plus tu es équipé, plus
elle monte.
- **Victoire** → tu gagnes de l'XP et de la RAM, mais tu **perds des PV** (le combat use). La quête reprend.
- **Défaite contre une créature faible/moyenne** → tu survis mais encaisses une **grosse perte de PV**.
- **Défaite contre une créature forte** → **KO** : ta quête échoue et tu dois récupérer dans l'Antre.

### `!discuter` — parlementer (selon ton **charisme**)
- **Réussite (sans allié)** → tu **recrutes** la créature ! Elle devient ton **compagnon** et **booste tes
  chances de combat** jusqu'à la fin de la quête (ou ta prochaine défaite).
- **Réussite (avec un allié déjà recruté)** → tu **passes** la rencontre sans combattre.
- **Échec** → la créature refuse : il te reste `!combat` ou `!fuir`.

### `!fuir` — t'échapper (selon ton **agilité**)
- **Réussite** → tu files, la quête reprend.
- **Échec** → la créature te barre la route : choisis `!combat` ou `!discuter`.

> 💡 Un **Cryptolame** fuit facilement ; un **Algorythmancien** recrute facilement ; un guerrier bien équipé
> gagne ses combats. Joue selon ta classe !

---

## 5. PV, soin et récupération ❤️

Tu **perds des PV** à chaque combat. Quand tes PV tombent à **0**, tu es **à terre** : tu ne peux plus partir
en quête tant que tu n'es pas soigné. **Les soins se font hors combat** :

- `!repos` — restauration **complète** des PV et du mana dans l'Antre (cooldown 30 min).
- `!soin` — petit soin qui coûte du **mana**.
- `!utiliser Potion` — restaure des PV et du mana (depuis ton sac).

---

## 6. Équipement & objets 🎒

- `!inventaire` — voir ton sac et ton équipement.
- `!equiper <objet>` — équiper une arme / armure / accessoire.
- `!vendre <objet>` — vendre contre de la RAM.
- `!utiliser <objet>` — consommer (ex. Potion).
- `!acheter` — quand un **marchand** passe, lui acheter une Potion.

---

## 7. Rencontres spéciales 🤝

Pendant une quête, tu peux aussi croiser :

- **Le Vieux Sage** — `!accepter` son marché (chance d'XP, mais risque de perdre un objet) ou `!refuser`
  (il peut t'attaquer → résous la rencontre avec `!combat` / `!discuter` / `!fuir`).
- **Le Marchand ambulant** — `!accepter` ses soins **et/ou** `!acheter` une Potion. Deux choix séparés,
  rien n'est imposé. Ou `!refuser`.
- **Événements** — bonus (RAM, XP, soin) ou petits malus, appliqués automatiquement.

---

## 8. Toutes les commandes

**Personnage** : `!rejoindre` · `!choisirclasse <classe>` · `!sousclasse <nom>` · `!profil` · `!arbonet`
**Quête** : `!quete` · `!abandon`
**Rencontre** : `!combat` · `!discuter` · `!fuir`
**Soin (hors combat)** : `!repos` · `!soin` · `!utiliser Potion`
**Sac** : `!inventaire` · `!equiper <objet>` · `!vendre <objet>` · `!acheter`
**Offres** : `!accepter` · `!refuser`
**Infos classes** : `!hexadécimeur` · `!cryptolame` · `!hackmancien` · `!firewaller` · `!algorythmancien`
**Classement** : `!classement` (réservé au streamer)

---

🐢 *« Le réseau murmure. Sauras-tu l'écouter, Aventurier ? »* — Pointu

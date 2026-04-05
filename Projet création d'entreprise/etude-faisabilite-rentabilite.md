# Étude de faisabilité et estimation de rentabilité
## Pointu-PJT — Jeu RPG Twitch + Discord via Streamer.bot

---

## 1. Concept résumé

Vendre aux streamers un **pack clé-en-main** comprenant :
- Un système de jeu RPG interactif sur Twitch (chat-driven)
- Une intégration Discord (rôles, récompenses, classements)
- Un fichier `.sb` uploadable sur Streamer.bot (zéro code pour le streamer)
- Un tutoriel pas-à-pas pour l'installation et la personnalisation

---

## 2. Étude de marché

### Cible principale
| Segment | Volume estimé (FR/BE/CH) | Maturité tech |
|---|---|---|
| Petits streamers (50–500 viewers) | ~8 000–15 000 | Faible à moyen |
| Streamers moyens (500–5 000 viewers) | ~1 500–3 000 | Moyen à élevé |
| Streamers anglophones (international) | Plusieurs dizaines de milliers | Variable |

### Concurrence directe
| Produit | Prix | Limite |
|---|---|---|
| Overlay génériques (StreamElements) | Gratuit (freemium) | Pas de RPG intégré |
| Bots Twitch payants (Nightbot Premium) | ~5$/mois | Pas de dimension jeu |
| Solutions custom (dev freelance) | 500–3 000€ | Pas accessible aux petits |

**Avantage concurrentiel** : pack tout-en-un, sans compétence technique, avec gameplay narratif unique + Discord natif.

---

## 3. Modèle économique

### Option A — Achat unique (recommandé pour le lancement)
| Offre | Prix | Contenu |
|---|---|---|
| Pack Starter | 19€ | Fichier .sb + tuto PDF |
| Pack Pro | 49€ | Starter + Discord bot config + 3 scénarios |
| Pack Studio | 99€ | Pro + personnalisation sur mesure (lore, UI) |

### Option B — Abonnement mensuel (phase 2)
| Offre | Prix/mois | Contenu |
|---|---|---|
| Solo | 7€/mois | Accès au pack de base + mises à jour |
| Agence | 29€/mois | Multi-streamer, branding custom, priorité support |

---

## 4. Estimation de rentabilité

### Hypothèse conservatrice — Année 1

| Indicateur | Valeur |
|---|---|
| Ventes Pack Starter | 60 unités × 19€ = **1 140€** |
| Ventes Pack Pro | 30 unités × 49€ = **1 470€** |
| Ventes Pack Studio | 5 unités × 99€ = **495€** |
| **CA total estimé** | **3 105€** |

### Charges estimées (Année 1)

| Poste | Coût |
|---|---|
| Hébergement (si serveur bot Discord) | ~60–120€/an |
| Outils (Gumroad, Stripe, domaine) | ~100–150€/an |
| Marketing (réseaux, créas) | ~200–300€/an |
| **Total charges** | **~400–570€/an** |

### Marge nette estimée
> **2 500–2 700€ net la première année** en hypothèse conservatrice.

### Hypothèse optimiste — Année 1 (avec traction internationale)
- CA potentiel : **8 000–15 000€**
- Marge nette : **6 000–12 000€**

---

## 5. Étude de faisabilité technique

| Composant | Faisabilité | Statut |
|---|---|---|
| Fichier Streamer.bot (.sb) | Haute | En cours (Pointu-PJT) |
| Intégration Discord | Haute | Partiel (DiscordBot/) |
| Tuto pas-à-pas | Haute | À créer |
| Système de paiement (Gumroad/Stripe) | Haute | Non démarré |
| Site vitrine | Moyenne | Non démarré |
| Support client (Discord communauté) | Haute | Non démarré |

---

## 6. Risques et mitigations

| Risque | Probabilité | Impact | Mitigation |
|---|---|---|---|
| Peu de traction initiale | Moyenne | Élevé | Streamer partenaire pour test public |
| Changement API Twitch/Discord | Faible | Élevé | Veille tech, mises à jour incluses dans Pro/Studio |
| Copie par concurrents | Faible | Moyen | Branding fort, relation communauté |
| Charge support trop élevée | Moyenne | Moyen | Tuto exhaustif, FAQ Discord, auto-réponses bot |

---

## 7. Conclusion

**Faisabilité : OUI** — Le projet est techniquement quasi-prêt, le marché existe, les charges sont faibles.

**Priorité immédiate** : finaliser le fichier `.sb` + tuto, puis lancer une version bêta gratuite sur 2–3 streamers partenaires pour valider le concept avant de monétiser.

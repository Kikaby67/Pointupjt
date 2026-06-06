"""Génération de la fiche personnage PNG (/fiche).

Prend un dict profil (tel que stocké dans Donnees/joueurs/<nom>.json) et renvoie un
buffer PNG. Réutilise le thème néon et les polices de calendrier_image.
"""
import io
from PIL import Image, ImageDraw
from calendrier_image import (_font, _texte_centre,
                              COUL_FOND, COUL_PANEL, COUL_NEON, COUL_BLANC, COUL_DIM)

# Seuils d'XP par niveau (index = niveau) — miroir de config_level côté jeu
XP_SEUILS = [0, 0, 300, 900, 2700, 6500, 14000, 23000, 34000, 48000, 64000]


def _i(joueur, cle, defaut=0):
    try:
        return int(joueur.get(cle, defaut))
    except (TypeError, ValueError):
        return defaut


def _xp_progress(niveau, xp):
    """Fraction de progression vers le niveau suivant + seuil suivant (None si max)."""
    if niveau >= len(XP_SEUILS) - 1:
        return 1.0, None
    cur, nxt = XP_SEUILS[niveau], XP_SEUILS[niveau + 1]
    if nxt <= cur:
        return 1.0, nxt
    return max(0.0, min(1.0, (xp - cur) / (nxt - cur))), nxt


def _inventaire_groupe(inv: str) -> str:
    """CSV 'Potion,Potion,Gants' → 'Potion ×2 · Gants'. '' → 'vide'."""
    if not inv:
        return "vide"
    ordre, comptes = [], {}
    for item in inv.split(","):
        item = item.strip()
        if not item:
            continue
        if item not in comptes:
            ordre.append(item)
        comptes[item] = comptes.get(item, 0) + 1
    return " · ".join(f"{it} ×{comptes[it]}" if comptes[it] > 1 else it for it in ordre)


def _barre(draw, x0, y0, x1, y1, frac, couleur):
    """Barre de progression arrondie (fond panel + remplissage couleur)."""
    draw.rounded_rectangle((x0, y0, x1, y1), radius=8, fill=(0, 0, 0), outline=COUL_DIM, width=1)
    larg = int((x1 - x0) * max(0.0, min(1.0, frac)))
    if larg > 4:
        draw.rounded_rectangle((x0, y0, x0 + larg, y1), radius=8, fill=couleur)


def _stat(draw, x0, y0, x1, y1, label, valeur, f_lab, f_val, coul_val=COUL_BLANC):
    """Cellule de stat : panneau + label (haut) + valeur (bas)."""
    draw.rounded_rectangle((x0, y0, x1, y1), radius=10, fill=COUL_PANEL, outline=COUL_NEON, width=2)
    _texte_centre(draw, (x0, y0 + 8, x1, y0 + 32), label, f_lab, COUL_DIM)
    _texte_centre(draw, (x0, y0 + 30, x1, y1 - 8), str(valeur), f_val, coul_val)


def generer_fiche(joueur: dict) -> io.BytesIO:
    """Compose la fiche personnage PNG et renvoie un buffer."""
    W = 700
    PAD = 18
    H = 940

    img  = Image.new("RGB", (W, H), COUL_FOND)
    draw = ImageDraw.Draw(img)

    f_titre = _font(46, gras=True)
    f_sous  = _font(24)
    f_lab   = _font(20)
    f_val   = _font(34, gras=True)
    f_txt   = _font(24)
    f_petit = _font(20)

    draw.rectangle((4, 4, W - 5, H - 5), outline=COUL_NEON, width=3)

    nom       = joueur.get("nomJoueur") or "Aventurier"
    classe    = joueur.get("classe") or "—"
    sousCl    = joueur.get("sousClasse") or ""
    niveau    = _i(joueur, "niveau", 1)
    xp        = _i(joueur, "experience")
    ram       = _i(joueur, "ram")

    # ── En-tête ───────────────────────────────────────
    _texte_centre(draw, (0, 22, W, 22 + 52), nom, f_titre, COUL_NEON)
    sous = classe + (f" · {sousCl}" if sousCl and sousCl != "0" else "") + f"   —   Niveau {niveau}"
    _texte_centre(draw, (0, 80, W, 80 + 30), sous, f_sous, COUL_BLANC)

    # ── Barre d'XP ────────────────────────────────────
    frac, nxt = _xp_progress(niveau, xp)
    y = 124
    draw.text((PAD, y), "XP", font=f_lab, fill=COUL_DIM)
    _barre(draw, PAD + 40, y + 2, W - PAD - 150, y + 24, frac, COUL_NEON)
    txt_xp = f"{xp}" + (f" / {nxt}" if nxt else "  (MAX)")
    draw.text((W - PAD - 140, y), txt_xp, font=f_lab, fill=COUL_BLANC)
    draw.text((W - PAD - 140, y + 26), f"RAM : {ram}", font=f_lab, fill=COUL_BLANC)

    # ── Grille de stats (2 colonnes × 3 lignes) ───────
    pvA, pvM   = _i(joueur, "pvActuels"), _i(joueur, "pvMax")
    manaA, manaM = _i(joueur, "manaActuels"), _i(joueur, "manaMax")
    ca   = _i(joueur, "classeArmure")
    atq  = _i(joueur, "bonusAttaque")
    cha  = _i(joueur, "charisme")
    agi  = _i(joueur, "agilite")

    gy0 = 190
    cellH, gap = 96, 12
    colW = (W - 2 * PAD - gap) // 2
    cx = [PAD, PAD + colW + gap]

    # Ligne 1 : PV (large, avec barre) sur les 2 colonnes
    pvFrac = pvA / pvM if pvM else 0
    coulPV = COUL_NEON if pvA > 0 else (255, 70, 70)
    draw.rounded_rectangle((PAD, gy0, W - PAD, gy0 + cellH), radius=10, fill=COUL_PANEL, outline=COUL_NEON, width=2)
    draw.text((PAD + 16, gy0 + 12), "PV", font=f_lab, fill=COUL_DIM)
    draw.text((W - PAD - 130, gy0 + 10), f"{pvA} / {pvM}", font=f_val, fill=coulPV)
    _barre(draw, PAD + 16, gy0 + 58, W - PAD - 16, gy0 + 82, pvFrac, coulPV)

    # Lignes 2-3 : CA · ATQ / MANA · les autres
    gy1 = gy0 + cellH + gap
    _stat(draw, cx[0], gy1, cx[0] + colW, gy1 + cellH, "CLASSE D'ARMURE", ca, f_lab, f_val)
    _stat(draw, cx[1], gy1, cx[1] + colW, gy1 + cellH, "ATTAQUE", f"+{atq}", f_lab, f_val)

    gy2 = gy1 + cellH + gap
    mana_txt = f"{manaA} / {manaM}" if manaM else "—"
    _stat(draw, cx[0], gy2, cx[0] + colW, gy2 + cellH, "MANA", mana_txt, f_lab, f_val)
    # Charisme + Agilité côte à côte dans la 2e colonne
    demiW = (colW - gap) // 2
    _stat(draw, cx[1], gy2, cx[1] + demiW, gy2 + cellH, "CHARISME", cha, f_lab, f_val)
    _stat(draw, cx[1] + demiW + gap, gy2, cx[1] + colW, gy2 + cellH, "AGILITÉ", agi, f_lab, f_val)

    # ── Équipement ────────────────────────────────────
    def eq(c):
        v = joueur.get(c, "")
        return v if v and v != "0" else "—"
    ey = gy2 + cellH + gap + 6
    draw.text((PAD, ey), "ÉQUIPEMENT", font=f_lab, fill=COUL_DIM)
    draw.text((PAD, ey + 26), f"Arme : {eq('armeEquipee')}", font=f_txt, fill=COUL_BLANC)
    draw.text((PAD, ey + 54), f"Armure : {eq('armureEquipee')}", font=f_txt, fill=COUL_BLANC)
    draw.text((PAD, ey + 82), f"Accessoire : {eq('accessoireEquipe')}", font=f_txt, fill=COUL_BLANC)

    # ── Sac ───────────────────────────────────────────
    sy = ey + 124
    draw.text((PAD, sy), "SAC", font=f_lab, fill=COUL_DIM)
    sac = _inventaire_groupe(joueur.get("inventaire", "") or "")
    # wrap simple sur 2 lignes
    lignes_sac, ligne = [], ""
    for part in sac.split(" · "):
        essai = (ligne + " · " + part) if ligne else part
        if _font(22).getlength(essai) > W - 2 * PAD and ligne:
            lignes_sac.append(ligne)
            ligne = part
        else:
            ligne = essai
    if ligne:
        lignes_sac.append(ligne)
    for k, l in enumerate(lignes_sac[:2]):
        draw.text((PAD, sy + 26 + k * 26), l, font=f_petit, fill=COUL_BLANC)

    # ── Bas : statut + bilan ──────────────────────────
    stats = joueur.get("statistiques", {}) if isinstance(joueur.get("statistiques"), dict) else {}
    vict = stats.get("combatsGagnes", 0)
    def_ = stats.get("combatsPerdus", 0)
    qst  = stats.get("quetesTerminees", 0)

    if str(joueur.get("enRencontre")).lower() == "true":
        statut, coul = "EN RENCONTRE", (255, 200, 0)
    elif str(joueur.get("enQuete")).lower() == "true":
        statut, coul = f"EN QUÊTE ({joueur.get('queteId', '')})", COUL_NEON
    elif pvA <= 0:
        statut, coul = "À TERRE — se soigne dans l'Antre", (255, 70, 70)
    else:
        statut, coul = "PRÊT À L'AVENTURE", COUL_NEON

    by = H - 132
    draw.rounded_rectangle((PAD, by, W - PAD, by + 100), radius=10, fill=COUL_PANEL, outline=COUL_NEON, width=2)
    _texte_centre(draw, (PAD, by + 10, W - PAD, by + 42), statut, f_txt, coul)
    bilan = f"Combats {vict}V / {def_}D     Quêtes {qst}"
    compagnon = joueur.get("compagnonActif", "")
    if compagnon and compagnon != "0":
        bilan += f"     Compagnon : {compagnon}"
    _texte_centre(draw, (PAD, by + 56, W - PAD, by + 90), bilan, f_petit, COUL_BLANC)

    _texte_centre(draw, (0, H - 28, W, H - 6), "L'Antre de Pointu", f_petit, COUL_DIM)

    buf = io.BytesIO()
    img.save(buf, format="PNG")
    buf.seek(0)
    return buf

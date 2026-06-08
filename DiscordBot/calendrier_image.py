"""Génération de l'affiche PNG du planning stream (/calendrier).

Module autonome : ne dépend que de Pillow (pas de discord), donc testable seul.
"""
import os
import io
import gc
import datetime
from functools import lru_cache
from PIL import Image, ImageDraw, ImageFont, ImageFilter

BASE_DIR    = os.path.dirname(os.path.abspath(__file__))
BANNERS_DIR = os.path.join(BASE_DIR, "banners")   # 1 image par jeu (slug.png)
FONTS_DIR   = os.path.join(BASE_DIR, "fonts")     # polices .ttf optionnelles

JOURS_FR_LONG = ["LUNDI", "MARDI", "MERCREDI", "JEUDI", "VENDREDI", "SAMEDI", "DIMANCHE"]

# Palette néon
COUL_FOND  = (8, 12, 8)
COUL_PANEL = (16, 24, 16)
COUL_NEON  = (57, 255, 20)      # #39FF14
COUL_BLANC = (235, 245, 235)
COUL_DIM   = (110, 130, 110)


def slug(nom: str) -> str:
    """Nom de jeu → identifiant fichier (minuscules, alphanum uniquement)."""
    return "".join(ch for ch in nom.lower() if ch.isalnum())


def trouver_banner(jeu: str):
    """Cherche une image dont le nom (slugifié) correspond au jeu.

    Compare en slug des deux côtés → insensible à la casse, aux espaces et à
    la ponctuation. Indispensable car Discloud (Linux) est sensible à la casse :
    'Terraria.png' ne serait pas trouvé par 'terraria.png'.
    """
    cible = slug(jeu)
    if not cible or not os.path.isdir(BANNERS_DIR):
        return None
    for nom in os.listdir(BANNERS_DIR):
        base, ext = os.path.splitext(nom)
        if ext.lower() in (".png", ".jpg", ".jpeg", ".webp") and slug(base) == cible:
            return os.path.join(BANNERS_DIR, nom)
    return None


@lru_cache(maxsize=64)
def _font(taille: int, gras: bool = False):
    """Police perso (fonts/bold.ttf|regular.ttf) si présente, sinon défaut scalable.
    Mise en cache : évite de re-parser le fichier TTF (~750 Ko) à chaque appel."""
    noms = (["bold.ttf", "regular.ttf"] if gras else ["regular.ttf", "bold.ttf"])
    for nom in noms:
        chemin = os.path.join(FONTS_DIR, nom)
        if os.path.exists(chemin):
            try:
                return ImageFont.truetype(chemin, taille)
            except Exception:
                pass
    try:
        return ImageFont.load_default(size=taille)   # Pillow >= 10.1
    except TypeError:
        return ImageFont.load_default()


def _cover(img, w: int, h: int):
    """Redimensionne+recadre pour remplir exactement w×h (façon CSS cover)."""
    iw, ih = img.size
    echelle = max(w / iw, h / ih)
    nw, nh = max(1, int(iw * echelle)), max(1, int(ih * echelle))
    img = img.resize((nw, nh), Image.LANCZOS)
    gauche = (nw - w) // 2
    haut   = (nh - h) // 2
    return img.crop((gauche, haut, gauche + w, haut + h))


def _contain(img, w: int, h: int):
    """Redimensionne SANS recadrer : l'image entière tient dans w×h."""
    iw, ih = img.size
    echelle = min(w / iw, h / ih)
    nw, nh = max(1, int(iw * echelle)), max(1, int(ih * echelle))
    return img.resize((nw, nh), Image.LANCZOS)


def _banniere_remplie(src, w: int, h: int):
    """Image w×h : bannière entière (contain) nette au centre, sur un fond
    flouté de la même image (remplit les bords sans barres noires ni découpe)."""
    fond = _cover(src, w, h).filter(ImageFilter.GaussianBlur(14))
    fond = Image.blend(fond, Image.new("RGB", (w, h), (0, 0, 0)), 0.40)
    av = _contain(src, w, h)
    fond.paste(av, ((w - av.width) // 2, (h - av.height) // 2))
    return fond


def _font_ajuste(draw, texte, largeur_max, taille_base, gras=False):
    """Plus grande police (≤ taille_base) telle que `texte` tienne en largeur_max."""
    taille = taille_base
    while taille > 12:
        f = _font(taille, gras=gras)
        bb = draw.textbbox((0, 0), texte, font=f)
        if bb[2] - bb[0] <= largeur_max:
            return f
        taille -= 2
    return _font(12, gras=gras)


def _texte_centre(draw, boite, texte, font, fill):
    """Dessine `texte` centré dans la boîte (x0,y0,x1,y1)."""
    x0, y0, x1, y1 = boite
    bb = draw.textbbox((0, 0), texte, font=font)
    tw, th = bb[2] - bb[0], bb[3] - bb[1]
    draw.text((x0 + (x1 - x0 - tw) / 2 - bb[0],
               y0 + (y1 - y0 - th) / 2 - bb[1]), texte, font=font, fill=fill)


def _heures_uniques(streams) -> str:
    """Heures pour la colonne jour, dédupliquées en gardant l'ordre."""
    return " · ".join(dict.fromkeys(s["time"] for s in streams))


def generer_image_calendrier(data: dict, lundi) -> io.BytesIO:
    """Compose l'affiche PNG du planning de la semaine et renvoie un buffer PNG."""
    W = 1100
    HEADER_H = 165
    ROW_H    = 200          # lignes plus hautes → bannières plus grandes
    FOOTER_H = 64
    PAD      = 16
    H = HEADER_H + 7 * ROW_H + FOOTER_H

    img  = Image.new("RGB", (W, H), COUL_FOND)
    draw = ImageDraw.Draw(img)

    f_titre = _font(64, gras=True)
    f_sous  = _font(24)
    f_heure = _font(30, gras=True)
    f_jeu   = _font(32, gras=True)
    f_repos = _font(28)
    f_foot  = _font(22)

    draw.rectangle((4, 4, W - 5, H - 5), outline=COUL_NEON, width=3)

    _texte_centre(draw, (0, 22, W, 22 + 72), "CALENDRIER STREAM", f_titre, COUL_NEON)
    _texte_centre(draw, (0, 104, W, 104 + 32),
                  "◆  PLANNING DE LA SEMAINE  ◆", f_sous, COUL_BLANC)

    COL_JOUR_W = 280        # élargie pour que MERCREDI/DIMANCHE rentrent
    for i in range(7):
        jour    = lundi + datetime.timedelta(days=i)
        streams = sorted(data.get(jour.isoformat(), []),
                         key=lambda s: s.get("time", ""))
        y0 = HEADER_H + i * ROW_H
        y1 = y0 + ROW_H - PAD // 2
        cy0 = y0 + PAD // 2

        # Colonne gauche : jour + date + heure(s)
        gx0, gx1 = PAD, COL_JOUR_W
        draw.rounded_rectangle((gx0, cy0, gx1, y1),
                               radius=10, fill=COUL_PANEL, outline=COUL_NEON, width=2)
        libelle = f"{JOURS_FR_LONG[i]} {jour.day}"
        f_jour  = _font_ajuste(draw, libelle, gx1 - gx0 - 20, 34, gras=True)
        _texte_centre(draw, (gx0, y0 + 24, gx1, y0 + 76), libelle, f_jour, COUL_BLANC)
        heures = _heures_uniques(streams) if streams else "—"
        _texte_centre(draw, (gx0, y0 + 96, gx1, y1 - 10), heures, f_heure, COUL_NEON)

        # Colonne droite
        dx0, dx1 = COL_JOUR_W + PAD, W - PAD
        cw, ch = dx1 - dx0, y1 - cy0

        if not streams:
            draw.rounded_rectangle((dx0, cy0, dx1, y1), radius=10,
                                   fill=COUL_PANEL, outline=COUL_NEON, width=2)
            _texte_centre(draw, (dx0, cy0, dx1, y1), "REPOS / CONTENU", f_repos, COUL_DIM)
            continue

        # Une sous-case par jeu (bannières côte à côte)
        n   = len(streams)
        gap = 8 if n > 1 else 0
        sub_w = (cw - gap * (n - 1)) // n
        for idx, s in enumerate(streams):
            sx0 = dx0 + idx * (sub_w + gap)
            sx1 = dx1 if idx == n - 1 else sx0 + sub_w   # le dernier prend le reste
            scw = sx1 - sx0
            masque = Image.new("L", (scw, ch), 0)
            ImageDraw.Draw(masque).rounded_rectangle((0, 0, scw, ch), radius=10, fill=255)
            banner = trouver_banner(s["game"])
            if banner:
                try:
                    src = Image.open(banner).convert("RGB")
                    img.paste(_banniere_remplie(src, scw, ch), (sx0, cy0), masque)
                    draw.rounded_rectangle((sx0, cy0, sx1, y1), radius=10,
                                           outline=COUL_NEON, width=2)
                    continue
                except Exception:
                    pass
            # pas de bannière → panneau avec le nom du jeu
            draw.rounded_rectangle((sx0, cy0, sx1, y1), radius=10,
                                   fill=COUL_PANEL, outline=COUL_NEON, width=2)
            _texte_centre(draw, (sx0, cy0, sx1, y1), s["game"], f_jeu, COUL_BLANC)

    _texte_centre(draw, (0, H - FOOTER_H, W, H - 6),
                  "À TRÈS VITE SUR LE STREAM !  ♥", f_foot, COUL_NEON)

    buf = io.BytesIO()
    img.save(buf, format="PNG")
    img.close()
    gc.collect()
    buf.seek(0)
    return buf

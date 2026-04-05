import discord
import os
import json
import base64
import sys
import traceback
import datetime
import requests
from typing import Optional
from discord.ext import commands

# ─── LOG ERREURS ──────────────────────────────────────
def log_error(exc_type, exc_value, exc_tb):
    with open("error.log", "a") as f:
        f.write(
            f"{datetime.datetime.now()} - "
            f"{''.join(traceback.format_exception(exc_type, exc_value, exc_tb))}\n"
        )

sys.excepthook = log_error

# ─── CONFIG ───────────────────────────────────────────
DISCORD_TOKEN = os.getenv("DISCORD_TOKEN")
GITHUB_REPO   = "Kikaby67/Pointupjt"
CHANNEL_ID    = 1490232175382102016

# ─── GITHUB ───────────────────────────────────────────

def get_joueur(nom: str) -> Optional[dict]:
    """Lit le profil d'un joueur depuis le repo GitHub."""
    url = (
        f"https://api.github.com/repos/{GITHUB_REPO}"
        f"/contents/Donnees/joueurs/{nom}.json"
    )
    headers = {
        "Accept": "application/vnd.github.v3+json",
    }
    resp = requests.get(url, headers=headers, timeout=10)
    if resp.status_code != 200:
        return None
    contenu = resp.json().get("content", "")
    return json.loads(base64.b64decode(contenu).decode("utf-8"))

# ─── BOT ──────────────────────────────────────────────

intents = discord.Intents.default()
intents.message_content = True

bot = commands.Bot(command_prefix="!", intents=intents, help_command=None)

@bot.event
async def on_ready():
    print(f"PointuBot connecté → {bot.user} (id: {bot.user.id})")

# ─── COMMANDES ────────────────────────────────────────

@bot.command(name="profil")
async def profil(ctx):
    """Affiche le profil Arbonet du joueur."""
    if ctx.channel.id != CHANNEL_ID:
        return

    nom = ctx.author.name.lower()
    joueur = get_joueur(nom)

    if joueur is None:
        await ctx.send(
            f"⚔️ **{nom}**, tu n'es pas encore inscrit dans l'Antre.\n"
            f"Rejoins un live Twitch et tape `!rejoindre` pour commencer ton aventure !"
        )
        return

    niveau = joueur.get("niveau", 1)
    xp     = joueur.get("xp", 0)
    ram    = joueur.get("ram", 10)
    sac    = joueur.get("sac", [])
    sac_str = ", ".join(sac) if sac else "vide"

    xp_prochain = niveau * 100
    barre = int((xp / xp_prochain) * 10) if xp_prochain > 0 else 0
    barre_xp = "█" * barre + "░" * (10 - barre)

    await ctx.send(
        f"🐢 **Profil Arbonet — {nom}**\n"
        f"━━━━━━━━━━━━━━━━\n"
        f"⚔️ Niveau : {niveau}\n"
        f"✨ XP     : {xp} / {xp_prochain}  [{barre_xp}]\n"
        f"💾 RAM    : {ram}\n"
        f"🎒 Sac    : {sac_str}"
    )


@bot.command(name="arbonet")
async def arbonet(ctx):
    """Présente l'univers d'Arbonet."""
    if ctx.channel.id != CHANNEL_ID:
        return
    await ctx.send(
        "🌿 **Arbonet** — Un monde où nature et technologie coexistent en équilibre fragile.\n"
        "Les **Corbeaux-Daemon** volent les souvenirs. "
        "Les **Castors-Rootkit** rongent les chênes-serveurs.\n"
        "**Pointu**, gardien ancien, cherche des aventuriers pour récupérer les fragments perdus.\n\n"
        "➡️ Rejoins un live Twitch et tape `!rejoindre` pour entrer dans l'Antre !"
    )


@bot.command(name="aide")
async def aide(ctx):
    """Liste les commandes disponibles."""
    if ctx.channel.id != CHANNEL_ID:
        return
    await ctx.send(
        "🐢 **Commandes PointuBot**\n"
        "━━━━━━━━━━━━━━━━\n"
        "`!profil`   — Affiche ton profil d'aventurier\n"
        "`!arbonet`  — Découvre l'univers du jeu\n"
        "`!aide`     — Cette liste\n\n"
        "📺 Pour t'inscrire, rejoins un live Twitch et tape `!rejoindre`"
    )


bot.run(DISCORD_TOKEN)

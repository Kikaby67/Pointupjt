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
from discord import app_commands
from dotenv import load_dotenv
from calendrier_image import generer_image_calendrier, trouver_banner, slug
from fiche_image import generer_fiche

# ─── LOG ERREURS ──────────────────────────────────────
def log_error(exc_type, exc_value, exc_tb):
    trace = "".join(traceback.format_exception(exc_type, exc_value, exc_tb))
    # Visible dans les logs Discloud + conservé dans le fichier
    print(f"{datetime.datetime.now()} - {trace}", flush=True)
    with open("error.log", "a") as f:
        f.write(f"{datetime.datetime.now()} - {trace}\n")

sys.excepthook = log_error

# ─── CONFIG ───────────────────────────────────────────
# Discloud ne charge pas .env automatiquement → on le lit nous-mêmes
ENV_PATH = os.path.join(os.path.dirname(os.path.abspath(__file__)), ".env")
load_dotenv(ENV_PATH)
print(f".env trouvé: {os.path.exists(ENV_PATH)}", flush=True)
DISCORD_TOKEN = os.getenv("DISCORD_TOKEN")
GITHUB_REPO   = "Kikaby67/Pointupjt"
CHANNEL_ID    = 1490232175382102016

# Chemins (à côté du script pour partir avec le zip Discloud)
BASE_DIR      = os.path.dirname(os.path.abspath(__file__))
STREAMS_FILE  = os.path.join(BASE_DIR, "streams.json")
JOURS_FR      = ["Lun", "Mar", "Mer", "Jeu", "Ven", "Sam", "Dim"]

# ─── PLANNING (lecture/écriture synchrone) ────────────

def charger_streams() -> dict:
    """Charge streams.json de façon synchrone (évite les race conditions)."""
    if not os.path.exists(STREAMS_FILE):
        return {}
    with open(STREAMS_FILE, "r", encoding="utf-8") as f:
        try:
            return json.load(f)
        except json.JSONDecodeError:
            return {}

def sauver_streams(data: dict) -> None:
    """Écrit streams.json de façon synchrone."""
    with open(STREAMS_FILE, "w", encoding="utf-8") as f:
        json.dump(data, f, ensure_ascii=False, indent=2)

# ─── GITHUB ───────────────────────────────────────────

def get_joueur(nom: str) -> Optional[dict]:
    """Lit le profil d'un joueur depuis GitHub (raw, cache CDN ~5 min, pas de quota API)."""
    url = (f"https://raw.githubusercontent.com/{GITHUB_REPO}/main"
           f"/Donnees/joueurs/{nom.lower()}.json")
    try:
        resp = requests.get(url, timeout=10)
        if resp.status_code != 200:
            return None
        return resp.json()
    except (requests.RequestException, ValueError):
        return None

# ─── BOT ──────────────────────────────────────────────

intents = discord.Intents.default()
intents.message_content = True

bot = commands.Bot(command_prefix="!", intents=intents, help_command=None)

@bot.event
async def on_ready():
    # Sync par-serveur = apparition instantanée (le sync global met jusqu'à ~1h)
    try:
        total = 0
        for guild in bot.guilds:
            bot.tree.copy_global_to(guild=guild)
            synced = await bot.tree.sync(guild=guild)
            total += len(synced)
            print(f"Slash sync serveur {guild.name} ({guild.id}) : {len(synced)} commandes")
        if not bot.guilds:
            print("⚠️ Le bot n'est dans AUCUN serveur — réinvite-le avec le scope applications.commands")
        print(f"Slash commands synchronisées (total) : {total}")
    except Exception as e:
        print(f"Erreur sync slash commands : {type(e).__name__} : {e}")
    print(f"PointuBot connecté → {bot.user} (id: {bot.user.id})")

# ─── COMMANDES (prefix) ───────────────────────────────

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

# ─── SLASH COMMANDS — PLANNING ────────────────────────

def _embed_calendrier(data: dict, lundi) -> discord.Embed:
    """Repli texte si la génération d'image échoue."""
    lignes = []
    for i in range(7):
        jour    = lundi + datetime.timedelta(days=i)
        streams = sorted(data.get(jour.isoformat(), []),
                         key=lambda s: s.get("time", ""))
        contenu = (", ".join(f"{s['time']} {s['game']}" for s in streams)
                   if streams else "—")
        lignes.append(f"**{JOURS_FR[i]} {jour.day}** → {contenu}")
    embed = discord.Embed(title="📅 Planning stream de la semaine",
                          description="\n".join(lignes), color=0x39FF14)
    embed.set_footer(text="Mis à jour • /calendrier pour rafraîchir")
    embed.timestamp = datetime.datetime.now()
    return embed


@bot.tree.command(name="calendrier",
                  description="Affiche le planning stream de la semaine en cours")
async def calendrier(interaction: discord.Interaction):
    await interaction.response.defer()
    data  = charger_streams()
    today = datetime.date.today()
    lundi = today - datetime.timedelta(days=today.weekday())
    try:
        buf = generer_image_calendrier(data, lundi)
        await interaction.followup.send(
            file=discord.File(buf, filename="calendrier.png"))
    except Exception as e:
        print(f"Image calendrier KO, repli embed : {type(e).__name__} : {e}",
              flush=True)
        await interaction.followup.send(embed=_embed_calendrier(data, lundi))


@bot.tree.command(name="addstream",
                  description="Ajoute un stream au planning (admin)")
@app_commands.describe(jour="Date au format YYYY-MM-DD",
                       heure="Heure au format HH:MM",
                       jeu="Nom du jeu / catégorie")
@app_commands.default_permissions(administrator=True)
async def addstream(interaction: discord.Interaction,
                    jour: str, heure: str, jeu: str):
    try:
        datetime.datetime.strptime(jour, "%Y-%m-%d")
        datetime.datetime.strptime(heure, "%H:%M")
    except ValueError:
        await interaction.response.send_message(
            "❌ Format invalide — jour : `YYYY-MM-DD`, heure : `HH:MM`.",
            ephemeral=True)
        return

    data = charger_streams()
    data.setdefault(jour, []).append({"time": heure, "game": jeu})
    data[jour].sort(key=lambda s: s["time"])
    sauver_streams(data)

    statut_img = ("🖼️ bannière trouvée" if trouver_banner(jeu)
                  else f"📭 pas de bannière — ajoute `banners/{slug(jeu)}.png`")
    await interaction.response.send_message(
        f"✅ Ajouté : **{jour}** → {heure} {jeu}\n{statut_img}", ephemeral=True)


@bot.tree.command(name="delstream",
                  description="Supprime un stream du planning (admin)")
@app_commands.describe(jour="Date au format YYYY-MM-DD",
                       index="Numéro du stream (0 = premier de la journée)")
@app_commands.default_permissions(administrator=True)
async def delstream(interaction: discord.Interaction,
                    jour: str, index: int):
    data    = charger_streams()
    streams = data.get(jour)
    if not streams or index < 0 or index >= len(streams):
        await interaction.response.send_message(
            "❌ Aucun stream à cet index pour ce jour.", ephemeral=True)
        return

    retire = streams.pop(index)
    if not streams:
        del data[jour]
    sauver_streams(data)
    await interaction.response.send_message(
        f"🗑️ Supprimé : **{jour}** → {retire['time']} {retire['game']}",
        ephemeral=True)


# ─── SLASH COMMAND — FICHE PERSO ──────────────────────

@bot.tree.command(name="fiche",
                  description="Affiche la fiche personnage d'un aventurier de l'Antre")
@app_commands.describe(pseudo="Pseudo Twitch de l'aventurier (par défaut : ton pseudo Discord)")
async def fiche(interaction: discord.Interaction, pseudo: Optional[str] = None):
    await interaction.response.defer()
    nom = (pseudo or interaction.user.name).strip().lstrip("@")
    joueur = get_joueur(nom)
    if joueur is None:
        await interaction.followup.send(
            f"⚔️ **{nom}** n'est pas (encore) dans l'Antre — ou son profil n'est pas encore synchronisé. "
            f"Rejoins un live Twitch et tape `!rejoindre` ! "
            f"(astuce : `/fiche pseudo:<ton pseudo Twitch>` si ton nom Discord diffère)")
        return
    try:
        buf = generer_fiche(joueur)
        await interaction.followup.send(file=discord.File(buf, filename=f"fiche_{nom}.png"))
    except Exception as e:
        print(f"Fiche KO : {type(e).__name__} : {e}", flush=True)
        await interaction.followup.send("⚠️ Impossible de générer la fiche pour le moment.")


@bot.tree.error
async def on_app_command_error(interaction: discord.Interaction, error):
    if isinstance(error, app_commands.MissingPermissions):
        msg = "⛔ Commande réservée aux admins."
    else:
        msg = f"⚠️ Erreur : {error}"
    if interaction.response.is_done():
        await interaction.followup.send(msg, ephemeral=True)
    else:
        await interaction.response.send_message(msg, ephemeral=True)


if __name__ == "__main__":
    print(f"Démarrage PointuBot — token présent: {bool(DISCORD_TOKEN)}", flush=True)
    try:
        bot.run(DISCORD_TOKEN)
    except Exception as e:
        print(f"CRASH bot.run : {type(e).__name__} : {e}", flush=True)
        traceback.print_exc()
        raise

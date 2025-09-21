# Third-Party Notices

This document lists all third-party content used in **Sword Trials: Final Boss** (Unity).  
**No Unity Asset Store packages or other paid/download-gated files are redistributed in this repository.**  
Please obtain them from the original sources as noted below.

---

## Legend
- **License** (summary): This is a human-readable reminder. Always refer to the original license/EULA for the exact terms.
- **Included in repo?**: Whether raw files are committed here. For Asset Store items, this should be **No**.

---

## 1) Unity Asset Store (NOT redistributed)

> These items are covered by the **Unity Asset Store EULA**.  
> You may use them in the built game, but you **cannot** rehost the raw assets in this Git repository.

- **Dungeon Modular Pack** — by **logicalbeat**  
  - **Link:** 🔁 https://assetstore.unity.com/packages/3d/environments/dungeons/dungeon-modular-pack-295430
  - **Version used:** 1.0  
  - **License:** Standard Unity Asset Store EULA  
  - **Included in repo?** No  
  - **Used for:** dungeon environment
  - **Where referenced in project:** `Assets`  
  - **Notes:** Import via Package Manager/Unity Asset Store; do not commit source files.

- **RPG_Animations_Pack_FREE** — by **DoubleL**  
  - **Link:** https://assetstore.unity.com/packages/3d/animations/rpg-animations-pack-free-288783
  - **Version used:** 1.4
  - **License:** Standard Unity Asset Store EULA  
  - **Included in repo?** Yes 
  - **Used for:** Player 
  - **Where referenced in project:** `Assets/Animation`  

- **Long Sword** — by **Digital Horde**  
  - **Link:** https://assetstore.unity.com/packages/3d/props/weapons/long-sword-212082
  - **Version used:** 1.0.0
  - **License:** Standard Unity Asset Store EULA  
  - **Included in repo?** Yes 
  - **Used for:** sword 
  - **Where referenced in project:** `Assets/Animation`  

- **Humans army - Guard** — by **Simple Game Assets**  
  - **Link:** https://assetstore.unity.com/packages/3d/characters/humanoids/fantasy/humans-army-guard-310092
  - **Version used:** 1.0
  - **License:** Standard Unity Asset Store EULA  
  - **Included in repo?** Yes 
  - **Used for:** Final Boss 
  - **Where referenced in project:** `Assets/Animation`  



## 2) Unity Packages (via Package Manager)

> Unity-provided packages are licensed under their respective terms (often the **Unity Companion License** or Unity’s standard terms).  
> Check **Window → Package Manager → (select package) → View details/license** inside your editor for the latest license.

- **Input System** (com.unity.inputsystem) — **License:** see package details  
- **Cinemachine** (com.unity.cinemachine) — **License:** see package details  
- **TextMeshPro** (com.unity.textmeshpro) — **License:** see package details  
- **URP/HDRP** — **License:** see package details


## 3) How to Obtain & Import Third-Party Assets

1. **Install Unity packages** via **Package Manager** (Input System, Cinemachine, etc.).  
2. **Purchase/download Asset Store packages** using your Unity account, then import them directly in the Editor.  
3. **External assets** (audio, fonts, etc.): follow their site’s download & license instructions.  
4. **Do not commit** any Asset Store raw files or other content that forbids redistribution. Keep them out via `.gitignore` or avoid staging those paths.


# Grafkom - Unity 3D Procedural City

Project Unity 3D dengan AI-assisted development menggunakan MCP (Model Context Protocol).

## Requirements

- **Unity 6** (6000.0.x atau lebih baru)
- **Git**

## Quick Start

### 1. Clone Repository
```bash
git clone https://github.com/Yeyodra/Grafkom.git
```

### 2. Buka di Unity Hub
1. Buka **Unity Hub**
2. Klik **Add** → **Add project from disk**
3. Pilih folder `Grafkom` yang sudah di-clone
4. Klik project untuk membuka

### 3. Buka Scene
- Buka `Assets/Scenes/SampleScene.unity`

## Git Workflow (Untuk Kolaborasi)

### Sebelum mulai kerja
```bash
git pull
```

### Setelah selesai kerja
```bash
git add .
git commit -m "Deskripsi perubahan"
git push
```

### Jika ada conflict
```bash
git pull --rebase
# Fix conflicts di file yang bentrok
git add .
git rebase --continue
git push
```

## Struktur Project

```
Grafkom/
├── Assets/
│   ├── Scenes/          # Scene files
│   ├── Scripts/         # C# scripts
│   ├── Materials/       # Materials
│   └── Prefabs/         # Prefabs
├── Packages/            # Unity packages
└── ProjectSettings/     # Project settings
```

## Tips

- **JANGAN** push folder `Library/`, `Temp/`, `Logs/` (sudah di-ignore)
- Selalu `git pull` sebelum mulai kerja
- Komunikasi dengan tim sebelum edit file yang sama

## Contributors

- Yeyodra

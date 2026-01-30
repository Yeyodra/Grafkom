# Quest 2 Upgrade Design: "Bersih-bersih Kota"

**Date:** 2026-01-30
**Status:** Ready for Implementation

---

## Overview

Player bertemu **Petugas Kebersihan** (NPC Among Us hijau) yang sakit di dekat **Hospital**. Ada **mobil sampah** (box truck) yang mogok di sampingnya. NPC minta tolong bersihkan 5 sampah di kota.

## Core Loop

```
1. Player ke Hospital → ketemu NPC + mobil sampah
2. Dialog: NPC minta tolong (tone sedih, batuk-batuk)
3. Quest aktif → 5 sampah spawn random dari 12-15 spawn points
4. Player collect sampah (max 2 di inventory)
5. Player buang ke mobil sampah (harus 3x trip)
6. Semua sampah dibuang → Cutscene ending (NPC terima kasih, mobil jalan pergi)
```

---

## Component Details

### 1. NPC Petugas Kebersihan

| Aspek | Detail |
|-------|--------|
| Model | Among Us style (reuse dari Player/NPC lain) |
| Warna | **Hijau** (warna petugas kebersihan) |
| Posisi | Duduk/berdiri lemas di samping mobil |
| Lokasi | Dekat Hospital, di pinggir jalan |
| Components | `AmongUsPlayer`, `NPCDialogue`, `NPCIndicator`, `SphereCollider` |

### 2. Mobil Sampah (Box Truck)

```
        ┌──────────────────────┐
        │    BAK SAMPAH        │  ← Bak terbuka (bisa diisi)
        │    (Box Collider)    │
┌───────┼──────────────────────┤
│ CABIN │                      │
│  ██   │                      │
└───●───┴──────────────────────●┘
  Roda                        Roda
```

**Struktur GameObject:**

```
GarbageTruck
├── Cabin (Cube, scaled) - Warna hijau tua
├── Bak (Cube, scaled) - Warna hijau muda, open top
├── Wheel_FL (Cylinder)
├── Wheel_FR (Cylinder)
├── Wheel_BL (Cylinder)
├── Wheel_BR (Cylinder)
└── DropZone (Box Collider, trigger) - Area buang sampah
```

**Dimensi:**
- Total panjang: ~5-6 unit
- Tinggi cabin: ~2 unit
- Tinggi bak: ~1.5 unit

### 3. Inventory System (Hotbar)

```
┌─────────────────────────────────────────────┐
│                 GAME VIEW                   │
│                                             │
├─────────────────────────────────────────────┤
│           ┌─────┐  ┌─────┐                  │
│           │  🗑 │  │     │   ← 2 slot       │
│           │ 1/2 │  │ 2/2 │                  │
│           └─────┘  └─────┘                  │
│              CENTER BOTTOM                  │
└─────────────────────────────────────────────┘
```

**Logic:**
- Max 2 slot
- Collect: sampah terbang ke slot + sound + particle
- Drop: tekan E di DropZone mobil

### 4. Spawn System

- 12-15 spawn points (empty GameObject) di lokasi valid
- Random pilih 5 saat quest mulai
- Floating indicator (🗑) di atas tiap sampah

### 5. Dialog

**Opening (4 lines):**
1. "Uhuk uhuk... Aduh..."
2. "Saya sudah 3 hari demam, tapi sampah kota numpuk di mana-mana..."
3. "Mobilnya juga mogok di sini... uhuk..."
4. "Kamu... bisa bantu kumpulkan 5 sampah? Buang ke mobil ini ya..."

**Ending (3 lines):**
1. "Wah... kamu hebat sekali!"
2. "Terima kasih banyak! Kota jadi bersih berkat kamu."
3. "Sepertinya saya sudah agak baikan... Saya bisa lanjutkan dari sini!"

### 6. Ending Cutscene

| Step | Duration | Action |
|------|----------|--------|
| 1 | 0s | Player auto-jalan ke depan NPC |
| 2 | 1s | Camera zoom ke NPC + Player |
| 3 | 1-4s | Dialog ending |
| 4 | 4-6s | NPC warna berubah lebih cerah (sembuh) |
| 5 | 6-8s | Mobil sampah jalan pergi |
| 6 | 8s | Quest Complete panel muncul |

---

## Implementation Tasks

**PENTING: Setiap task yang selesai LANGSUNG PUSH ke GitHub!**

### Phase 1: Core Infrastructure

#### Task 1.1: InventoryManager.cs
- [ ] Buat script `Assets/Scripts/Inventory/InventoryManager.cs`
- [ ] Singleton pattern
- [ ] List<string> items, maxSlots = 2
- [ ] Methods: AddItem(), RemoveItem(), ClearItems(), IsFull(), GetCount()
- [ ] Event: OnInventoryChanged

**Commit:** `feat(quest2): add InventoryManager singleton`

---

#### Task 1.2: InventoryUI.cs
- [ ] Buat script `Assets/Scripts/Inventory/InventoryUI.cs`
- [ ] Create UI Canvas dengan 2 slot di center bottom
- [ ] Subscribe ke InventoryManager.OnInventoryChanged
- [ ] Update slot visuals saat inventory berubah

**Commit:** `feat(quest2): add InventoryUI hotbar with 2 slots`

---

#### Task 1.3: Slot Animation
- [ ] Animate slot saat item masuk (scale pop 1 → 1.2 → 1)
- [ ] Trash icon sprite di slot

**Commit:** `feat(quest2): add slot pop animation`

---

### Phase 2: Trash Collection

#### Task 2.1: TrashCollectible.cs
- [ ] Buat script `Assets/Scripts/Quest/TrashCollectible.cs`
- [ ] OnTriggerEnter: check player, check inventory not full
- [ ] Collect: add to inventory, destroy self
- [ ] Integrate dengan QuestManager

**Commit:** `feat(quest2): add TrashCollectible pickup script`

---

#### Task 2.2: Fly Animation
- [ ] Sampah lerp/tween dari world position ke screen slot position
- [ ] Duration: 0.3-0.5s
- [ ] Easing: ease-out

**Commit:** `feat(quest2): add trash fly-to-inventory animation`

---

#### Task 2.3: FloatingIndicator.cs
- [ ] Buat script `Assets/Scripts/UI/FloatingIndicator.cs`
- [ ] Billboard (always face camera)
- [ ] Trash icon sprite
- [ ] Hover animation (naik turun pelan)

**Commit:** `feat(quest2): add floating trash indicator`

---

### Phase 3: Garbage Truck

#### Task 3.1: GarbageTruck Prefab
- [ ] Buat prefab dari primitives (Cubes + Cylinders)
- [ ] Cabin: hijau tua
- [ ] Bak: hijau muda
- [ ] 4 roda

**Commit:** `feat(quest2): add GarbageTruck prefab model`

---

#### Task 3.2: GarbageTruck.cs
- [ ] Buat script `Assets/Scripts/Quest/GarbageTruck.cs`
- [ ] DropZone trigger collider
- [ ] OnTriggerEnter/Stay: show prompt "Tekan E untuk buang sampah"
- [ ] Input E: transfer inventory ke truck, update quest progress
- [ ] Particle effect saat buang

**Commit:** `feat(quest2): add GarbageTruck drop-off logic`

---

#### Task 3.3: Truck Drive Away
- [ ] Method DriveAway(): Transform.Translate ke kanan
- [ ] Duration: 3-4 detik
- [ ] Destroy setelah offscreen

**Commit:** `feat(quest2): add truck drive away animation`

---

### Phase 4: Spawn System

#### Task 4.1: Spawn Points Setup
- [ ] Buat 12-15 empty GameObject sebagai spawn points
- [ ] Parent: `_QuestSystem/Quest2_Collection/SpawnPoints`
- [ ] Lokasi: jalan, trotoar, taman (hindari dalam gedung)

**Commit:** `feat(quest2): add 15 trash spawn points`

---

#### Task 4.2: TrashSpawner.cs
- [ ] Buat script `Assets/Scripts/Quest/TrashSpawner.cs`
- [ ] Reference ke semua spawn points
- [ ] Method SpawnTrash(int count): random pilih, instantiate prefab
- [ ] Integrate dengan QuestManager

**Commit:** `feat(quest2): add random trash spawner`

---

### Phase 5: NPC & Dialog

#### Task 5.1: NPC Petugas Kebersihan
- [ ] Duplicate existing NPC, rename "NPC_PetugasKebersihan"
- [ ] Set warna hijau
- [ ] Posisi dekat Hospital
- [ ] Setup NPCDialogue dengan opening dialog

**Commit:** `feat(quest2): add NPC_PetugasKebersihan`

---

#### Task 5.2: Opening Dialog Integration
- [ ] 4 dialog lines sesuai design
- [ ] After dialog complete → trigger quest start
- [ ] Spawn trash, show inventory UI

**Commit:** `feat(quest2): add opening dialog for garbage collector NPC`

---

### Phase 6: Quest Integration

#### Task 6.1: Update QuestManager
- [ ] Modify Quest 2 logic untuk support new flow
- [ ] Track: totalTrashDelivered (bukan collected)
- [ ] Quest complete saat totalTrashDelivered >= 5

**Commit:** `feat(quest2): update QuestManager for delivery-based completion`

---

#### Task 6.2: Quest Progress UI
- [ ] Update progress text: "X/5 sampah dibuang"
- [ ] Show inventory count juga

**Commit:** `feat(quest2): update quest progress UI`

---

### Phase 7: Ending Cutscene

#### Task 7.1: Quest2Cutscene.cs
- [ ] Extend/modify QuestCutscene
- [ ] Sequence: camera zoom, dialog, NPC color change, truck drive away

**Commit:** `feat(quest2): add ending cutscene sequence`

---

#### Task 7.2: Ending Dialog
- [ ] 3 dialog lines sesuai design
- [ ] NPC color brighten (sembuh)
- [ ] Trigger truck.DriveAway()

**Commit:** `feat(quest2): add ending dialog and NPC recovery`

---

### Phase 8: Polish

#### Task 8.1: Sound Effects
- [ ] Collect sound (swoosh)
- [ ] Drop sound (plop/thud)
- [ ] Quest complete jingle

**Commit:** `feat(quest2): add sound effects`

---

#### Task 8.2: Particle Effects
- [ ] Collect: sparkle di tempat sampah
- [ ] Drop: dust/leaves di mobil

**Commit:** `feat(quest2): add particle effects`

---

#### Task 8.3: Testing & Bug Fixes
- [ ] Test full flow Quest 1 → Quest 2
- [ ] Fix any bugs
- [ ] Verify semua edge cases

**Commit:** `fix(quest2): bug fixes and polish`

---

## Summary Checklist

| Phase | Tasks | Status |
|-------|-------|--------|
| 1. Core Infrastructure | 1.1, 1.2, 1.3 | Pending |
| 2. Trash Collection | 2.1, 2.2, 2.3 | Pending |
| 3. Garbage Truck | 3.1, 3.2, 3.3 | Pending |
| 4. Spawn System | 4.1, 4.2 | Pending |
| 5. NPC & Dialog | 5.1, 5.2 | Pending |
| 6. Quest Integration | 6.1, 6.2 | Pending |
| 7. Ending Cutscene | 7.1, 7.2 | Pending |
| 8. Polish | 8.1, 8.2, 8.3 | Pending |

**Total: 18 tasks, 18 commits**

---

## Git Workflow

```bash
# Setelah setiap task selesai:
git add .
git commit -m "<commit message dari task>"
git push
```

**JANGAN tunggu sampai selesai semua! Push setiap task untuk safety.**

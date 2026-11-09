# 🛡️ PrivateViewer (Dark Title Bar & Zen Mode Edition)

[![Author](https://img.shields.io/badge/Author-Alex%20Ponzin%20(%40Ilponz)-blue.svg)](https://github.com/Ilponz)
[![License](https://img.shields.io/badge/License-Proprietary%20Attribution-red.svg)](https://github.com/Ilponz/PrivateViewer/blob/main/LICENSE)
[![Platform](https://img.shields.io/badge/Platform-Windows%2010%20%7C%2011-lightgrey.svg)](https://github.com/Ilponz/PrivateViewer)

> **Ideato e sviluppato originariamente da [Alex Ponzin (@Ilponz)](https://github.com/Ilponz)**  
> *Repository Ufficiale:* [https://github.com/Ilponz/PrivateViewer](https://github.com/Ilponz/PrivateViewer)

---

## ⚡ Autorialità e Watermark
Questo software include un **watermark indelebile di paternità**:
* **Nei binari Windows (.exe):** Metadati assembly con firma d'autore, copyright e marchio registrati nelle proprietà del file.
* **Nell'interfaccia utente:** Barra inferiore con firma attiva e collegamento diretto al profilo GitHub dell'autore originario (`Alex Ponzin`).
* **Nel codice sorgente:** Intestazioni e costanti con attribuzione inderogabile.

---

## 🚀 Come Funziona

A differenza di strumenti che tentano l'iniezione rischiosa di DLL in processi esterni (spesso bloccati da antivirus o privi di permessi), **PrivateViewer** implementa la protezione direttamente a livello del proprio processo utilizzando l'API nativa di Windows:

```csharp
[DllImport("user32.dll", SetLastError = true)]
private static extern bool SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);

// WDA_EXCLUDEFROMCAPTURE = 0x00000011 (Windows 10 2004+ / Windows 11)
SetWindowDisplayAffinity(this.Handle, 0x00000011);
```

---

## ✨ Nuove Funzionalità v2.3

1. **🖤 Barra del Titolo Nativa Nera (Dark Title Bar):**
   * Tramite l'API nativa DWM di Windows (`DwmSetWindowAttribute`), la barra superiore del titolo con il nome della finestra e i tasti di riduzione, ingrandimento e chiusura (`✕`) è ora **completamente nera** con testo bianco, eliminando la fastidiosa barra chiara standard di Windows.
2. **👁️ Modalità Focus / Nascondi Menu (Zen Mode):**
   * Premi il pulsante **`▲ Nascondi Menu`** o la scorciatoia **`Ctrl + M`** (oppure `F1`) per collassare tutte le barre superiori e visualizzare **solo le schede e le pagine web a tutto schermo**.
   * Quando il menu è nascosto, appare una discreta mini-barra con il pulsante **`▼ Mostra Menu (Ctrl+M)`** per riaprirlo con un clic.
3. **🖥️ Modalità Schermo Intero Immersiva (`F11`):**
   * Premi **`F11`** per passare alla modalità senza bordi a schermo intero (Zero distrazioni).
4. **👻 Modalità Stealth (Invisibile nella Barra delle Applicazioni):**
   * L'applicazione non compare nella barra delle applicazioni di Windows in basso.
   * Icona Tray (scudo) affidabile al 100% per ripristinare/nascondere la finestra (click sinistro) o aprire il menu contestuale (click destro).
5. **📑 Browser Multi-Scheda Completo (Multi-Tab Chromium):**
   * Nuova Scheda: **`➕ Scheda`** o **`Ctrl + T`**.
   * Chiudi Scheda: **`✕`** o **`Ctrl + W`**.
   * Scheda successiva: **`Ctrl + Tab`**.
   * Layout responsive al 100%: si adatta a qualsiasi dimensione senza sovrapposizioni.
6. **📂 Visualizzatore Documenti PDF & File Locali:**
   * Apri file PDF o immagini dal pulsante dedicato o semplicemente trascinandoli dentro la finestra (Drag-and-Drop).
7. **🚀 Centro Web Apps con 1 Clic:**
   * Pulsanti rapidi per **ChatGPT**, **Gemini**, **WhatsApp Web**, **Telegram**, **Wikipedia**, **Calcolatrice**, ecc.
8. **Boss Key Globale (`ESC`):** Nasconde la finestra istantaneamente nella barra di sistema.

---

## 🛠️ Come Avviare o Ricompilare

* **Eseguibile già pronto:** Avvia direttamente `PrivateViewer.exe`.
* **Ricompilare:** Esegui `build.bat` (utilizza il compilatore nativo `csc.exe` già incluso in Windows e referenzia i moduli WebView2 inclusi).

---

## 📜 Licenza
Il software è protetto da clausola di attribuzione proprietaria. Leggi il file [LICENSE](https://github.com/Ilponz/PrivateViewer/blob/main/LICENSE) per i dettagli. È vietata la rimozione del watermark e dei crediti d'autore.

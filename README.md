# 🛡️ PrivateViewer (Chrome-Style Multi-Tab & Dark Edition)

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

## ✨ Barra Schede in Stile Browser (Chrome/Edge) v2.4

1. **🔄 Sposta e Riordina le Schede (Drag & Drop):**
   * Clicca con il mouse su qualsiasi scheda e trascinala a destra o sinistra per riordinarla liberamente in tempo reale, proprio come in Google Chrome.
2. **➕ Aggiungi Schede Facilmente:**
   * Clicca sul pulsante permanente **`➕`** posizionato all'estremità delle schede.
   * Fai **doppio clic sullo spazio vuoto** della barra per aprire subito una nuova scheda.
   * Scorciatoia rapida: **`Ctrl + T`**.
3. **✕ Rimuovi e Chiudi le Schede:**
   * Fai **clic con la rotellina (tasto centrale)** direttamente sulla scheda per chiuderla istantaneamente.
   * Clicca sul pulsante **`✕`** presente nella barra della scheda o usa **`Ctrl + W`**.
   * Fai **clic destro sulla scheda** per aprire il menu contestuale:
     * *✕ Chiudi Scheda*
     * *🚫 Chiudi le altre schede*
     * *📄 Duplica Scheda* (apre un clone con lo stesso link)
     * *🔄 Ricarica*
     * *⬅️ Sposta a Sinistra* / *➡️ Sposta a Destra*
4. **🖤 Barra del Titolo Nativa Nera (Dark Title Bar):**
   * La barra in alto di Windows (con titolo e pulsanti riduci, ingrandisci e chiudi `✕`) è 100% nera con scritte e icone bianche via API DWM nativa.
5. **👁️ Modalità Focus / Nascondi Menu (Zen Mode):**
   * Tasto **`▲ Nascondi Menu`** o **`Ctrl + M`** (oppure `F1`) per nascondere tutti i menu superiori e visualizzare **solo le pagine web a tutto schermo**.
   * Mini-barra a scomparsa con il tasto **`▼ Mostra Menu (Ctrl+M)`** per ripristinare i menu con un clic.
6. **🖥️ Schermo Intero Immersivo (`F11`):**
   * Passa alla visualizzazione a schermo intero senza bordi né barre con un solo tasto.
7. **👻 Modalità Stealth (Invisibile nella Barra Applicazioni):**
   * L'applicazione non compare nella barra di Windows in basso; è accessibile da monitor e da System Tray.
8. **📂 Visualizzatore Documenti PDF & File Locali:**
   * Apri PDF o immagini dal tasto dedicato o semplicemente trascinandoli dentro la finestra (Drag-and-Drop).
9. **🚀 Centro Web Apps con 1 Clic:**
   * Pulsanti rapidi per **ChatGPT**, **Gemini**, **WhatsApp Web**, **Telegram**, **Wikipedia**, **Calcolatrice**, ecc.

---

## 🛠️ Come Avviare o Ricompilare

* **Eseguibile già pronto:** Avvia direttamente `PrivateViewer.exe`.
* **Ricompilare:** Esegui `build.bat` (utilizza il compilatore nativo `csc.exe` già incluso in Windows e referenzia i moduli WebView2 inclusi).

---

## 📜 Licenza
Il software è protetto da clausola di attribuzione proprietaria. Leggi il file [LICENSE](https://github.com/Ilponz/PrivateViewer/blob/main/LICENSE) per i dettagli. È vietata la rimozione del watermark e dei crediti d'autore.

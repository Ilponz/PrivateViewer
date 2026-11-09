# 🛡️ PrivateViewer (Clean & Streamlined Edition)

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

## ✨ Interfaccia Pulita ed Essenziale v2.5

1. **✨ Barra Superiore Unica e Minimale:**
   * Rimossi tutti i pulsanti ingombranti dei siti web: ora l'interfaccia è pulita, spaziosa e focalizzata sulla navigazione libera.
   * Mantenuto in primo piano il pulsante essenziale **`📂 Apri File / PDF`** per aprire documenti locali, immagini e PDF protetti da cattura.
2. **🔄 Barra Schede Stile Chrome (Drag & Drop, Aggiunta, Chiusura):**
   * **Sposta e Riordina:** Clicca su qualsiasi scheda e trascinala con il mouse per cambiare l'ordine in tempo reale.
   * **Aggiungi Schede:** Clicca sul tasto permanente **`➕`** all'estremità delle schede, fai doppio clic sulla barra o usa **`Ctrl + T`**.
   * **Chiudi Schede:** Fai clic con la rotellina del mouse (tasto centrale) su una scheda, usa il tasto **`✕`** o premi **`Ctrl + W`**.
   * **Menu Tasto Destro:** Chiudi scheda, Chiudi le altre schede, Duplica scheda, Ricarica, Sposta a sinistra/destra.
3. **🖤 Barra del Titolo Nativa Nera (Dark Title Bar):**
   * La barra in alto di Windows (con titolo e pulsanti `_`, `□`, `✕`) è 100% nera con scritte e icone bianche via API DWM nativa.
4. **👁️ Modalità Focus / Nascondi Barra (Zen Mode):**
   * Tasto **`▲ Nascondi Barra`** o **`Ctrl + M`** (oppure `F1`) per nascondere la barra superiore e visualizzare **solo le schede e le pagine web a tutto spazio**.
   * Mini-barra con il tasto **`▼ Mostra Barra (Ctrl+M)`** per ripristinare i comandi con un clic.
5. **🖥️ Schermo Intero Immersivo (`F11`):**
   * Passa a schermo intero totale senza barre né bordi.
6. **👻 Modalità Stealth (Invisibile nella Barra Applicazioni):**
   * L'applicazione non compare nella barra di Windows in basso; è accessibile da monitor e da System Tray.
7. **📂 Visualizzatore Documenti PDF & File Locali:**
   * Apri PDF o immagini dal tasto dedicato o semplicemente trascinandoli dentro la finestra (Drag-and-Drop).
8. **Boss Key Globale (`ESC`):** Nasconde la finestra istantaneamente nella barra di sistema.

---

## 🛠️ Come Avviare o Ricompilare

* **Eseguibile già pronto:** Avvia direttamente `PrivateViewer.exe`.
* **Ricompilare:** Esegui `build.bat` (utilizza il compilatore nativo `csc.exe` già incluso in Windows e referenzia i moduli WebView2 inclusi).

---

## 📜 Licenza
Il software è protetto da clausola di attribuzione proprietaria. Leggi il file [LICENSE](https://github.com/Ilponz/PrivateViewer/blob/main/LICENSE) per i dettagli. È vietata la rimozione del watermark e dei crediti d'autore.

# 🛡️ PrivateViewer (Chromium Multi-Tab & Stealth Edition)

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

### Risultato:
* **Sul tuo monitor:** La finestra è completamente visibile, reattiva e utilizzabile normalmente.
* **Nei software di cattura e registrazione:** La finestra non viene disegnata o viene completamente esclusa dal Desktop Window Manager (DWM), mostrando solo lo sfondo o le finestre sottostanti.

---

## ✨ Funzionalità Avanzate

1. **Protezione Schermo Istantanea:** Attiva di default all'avvio con pulsante di stato per commutare (Attivo / Disattivo) e verificare il funzionamento.
2. **👻 Modalità Stealth (Invisibile nella Barra delle Applicazioni):**
   * L'applicazione non compare nella barra delle applicazioni di Windows in basso.
   * Rimane controllabile a schermo e tramite l'icona protetta nella System Tray (area di notifica).
   * Possibilità di attivare/disattivare la visibilità in barra con l'interruttore rapido in alto.
3. **📑 Browser Multi-Scheda Completo (Multi-Tab Chromium):**
   * Apri schede multiple contemporaneamente con il pulsante **`➕ Nuova Scheda`** o la scorciatoia **`Ctrl + T`**.
   * Chiudi la scheda corrente con **`✕`** o **`Ctrl + W`**.
   * Naviga tra le schede con **`Ctrl + Tab`**.
   * Ogni scheda ha la sua barra di navigazione indipendente (Indietro, Avanti, Ricarica/Stop, Home, URL intelligente).
   * I titoli delle schede si sincronizzano automaticamente con le pagine web aperte.
4. **📂 Visualizzatore Documenti PDF & File Locali:**
   * Apri file PDF, immagini, file HTML o note di testo tramite il pulsante **`📂 Apri PDF / File`**.
   * Supporto **Drag-and-Drop**: trascina direttamente qualsiasi file PDF o immagine dentro la finestra per aprirlo in una nuova scheda protetta.
5. **🚀 Centro Web Apps con 1 Clic:**
   * Menu rapido per avviare in schede dedicate:
     * 🤖 **ChatGPT**
     * 🧠 **Google Gemini**
     * 💬 **WhatsApp Web**
     * ✈️ **Telegram Web**
     * 📖 **Wikipedia**
     * 🧮 **Calcolatrice Scientifica**
     * 🌐 **Google Traduttore**
     * 📄 **Google Docs**
6. **Boss Key Globale (`ESC`):** Premendo `ESC` la finestra scompare all'istante nascondendosi nell'area di notifica (System Tray). Il tasto `ESC` viene intercettato anche quando il focus è dentro la pagina web o su un campo di input. Cliccando sull'icona dello scudo nella barra di sistema la finestra viene ripristinata.
7. **Note Private:** Scheda dedicata per appunti, testi o credenziali con salvataggio su file e pulizia.
8. **Opzioni Finestra:**
   * *Sempre in primo piano:* Mantiene il visualizzatore sopra le altre finestre di lavoro.
   * *Controllo Opacità:* Slider per regolare la trasparenza dal 30% al 100%.

---

## 🛠️ Come Avviare o Ricompilare

* **Eseguibile già pronto:** Avvia direttamente `PrivateViewer.exe`.
* **Ricompilare:** Esegui `build.bat` (utilizza il compilatore nativo `csc.exe` già incluso in Windows e referenzia i moduli WebView2 inclusi).

---

## 📜 Licenza
Il software è protetto da clausola di attribuzione proprietaria. Leggi il file [LICENSE](https://github.com/Ilponz/PrivateViewer/blob/main/LICENSE) per i dettagli. È vietata la rimozione del watermark e dei crediti d'autore.
